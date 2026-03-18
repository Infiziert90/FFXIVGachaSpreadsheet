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
    import {getFormattedIconId, getIconPath, getReviewUrl, HousingMaps, pad} from "$lib/utils";
    import MultiSelect, {type ObjectOption, type Option} from "svelte-multiselect";
    import {
        SimpleHousingLandSet,
        SimpleHousingMapMarker, SimpleMapMarker,
        SimpleMapSheet, SimpleWorld, SimpleWorldDCGroup
    } from "$lib/sheets/simplifiedSheets";
    import {type WorldDetail} from "$lib/paissa/paissaStruct";
    import {RequestWorld} from "$lib/paissa/paissaRequest";
    import PageSidebar from "../../component/PageSidebar.svelte";
    import {createOpenPlot, getPhaseOrBids, getPurchaseType, type OpenPlot} from "$lib/paissa/paissaUtils";
    import {Input} from "@sveltestrap/sveltestrap";
    import {currentWorld} from "$lib/stores/worldSelection";

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});
    let tabMonsterElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data } = $props();

    // Set default meta data
    let title = $state('Housing Ward Viewer');
    let description = $state('An overview of all housing wards and their plot allocations.');

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

    let serverOptions: ObjectOption[] = $state([]);
    let serverToId: Record<number, number> = $state({});
    let selectedWorldOption: ObjectOption = $state({label: ''});
    let serverSelectionLimited: boolean = $state(false);

    let showSmall = $state(true);
    let showMedium = $state(true);
    let showLarge = $state(true);
    let showMarkers = $state(true);

    let worldData: WorldDetail | null = $state(null);

    for (const [mapId, mainDiv] of Object.entries(HousingMaps)) {
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

        if (worldRow.Name.length < 2)
            continue;

        let idx = serverOptions.length;
        serverOptions.push({label: worldRow.Name, group: SimpleWorldDCGroup[worldRow.DataCenter].Name});

        serverToId[idx] = worldRow.RowId;
    }

    onMount(async () => {
        leaflet = await import("leaflet");

        let worldId: number = $currentWorld;

        let idx = Object.entries(serverToId).find(([key, value]) => value === worldId);
        if (idx === undefined) {
            worldId = serverToId[0];
            selectedWorldOption = serverOptions[0]
        } else {
            selectedWorldOption = serverOptions[parseInt(idx[0])];
        }

        let rateLimitPromise = limitServerSelection();
        worldData = await RequestWorld(worldId);
        changeMapSelection(selectOptionId);

        // await map rebuild
        await tick();

        createMarkers(selectedMap);
        await rateLimitPromise;
    })

    function createMap(container) {
        leaflet.CRS.XY = leaflet.Util.extend({}, leaflet.CRS.Simple, {
            code: 'XY',
            projection: leaflet.Projection.LonLat,
            transformation: new leaflet.Transformation(1, 0, 1, 0)
        });

        let boundMaxCoord = convertSizeFactorToMapMaxCoord(selectedMap);
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
        let maxBounds = new leaflet.LatLngBounds( [-20, -20], [boundMaxCoord + 20, boundMaxCoord + 20]);
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
            map = createMap(container);

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
                    openPlots.push(createOpenPlot(openBid));
            }

            let size;
            let houseSet = SimpleHousingLandSet[getDistrict(mapId)].Sets[mapMarkerSubRow.RowId];
            switch (houseSet.PlotSize) {
                case 0:
                    size = 'Small';

                    if (!showSmall)
                        continue;
                    break;
                case 1:
                    size = 'Medium';

                    if (!showMedium)
                        continue;
                    break;
                case 2:
                    size = 'Large';

                    if (!showLarge)
                        continue;
                    break;
            }

            let marker;
            if (openPlots.length !== 0) {
                marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: bidIconMarker}).addTo(map);

                let text = `${size} ${houseSet.InitialPrice.toLocaleString()}
                             <br>
                             Review & Pictures: <a href="${getReviewUrl(mapId, mapMarkerSubRow.RowId)}" target="_blank">GameTora</a>
                             <br><br>
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
                    text += `
                              <tr>
                                <td class="py-0">${getPurchaseType(plot.Tenant)}</td>
                                <td class="py-0">${pad(plot.Ward + 1, 2)}</td>
                                <td class="py-0">${pad(plot.Plot + 1, 2)}</td>
                                <td class="py-0">${getPhaseOrBids(plot)}</td>
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

        if (!showMarkers)
            return;

        let mapMarkerRow = SimpleMapMarker[mapRow.MapMarkerRange];
        for (const mapMarkerSubRow of Object.values(mapMarkerRow)) {
            let ingameCoords = convertSheetToMapCoord(mapMarkerSubRow, mapRow.SizeFactor);
            let coords = swapCoords(ingameCoords);

            if (mapMarkerSubRow.Icon !== 0) {
                let iconUrl = getIconPath(getFormattedIconId(mapMarkerSubRow.Icon));
                let iconMarker = leaflet.icon({
                    iconUrl: iconUrl,

                    iconSize:     [32, 32], // size of the icon
                    popupAnchor:  [0, -20] // point from which the popup should open relative to the iconAnchor
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

    async function serverOptionChanged(payload: {type: 'add' | 'remove' | 'removeAll' | 'selectAll' | 'reorder', option: ObjectOption}) {
        if (payload.type === 'selectAll' || payload.type === 'selectAll' || payload.type === 'reorder' || payload.type === 'removeAll' || payload.type === 'remove')
            return;

        let optionIndex = serverOptions.indexOf(payload.option);
        if (optionIndex === -1) {
            console.error(`Option ${payload.option} not found in options array`);
            return;
        }

        let rateLimitPromise = limitServerSelection();
        await changeServerSelection(serverToId[optionIndex]);
        createMarkers(selectedMap);

        await rateLimitPromise;
    }

    function changeMapSelection(mapId: number) {
        let mapRow = SimpleMapSheet[mapId];

        selectedMap = mapId;
        selectedMapId.name = mapRow.Id;
        resolvedMapUrl = `https://v2.xivapi.com/api/asset/map/${selectedMapId.name}`
    }

    async function changeServerSelection(serverId: number) {
        currentWorld.set(serverId);
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

    async function limitServerSelection() {
        serverSelectionLimited = true;
        await new Promise(_ => setTimeout(_ => serverSelectionLimited = false, 10_000)); // Wait 10s before allowing another server change
    }

    /**
     * User checked one of the checkboxes so we redraw all markers.
     */
    function showStateChanged() {
        createMarkers(selectedMap);
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
                bind:value={selectedWorldOption}
                options={serverOptions}
                ulSelectedClass="multiSelect-selection"
                ulOptionsStyle="padding-left:0.5rem;"
                placeholder="Select a server"
                onchange={serverOptionChanged}
                maxSelect={1}
                minSelect={1}
                required={true}
                portal={{ active: true }}
                disabled={serverSelectionLimited}
                disabledInputTitle="Disabled for 10 seconds"
        />

        <MultiSelect
                bind:value={selectedOption}
                options={nameOptions}
                ulSelectedClass="multiSelect-selection"
                ulOptionsStyle="padding-left:0.5rem;"
                placeholder="Select a housing area"
                onchange={optionChanged}
                maxSelect={1}
                minSelect={1}
                required={true}
                portal={{ active: true }}
        />

        <h5 class="mt-3">Options:</h5>
        <Input class="mb-0" type="checkbox" bind:checked={showSmall} label="Show Small Plots" on:change={showStateChanged}></Input>
        <Input class="mb-0" type="checkbox" bind:checked={showMedium} label="Show Medium Plots" on:change={showStateChanged}></Input>
        <Input class="mb-0" type="checkbox" bind:checked={showLarge} label="Show Large Plots" on:change={showStateChanged}></Input>
        <Input class="mb-0" type="checkbox" bind:checked={showMarkers} label="Show Map Markers" on:change={showStateChanged}></Input>
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