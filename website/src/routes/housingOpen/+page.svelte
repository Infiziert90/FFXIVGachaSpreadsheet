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
    import {getFormattedIconId, getIconPath, pad} from "$lib/utils";
    import MultiSelect, {type Option} from "svelte-multiselect";
    import {
        SimpleHousingLandSet,
        SimpleHousingMapMarker, SimpleMapMarker,
        SimpleMapSheet, SimpleWorld
    } from "$lib/sheets/simplifiedSheets";
    import {PurchaseSystem, type WorldDetail} from "$lib/paissa/paissaStruct";
    import {RequestWorld} from "$lib/paissa/paissaRequest";
    import PageSidebar from "../../component/PageSidebar.svelte";
    import {getPurchaseType} from "$lib/paissa/paissaUtils";

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

    let worldData: WorldDetail | null = $state(null);

    for (const [mapId, mainDiv] of Object.entries(housingWards)) {
        if (!mainDiv)
            continue;

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

            // Re-run text label visibility when the user zooms in/out
            map.on('zoomend', updateTextMarkersVisibility);

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

    // Map markers keyed by RowId (housing), RowId+1e6 (icons), RowId+2e6 (text labels)
    let createdMarkersDict: Record<number, object> = {};
    // Zoom level at which text labels become visible. dataType1 = link-to-region (light blue) labels.
    const TEXT_MARKER_MIN_ZOOM = { default: 6.5, dataType1: 5 };
    let textMarkersByMinZoom: { marker: object; minZoom: number }[] = [];

    function clearMarkers() {
        for (const marker of Object.values(createdMarkersDict)) {
            map.removeLayer(marker);
        }

        createdMarkersDict = {};
    }

    interface OpenPlot {
        Plot: number;
        Ward: number;
        Type: number;
        Tenant: PurchaseSystem;
        Bids: number | null;
    }

    function createMarkers(mapId: number) {
        if (map === undefined)
            return;

        const bidIconMarker = leaflet.icon({
            iconUrl: getIconPath(getFormattedIconId(60758)),

            iconSize:     [32, 32], // size of the icon
            popupAnchor:  [0, -20] // point from which the popup should open relative to the iconAnchor
        });

        const redXMarker = leaflet.icon({
            iconUrl: getIconPath(getFormattedIconId(61502)),

            iconSize:     [32, 32], // size of the icon
            popupAnchor:  [0, -20] // point from which the popup should open relative to the iconAnchor
        });

        // Always clear markers before replacing them
        clearMarkers();

        let mapRow = SimpleMapSheet[mapId];
        let housingMapMarkerRow = SimpleHousingMapMarker[mapRow.Territory];
        for (const mapMarkerSubRow of Object.values(housingMapMarkerRow)) {
            if (mapMarkerSubRow.RowId > 29)
                continue

            let location = new Vector3(mapMarkerSubRow.X, mapMarkerSubRow.Y, mapMarkerSubRow.Z);
            let ingameCoords = convertToMapCoords(location, mapId);
            let coords = swapCoords(ingameCoords);

            let openPlots: OpenPlot[] = [];
            for (const openBid of Object.values(worldData.districts[getDistrict(mapId)].open_plots)) {
                if (openBid.plot_number === mapMarkerSubRow.RowId || openBid.plot_number === mapMarkerSubRow.RowId + 30)
                    openPlots.push({Plot: openBid.plot_number, Ward: openBid.ward_number, Type: openBid.size, Tenant: openBid.purchase_system, Bids: openBid.lotto_entries});
            }

            let marker;
            if (openPlots.length !== 0) {
                marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: bidIconMarker}).addTo(map);

                let houseSet = SimpleHousingLandSet[getDistrict(mapId)].Sets[mapMarkerSubRow.RowId];
                let size = houseSet.PlotSize === 0
                    ? 'Small' : houseSet.PlotSize === 1
                        ? 'Medium' : 'Large';


                let text = `${size} ${houseSet.InitialPrice.toLocaleString()}<br><br>
                             <table class="table table-light">
                             <thead>
                              <tr>
                                <th>Tenant</th>
                                <th>Ward</th>
                                <th>Plot</th>
                                <th>Bids</th>
                              </tr>
                            </thead>
                            <tbody>`;
                for (const plot of openPlots) {
                    // text += `[${getPurchaseType(plot.Tenant)}]&nbsp;&nbsp;Ward: ${pad(plot.Ward + 1, 2)} Plot: ${pad(plot.Plot + 1, 2)} Bids: ${plot.Bids ?? 'Missing Data'}<br>`
                    text += `
                              <tr>
                                <td class="py-0">${getPurchaseType(plot.Tenant)}</td>
                                <td class="py-0">${pad(plot.Ward + 1, 2)}</td>
                                <td class="py-0">${pad(plot.Plot + 1, 2)}</td>
                                <td class="py-0">${plot.Bids ?? 'Missing Data'}</td>
                              </tr>`
                }
                text += `</tbody></table>`;

                marker.bindPopup(text);
            }
            else {
                marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: redXMarker}).addTo(map);
                marker.bindPopup(`No Plot Available`);
            }

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
                // If mapMarkerSubRow.DataType is 1, then it's a link to another region - color it light blue
                let textColor = mapMarkerSubRow.DataType === 1 ? '#9CD8DE' : 'white';

                let text = mapMarkerSubRow.PlaceNameSubtext.Name.replace("\r\n", "\n");
                let fontSize = 14;
                let cssText = /*css*/`
                    font-size: ${fontSize}px;
                    width:fit-content;
                    text-wrap: nowrap;
                    font-family: var(--bs-body-font-family);
                    color: ${textColor};
                    text-shadow: 0px 0px 2px black, 0px 0px 3px black, 0px 0px 4px black
                `;

                let tmpElem = document.createElement('h6');
                tmpElem.style.cssText = cssText + 'visibility:hidden;';
                tmpElem.textContent = text;
                document.body.appendChild(tmpElem);

                let height = tmpElem.offsetHeight;
                let width = tmpElem.offsetWidth;

                document.body.removeChild(tmpElem);

                let anchorX, anchorY;

                switch (mapMarkerSubRow.SubtextOrientation) {
                    case 2: // right of point
                        anchorX = mapMarkerSubRow.Icon !== 0 ? -(fontSize*1.5) : 0;
                        // if icon is present, offset the text to the right to avoid it being on top of the point
                        anchorY = height / 2;
                        break;
                    case 4: // above point
                        anchorX = width / 2;
                        anchorY = height * 2; // Text is always slightly above the point, further away from the point
                        break;
                    case 3: // below point
                        anchorX = width / 2;
                        anchorY = -height / 2;
                        break;
                    case 1: // left of point
                        anchorX = mapMarkerSubRow.Icon !== 0 ? width + (fontSize*1.5) : width;
                        // if icon is present, offset the text to the left to avoid it being on top of the point
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
                marker.bindPopup(`X: ${ingameCoords.X.toFixed(2)} Y: ${ingameCoords.Y.toFixed(2)}${mapMarkerSubRow.PlaceNameSubtext.Name !== '' ? `<br>Name: ${mapMarkerSubRow.PlaceNameSubtext.Name}` : ''}`);

                createdMarkersDict[mapMarkerSubRow.RowId + 2_000_000] = marker;
                // DataType 1 (region links) use lower minZoom so they stay visible when zoomed out.
                textMarkersByMinZoom.push({
                    marker,
                    minZoom: mapMarkerSubRow.DataType === 1 ? TEXT_MARKER_MIN_ZOOM.dataType1 : TEXT_MARKER_MIN_ZOOM.default
                });
            }
        }

        // Apply current zoom so new labels are shown/hidden correctly right away
        updateTextMarkersVisibility();
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

    function changeMapSelection(mapId: number) {
        console.log(`Changing map selection to ${mapId}`);
        let mapRow = SimpleMapSheet[mapId];

        selectedMap = mapId;
        selectedMapId.name = mapRow.Id;
        resolvedMapUrl = `https://v2.xivapi.com/api/asset/map/${selectedMapId.name}`
    }

    async function changeServerSelection(serverId: number) {
        worldData = await RequestWorld(serverId);
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

    /**
     * Show or hide map text labels based on the current zoom level.
     * - textMarkersByMinZoom holds every text label plus the zoom at which it should appear
     *   (e.g. region-link labels use 5, normal labels use 6.5).
     * - We read the current zoom from the map, then for each label set opacity to 1 (visible)
     *   if zoom >= that label’s minZoom, otherwise 0 (hidden). Labels stay on the map;
     *   we only toggle visibility so we don’t have to remove/add layers on zoom.
     * Called on map "zoomend" and after createMarkers() so the initial zoom is applied.
     */
    function updateTextMarkersVisibility() {
        if (!map) return;
        const zoom = map.getZoom();
        const setOpacity = (m: object, visible: boolean) => (m as { setOpacity: (n: number) => void }).setOpacity(visible ? 1 : 0);
        textMarkersByMinZoom.forEach(({ marker, minZoom }) => setOpacity(marker, zoom >= minZoom));
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

<PageSidebar title="Housing filters" colClass="col-12 col-lg-2 order-0 order-lg-1 sticky-left-col">
    <div class="d-flex flex-column gap-2 max-w-100 overflow-x-hidden">
        <MultiSelect
                bind:value={selectedOption}
                options={nameOptions}
                ulSelectedStyle="width: 85%;"
                ulOptionsStyle="background-color: var(--bs-body-bg);"
                placeholder="Select a housing area"
                onchange={optionChanged}
                maxSelect={1}
                required={true}
                portal={{ active: true }}
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
                portal={{ active: true }}
        />
    </div>
</PageSidebar>
<div class="col-12 col-lg-10 order-0 order-lg-2">
    <h1 class="text-center">Work in Progress, feedback and ideas welcome</h1>
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if selectedMapId.name !== ''}
            <div class="map" style="height:1024px;width:1024px" use:mapAction />
        {/if}
    </div>
</div>