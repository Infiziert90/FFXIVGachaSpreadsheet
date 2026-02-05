<script lang="ts">
    import {onMount, tick} from "svelte";
    import {
        convertSheetToMapCoord,
        convertSizeFactorToMapMaxCoord,
        convertToMapCoords,
        type SimpleCoords,
        swapCoords
    } from "$lib/coordHelper";
    import {Vector3} from "$lib/math/vector3";
    import StackedSidebar from "../../component/StackedSidebar.svelte";
    import {getFormattedIconId, getIconPath} from "$lib/utils";
    import MultiSelect, {type Option} from "svelte-multiselect";
    import {
        SimpleHousingLandSet,
        SimpleHousingMapMarker, SimpleMapMarker,
        SimpleMapSheet, SimpleWorld
    } from "$lib/sheets/simplifiedSheets";
    import type {WorldDetail} from "$lib/paissa/paissaStruct";
    import {RequestWorld} from "$lib/paissa/paissaRequest";

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});
    let tabMonsterElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data } = $props();

    const housingWards: Record<number, boolean> = {
        72: true,
        192: false,

        82: true,
        193: false,

        83: true,
        194: false,

        364: true,
        365: false,

        679: true,
        680: false,
    }

    let leaflet;

    let selectedId = $state(0);
    let selectedMap = $state(0);
    let selectedMapId = $state({name: ''});
    let resolvedMapUrl = $state('');

    let map;
    let position;

    let names: Record<number, number[]> = $state({});

    let nameOptions: string[] = $state([]);
    let optionsToId: Record<number, number> = $state({});
    let selectedOption: Option = $state('' as Option);
    let selectOptionId = $state(0);

    let serverOptions: string[] = $state([]);
    let serverToId: Record<number, number> = $state({});
    let selectedServerOption: Option = $state('' as Option);
    let selectServerId = $state(0);

    let wardOptions: string[] = $state([]);
    let wardToId: Record<number, number> = $state({});
    let selectedWardOption: Option = $state('' as Option);
    let selectWardId = $state(0);

    let worldData: WorldDetail | null = $state(null);

    for (const mapId of Object.keys(housingWards)) {
        let idx = nameOptions.length;
        nameOptions.push(SimpleMapSheet[parseInt(mapId)].PlaceNameSub.Name);

        optionsToId[idx] = parseInt(mapId);
    }
    selectedOption = nameOptions[0];
    selectOptionId = optionsToId[0];


    for (const worldRow of Object.values(SimpleWorld)) {
        if (!worldRow.IsPublic)
            continue;

        let idx = serverOptions.length;
        serverOptions.push(worldRow.Name);

        serverToId[idx] = worldRow.RowId;
    }

    selectedServerOption = serverOptions[0];
    selectServerId = serverToId[0];

    function fillWards() {
        wardOptions = [];
        wardToId = {};
        if (worldData === null)
            return;

        let isMainDivision = housingWards[selectedMap];
        for (const plot of Object.values(worldData.districts[getDistrict(selectedMap)].open_plots)) {
            if (isMainDivision) {
                if (plot.plot_number > 29)
                    continue;
            } else {
                if (plot.plot_number < 30)
                    continue;
            }

            let wardName = `Ward ${plot.ward_number + 1}`;
            if (wardOptions.includes(wardName))
                continue;

            let idx = wardOptions.length;
            wardOptions.push(wardName);

            wardToId[idx] = plot.ward_number;
        }

        // Reset the default
        selectedWardOption = wardOptions[0];
        selectWardId = wardToId[0];
    }

    onMount(async () => {
        leaflet = await import("leaflet");

        worldData = await RequestWorld(selectServerId);
        changeMapSelection(selectOptionId);

        // await map rebuild
        await tick();
        createMarkers(selectedMap);
    })

    function createMap(container) {
        leaflet.CRS.XY = leaflet.Util.extend({}, leaflet.CRS.Simple, {
            code: 'XY',
            projection: leaflet.Projection.LonLat,
            transformation: new leaflet.Transformation(1, 0, 1, 0)
        });

        let boundMaxCoord = convertSizeFactorToMapMaxCoord(selectedMap);
        console.log(`Bound max coord: ${boundMaxCoord}`);
        let m = leaflet.map(container, {
            minZoom: 4.5,
            maxZoom: 10.0,
            center: [boundMaxCoord / 2, boundMaxCoord / 2],
            zoom: 6.5,
            zoomSnap: 0.5,
            crs: leaflet.CRS.XY,
            wheelPxPerZoomLevel: 200,
        });

        let bounds = new leaflet.LatLngBounds( [1, 1], [boundMaxCoord, boundMaxCoord]);
        let maxBounds = new leaflet.LatLngBounds( [-5, -5], [boundMaxCoord + 10, boundMaxCoord + 10]);
        leaflet.imageOverlay(
            resolvedMapUrl,
            bounds
        ).addTo(m);

        m.setMaxBounds(maxBounds);
        console.log('Map created')

        return m;
    }

    function resizeMap() {
        if(map) { map.invalidateSize(); }
    }

    function mapAction(container) {
        $effect(() => {
            console.log('Triggering mapAction')
            map = createMap(container);
            console.log('Map assigned')

            let Position = leaflet.Control.extend({
                _container: null,
                options: {
                    position: 'bottomleft'
                },

                onAdd: function (map) {
                    let latlng = leaflet.DomUtil.create('div', 'mouseposition');
                    this.latlngPreviewElement = latlng;
                    return latlng;
                },

                updateHTML: function(coords: SimpleCoords) {
                    coords = swapCoords(coords);
                    this.latlngPreviewElement.innerHTML = `
                    <div class="text-bg-secondary p-2">
                        <h6 class="m-0">Coords: ${coords.X.toFixed(1)} ${coords.Y.toFixed(1)}</h6>
                    </div>`;
                }
            });
            position = new Position();
            map.addControl(position);

            map.addEventListener('mousemove', (event) => {
                let lat = Math.round(event.latlng.lat * 100000) / 100000;
                let lng = Math.round(event.latlng.lng * 100000) / 100000;
                position.updateHTML({X: lat, Y: lng});
            });

            return () => {
                map.remove();
                map = null;
            }
        });
    }

    let createdMarkersDict: Record<number, object> = {};
    function clearMarkers() {
        for (const marker of Object.values(createdMarkersDict)) {
            map.removeLayer(marker);
        }

        createdMarkersDict = {};
    }

    function createMarkers(mapId: number) {
        if (map === undefined)
            return;

        // Always clear markers before replacing them
        clearMarkers();

        const smallHouseIconUrl = getIconPath(getFormattedIconId(60754));
        const smallHouseIconMarker = leaflet.icon({
            iconUrl: smallHouseIconUrl,

            iconSize:     [24, 24], // size of the icon
            popupAnchor:  [0, -48] // point from which the popup should open relative to the iconAnchor
        });

        const mediumHouseIconUrl = getIconPath(getFormattedIconId(60755));
        const mediumHouseIconMarker = leaflet.icon({
            iconUrl: mediumHouseIconUrl,

            iconSize:     [28, 28], // size of the icon
            popupAnchor:  [0, -48] // point from which the popup should open relative to the iconAnchor
        });

        const largeHouseIconUrl = getIconPath(getFormattedIconId(60756));
        const largeHouseIconMarker = leaflet.icon({
            iconUrl: largeHouseIconUrl,

            iconSize:     [32, 32], // size of the icon
            popupAnchor:  [0, -48] // point from which the popup should open relative to the iconAnchor
        });

        const bidIconUrl = getIconPath(getFormattedIconId(60758));
        const bidIconMarker = leaflet.icon({
            iconUrl: bidIconUrl,

            iconSize:     [32, 32], // size of the icon
            popupAnchor:  [0, -48] // point from which the popup should open relative to the iconAnchor
        });

        let mapRow = SimpleMapSheet[mapId];
        let housingMapMarkerRow = SimpleHousingMapMarker[mapRow.Territory];
        for (const mapMarkerSubRow of Object.values(housingMapMarkerRow)) {
            if (mapMarkerSubRow.RowId > 59)
                continue;

            // Main
            if (housingWards[mapId]) {
                if (mapMarkerSubRow.RowId > 29)
                    continue;
            } else { // SubRow
                if (mapMarkerSubRow.RowId < 30)
                    continue;
            }

            let location = new Vector3(mapMarkerSubRow.X, mapMarkerSubRow.Y, mapMarkerSubRow.Z);
            let ingameCoords = convertToMapCoords(location, mapId);
            let coords = swapCoords(ingameCoords);

            let houseType = SimpleHousingLandSet[getDistrict(mapId)].Sets[mapMarkerSubRow.RowId].PlotSize;
            let hasBid = false;
            for (const openBid of Object.values(worldData.districts[getDistrict(mapId)].open_plots)) {
                if (openBid.ward_number !== selectWardId)
                    continue;

                if (openBid.plot_number === mapMarkerSubRow.RowId)
                    hasBid = true;
            }

            let iconUsed = hasBid
                ? bidIconMarker : houseType === 0
                    ? smallHouseIconMarker : houseType === 1
                        ? mediumHouseIconMarker : largeHouseIconMarker;

            let marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: iconUsed}).addTo(map);
            marker.bindPopup(`X: ${ingameCoords.X.toFixed(2)} Y: ${ingameCoords.Y.toFixed(2)}`);

            createdMarkersDict[mapMarkerSubRow.RowId] = marker;
        }

        let mapMarkerRow = SimpleMapMarker[mapRow.MapMarkerRange];
        for (const mapMarkerSubRow of Object.values(mapMarkerRow)) {
            let ingameCoords = convertSheetToMapCoord(mapMarkerSubRow, mapRow.SizeFactor);
            let coords = swapCoords(ingameCoords);

            if (mapMarkerSubRow.Icon !== 0) {
                let iconUrl = getIconPath(getFormattedIconId(mapMarkerSubRow.Icon));
                let iconMarker = leaflet.icon({
                    iconUrl: iconUrl,

                    iconSize:     [32, 32], // size of the icon
                    popupAnchor:  [0, -48] // point from which the popup should open relative to the iconAnchor
                });

                let marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: iconMarker}).addTo(map);
                marker.bindPopup(`X: ${ingameCoords.X.toFixed(2)} Y: ${ingameCoords.Y.toFixed(2)}<br>Name: ${mapMarkerSubRow.PlaceNameSubtext.Name}`);

                createdMarkersDict[mapMarkerSubRow.RowId + 1_000_000] = marker;
            }

            if (mapMarkerSubRow.PlaceNameSubtext.RowId !== 0) {
                let text = mapMarkerSubRow.PlaceNameSubtext.Name.replace("\r\n", "\n");

                let cssText = 'text-shadow: -2px 0 black, 0 2px black, 2px 0 black, 0 -2px black; width:300px;'

                let tmpElem = document.createElement('h6');
                tmpElem.style.cssText = cssText + 'visibility:hidden;';
                tmpElem.textContent = text;
                document.body.appendChild(tmpElem);

                let height = tmpElem.offsetHeight;
                let width = tmpElem.offsetWidth;
                let fontSize = parseFloat(window.getComputedStyle(tmpElem).fontSize);

                document.body.removeChild(tmpElem);

                let anchorX, anchorY;

                switch (mapMarkerSubRow.SubtextOrientation) {
                    case 2: // right of point
                        anchorX = -fontSize;
                        anchorY = height / 2;
                        break;
                    case 4: // above point
                        anchorX = width / 2;
                        anchorY = height;
                        break;
                    case 3: // below point
                        anchorX = width / 2;
                        anchorY = -height / 2;
                        break;
                    case 1: // left of point
                        anchorX = width - fontSize;
                        anchorY = height / 2;
                        break;
                    default: // centered
                        anchorX = width / 2;
                        anchorY = height / 2;
                        break;
                }

                let divIcon = leaflet.divIcon({
                    html: `<h6 style="${cssText}">${text}</h6>`,
                    className: 'transparent-divIcon',
                    iconSize: [width, height],
                    iconAnchor: [anchorX, anchorY]
                })

                let marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: divIcon}).addTo(map);
                marker.bindPopup(`X: ${ingameCoords.X.toFixed(2)} Y: ${ingameCoords.Y.toFixed(2)}<br>Name: ${mapMarkerSubRow.PlaceNameSubtext.Name}`);

                createdMarkersDict[mapMarkerSubRow.RowId + 2_000_000] = marker;
            }
        }
    }

    // Set default meta data
    let title = $state('Open Housing Plot Viewer');
    let description = $state('An overview of all housing wards and their open plots.');

    async function optionChanged(payload: {type: 'add' | 'remove' | 'removeAll' | 'selectAll' | 'reorder', option: Option}) {
        if (payload.type === 'selectAll' || payload.type === 'selectAll' || payload.type === 'reorder' || payload.type === 'removeAll' || payload.type === 'remove')
            return;

        let optionIndex = nameOptions.indexOf(payload.option.toString());
        if (optionIndex === -1) {
            console.error(`Option ${payload.option} not found in options array`);
            return;
        }

        changeMapSelection(optionsToId[optionIndex]);

        // Await the map rebuild before placing markers
        await tick();
        createMarkers(selectedMap);
    }

    async function serverOptionChanged(payload: {type: 'add' | 'remove' | 'removeAll' | 'selectAll' | 'reorder', option: Option}) {
        if (payload.type === 'selectAll' || payload.type === 'selectAll' || payload.type === 'reorder' || payload.type === 'removeAll' || payload.type === 'remove')
            return;

        let optionIndex = serverOptions.indexOf(payload.option.toString());
        if (optionIndex === -1) {
            console.error(`Option ${payload.option} not found in options array`);
            return;
        }

        await changeServerSelection(serverToId[optionIndex]);
        createMarkers(selectedMap);
    }

    async function wardOptionChanged(payload: {type: 'add' | 'remove' | 'removeAll' | 'selectAll' | 'reorder', option: Option}) {
        if (payload.type === 'selectAll' || payload.type === 'selectAll' || payload.type === 'reorder' || payload.type === 'removeAll' || payload.type === 'remove')
            return;

        let optionIndex = wardOptions.indexOf(payload.option.toString());
        if (optionIndex === -1) {
            console.error(`Option ${payload.option} not found in options array`);
            return;
        }

        selectWardId = wardToId[optionIndex];
        createMarkers(selectedMap);
    }

    function changeMapSelection(mapId: number) {
        console.log(`Changing map selection to ${mapId}`);
        let mapRow = SimpleMapSheet[mapId];

        selectedMap = mapId;
        selectedMapId.name = mapRow.Id;
        resolvedMapUrl = `https://v2.xivapi.com/api/asset/map/${selectedMapId.name}`

        fillWards()
    }

    async function changeServerSelection(serverId: number) {
        worldData = await RequestWorld(serverId);
        fillWards()
    }

    function getDistrict(mapId: number) {
        let district = 0;

        switch (mapId) {
            case 72 || 192: district = 0; break;
            case 82 || 193: district = 1; break;
            case 83 || 194: district = 2; break;
            case 364 || 365: district = 3; break;
            case 679 || 680: district = 4; break;
        }

        return district
    }
</script>
<svelte:window on:resize={resizeMap} />

<svelte:head>
    <title>{title}</title>

    <meta property="og:site_name" content={title}>
    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>

<StackedSidebar>
    <div class="container p-0">
        <MultiSelect
                bind:value={selectedOption}
                options={nameOptions}
                ulSelectedStyle="width: 85%;"
                ulOptionsStyle="background-color: var(--bs-body-bg);"
                placeholder="Select a housing area"
                onchange={optionChanged}
                maxSelect={1}
                required={true}
        />

        <MultiSelect
                bind:value={selectedServerOption}
                options={serverOptions}
                ulSelectedStyle="width: 85%;"
                ulOptionsStyle="background-color: var(--bs-body-bg);"
                placeholder="Select a server"
                onchange={serverOptionChanged}
                maxSelect={1}
                required={true}
        />

        <MultiSelect
                bind:value={selectedWardOption}
                options={wardOptions}
                ulSelectedStyle="width: 85%;"
                ulOptionsStyle="background-color: var(--bs-body-bg);"
                placeholder="Select a ward"
                onchange={wardOptionChanged}
                maxSelect={1}
                required={true}
        />
    </div>
</StackedSidebar>
<div class="col-12 col-lg-10 order-0 order-lg-2">
    <h1 class="text-center">Work in Progress, feedback and ideas welcome</h1>
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if selectedMapId.name !== ''}
            <div class="map" style="height:1024px;width:1024px" use:mapAction />
        {/if}
    </div>
</div>
