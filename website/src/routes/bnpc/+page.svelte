<script lang="ts">
    import type {BnpcPairing, Location, Pairing, UniqueLocation} from "$lib/interfaces";
    import {onMount, tick} from "svelte";
    import MapSearchbar from "../../component/MapSearchbar.svelte";
    import {
        convertSheetToMapCoord,
        convertSizeFactorToMapMaxCoord,
        convertToMapCoords,
        type SimpleCoords,
        swapCoords
    } from "$lib/coordHelper";
    import {Vector3} from "$lib/math/vector3";
    import {getFormattedIconId, getIconPath, logAndThrow} from "$lib/utils";
    import MultiSelect, {type Option} from "svelte-multiselect";
    import {SimpleBNpcNameSheet, SimpleMapMarker, SimpleMapSheet} from "$lib/sheets/simplifiedSheets";
    import {SimpleTerritorySheet} from "$lib/sheets/simplifiedSheets.ts";
    import PageSidebar from "../../component/PageSidebar.svelte";

    interface Props {
        content: BnpcPairing;
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});
    let tabMonsterElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data }: Props = $props();
    let pairingData: BnpcPairing = data.content;

    const uniqueLocations: UniqueLocation[] = [];
    for (const pairing of Object.values(pairingData.BnpcPairings)) {
        if (pairing.Base === 0 || pairing.Name === 0)
            continue;

        for (const location of Object.values(pairing.Locations)) {
            // TODO Add checkbox to allow quest areas be searched
            if (SimpleTerritorySheet[location.Territory].QuestBattle > 0)
                continue;

            const uniqueLocation: UniqueLocation = {Territory: location.Territory, Map: location.Map};
            if (!uniqueLocations.some((e) => e.Territory === uniqueLocation.Territory && e.Map === uniqueLocation.Map))
                uniqueLocations.push(uniqueLocation);
        }
    }

    // Set default meta data
    let title = $state('Monster Locations');
    let description = $state('A searchable map with all monster locations.');

    let leaflet;

    let selectedId = $state(0);
    let selectedMapId = $state({name: ''});
    let resolvedMapUrl = $state('');
    let selectedLocation: UniqueLocation = $state({Territory: 0, Map: 0});

    let map;
    let position;

    let names: Record<number, number[]> = $state({});
    let deduplicateNames: Record<number, number> = $state({});

    let pairs: Pairing[];

    let nameOptions: string[] = $state([]);
    let optionsToId: Record<number, number> = $state({});
    let selectedOptions: Option[] = $state([]);

    function fillPairs(location: UniqueLocation) {
        selectedOptions = []; // Reset the multiselect array

        names = {};
        deduplicateNames = {};
        pairs = [];

        for (const pairing of Object.values(pairingData.BnpcPairings).filter((pair) => Object.values(pair.Locations).some((l) => l.Territory === location.Territory && l.Map === location.Map)).sort((a, b) => a.Base - b.Base)) {
            let idx = pairs.length;
            pairs.push(pairing);

            // Some names are duplicates and need to be sorted into the same Id
            let name = pairing.Name;
            let evaluatedName = SimpleBNpcNameSheet[name]['En'];
            if (evaluatedName in deduplicateNames) {
                name = deduplicateNames[evaluatedName];
            } else {
                deduplicateNames[evaluatedName] = name;
            }

            if (name in names) {
                names[name].push(idx);
                continue;
            }

            names[name] = [idx];
        }

        nameOptions = [];
        optionsToId = {};
        for (const name of Object.keys(names)) {
            let idx = nameOptions.length;
            nameOptions.push(SimpleBNpcNameSheet[name]['En'] ?? 'Unknown');

            optionsToId[idx] = parseInt(name);
        }
    }

    onMount(async () => {
        leaflet = await import("leaflet");

        await tick();

        let locationIdx = uniqueLocations.findIndex(u => u.Territory === 134 && u.Map === 15);
        if (locationIdx === -1) {
            logAndThrow("Unable to find default map.", undefined);
        }

        selectedId = locationIdx;
        await onButtonClick(uniqueLocations[locationIdx], false)
    })

    function createMap(container) {
        leaflet.CRS.XY = leaflet.Util.extend({}, leaflet.CRS.Simple, {
            code: 'XY',
            projection: leaflet.Projection.LonLat,
            transformation: new leaflet.Transformation(1, 0, 1, 0)
        });

        let boundMaxCoord = convertSizeFactorToMapMaxCoord(selectedLocation.Map);
        console.log(`Bound max coord: ${boundMaxCoord}`);
        let m = leaflet.map(container, {
            minZoom: 3.0,
            maxZoom: 10.0,
            center: [boundMaxCoord / 2, boundMaxCoord / 2],
            zoom: 4.5,
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

        return m;
    }

    function resizeMap() {
        if(map) { map.invalidateSize(); }
    }

    function mapAction(container) {
        $effect(() => {
            console.log('Triggering mapAction')
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

            map.addEventListener('mousemove', (event) => {
                let lat = Math.round(event.latlng.lat * 100000) / 100000;
                let lng = Math.round(event.latlng.lng * 100000) / 100000;
                position.updateHTML({X: lat, Y: lng});
            });

            // Re-run text label visibility when the user zooms in/out
            map.on('zoomend', updateTextMarkersVisibility);

            return () => {
                map.remove();
                map = null;
            }
        });
    }

    // Map markers keyed by RowId (housing), RowId+1e6 (icons), RowId+2e6 (text labels)
    let createdMarkersDict: Record<number, object[]> = {};
    // Zoom level at which text labels become visible. dataType1 = link-to-region (light blue) labels.
    const TEXT_MARKER_MIN_ZOOM = { default: 6.5, dataType1: 5 };
    let textMarkersByMinZoom: { marker: object; minZoom: number }[] = [];

    function clearMarker(selectedMonster: number) {
        for (const marker of createdMarkersDict[selectedMonster]) {
            map.removeLayer(marker);
        }

        createdMarkersDict[selectedMonster] = [];
    }

    function clearAllMonsterMarkers() {
        for (const id of Object.keys(createdMarkersDict)) {
            if (parseInt(id) < 1_000_000) {
                clearMarker(parseInt(id));
            }
        }
    }

    function clearAllMarkers() {
        for (const markers of Object.values(createdMarkersDict)) {
            for (const marker of markers) {
                map.removeLayer(marker);
            }
        }

        createdMarkersDict = {};
        textMarkersByMinZoom = [];
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

    function createMarkers(selectedMonster: number) {
        if (map === undefined)
            return;

        let indexes = names[selectedMonster];
        for (const idx of indexes) {
            for (const [_, location] of Object.entries(pairs[idx].Locations).filter(([_, l]) => l.Territory === selectedLocation.Territory && l.Map === selectedLocation.Map)) {
                for (const worldPos of Object.values(location.Positions)) {
                    let ingameCoords = convertToMapCoords(new Vector3(worldPos.X, worldPos.Y, worldPos.Z), location.Map);
                    let coords = swapCoords(ingameCoords);

                    const iconUrl = getIconPath(getFormattedIconId(93047));
                    const iconMarker = leaflet.icon({
                        iconUrl: iconUrl,

                        iconSize:     [48, 48], // size of the icon
                        popupAnchor:  [0, -20] // point from which the popup should open relative to the iconAnchor
                    });

                    let marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: iconMarker}).addTo(map);
                    marker.bindPopup(`
                                    ${SimpleBNpcNameSheet[pairs[idx].Name]['En']}
                                    <br>
                                    Level: ${location.Level}
                                    <br>
                                    X ${ingameCoords.X.toFixed(2)} Y ${ingameCoords.Y.toFixed(2)}
                                    <br>
                                    <br>
                                    Base: ${pairs[idx].Base}
                                    <br>
                                    Name: ${pairs[idx].Name}`);

                    if (!(selectedMonster in createdMarkersDict))
                        createdMarkersDict[selectedMonster] = [marker]
                    else
                        createdMarkersDict[selectedMonster].push(marker)
                }
            }
        }

        // Check if we already placed all map markers
        if (Object.keys(createdMarkersDict).some(key => key > 1_000_000))
            return;

        createDefaultMapMarkers();
    }

    function createDefaultMapMarkers() {
        let mapRow = SimpleMapSheet[selectedLocation.Map];
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

                createdMarkersDict[mapMarkerSubRow.RowId + 1_000_000] = [marker];
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

                createdMarkersDict[mapMarkerSubRow.RowId + 2_000_000] = [marker];
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

    async function onButtonClick(usedData: UniqueLocation, addQuery: boolean) {
        selectedMapId.name = SimpleMapSheet[usedData.Map].Id;
        resolvedMapUrl = `https://v2.xivapi.com/api/asset/map/${selectedMapId.name}`
        selectedLocation = usedData;

        fillPairs(usedData);
        clearAllMarkers();

        await tick();
        createDefaultMapMarkers();
    }

    /**
     * Called when user changes the monster selection dropdown
     */
    function nameOptionChanged(payload: {type: 'add' | 'remove' | 'removeAll' | 'selectAll' | 'reorder', option: Option}) {
        if (payload.type === 'selectAll' || payload.type === 'selectAll' ||  payload.type === 'reorder')
            return;

        if (payload.type === 'removeAll') {
            clearAllMonsterMarkers();
            return;
        }

        let optionIndex = nameOptions.indexOf(payload.option.toString());
        if (optionIndex === -1) {
            console.error(`Option ${payload.option} not found in options array`);
            return;
        }

        let selectedMonster = optionsToId[optionIndex];
        if (payload.type === 'add') {
            createMarkers(selectedMonster)
        }
        else {
            clearMarker(selectedMonster);
        }
    }

    function getMapName(selectedMap: number) {
        const map = SimpleMapSheet[selectedMap];
        return `${map.PlaceName.Name}${map.PlaceNameSub.Name.length > 1 ? ` - ${map.PlaceNameSub.Name}` : ''}`
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
        <h6>Map Selection:</h6>
        {#if selectedLocation.Map !== 0}
            <h6>{getMapName(selectedLocation.Map)}</h6>
        {/if}
        <MapSearchbar
                {uniqueLocations}
                {selectedId}
                {onButtonClick}
                {tabElements}
        />

        <div class="m-5"></div>

        {#if selectedMapId.name !== ''}
            <h6>Monster Selection:</h6>
            <MultiSelect
                    bind:selected={selectedOptions}
                    options={nameOptions}
                    ulSelectedClass="multiSelect-selection"
                    ulOptionsStyle="padding-left:0.5rem;"
                    onchange={nameOptionChanged}
                    placeholder="Select a monster"
                    portal={{ active: true }}
            />
        {/if}
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