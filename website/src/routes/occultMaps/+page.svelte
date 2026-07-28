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
    import {getLotteryPhase, getNextPhaseLeftover, getNextPhaseStart, getPhaseName} from "$lib/time/lotteryPhase";
    import {currentRateLimit} from "$lib/stores/rateLimit";

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data } = $props();

    // Set default meta data
    let title = $state('Open Plot Viewer');
    let description = $state('An overview of all housing wards and their plot allocations.');

    let leaflet;

    let selectedMap = $state(0);
    let showMarkers = $state(true);
    let resolvedMapUrl = $state(`https://v2.xivapi.com/api/asset/map/o6b2/01`);
    // let resolvedMapUrl = $state(`https://v2.xivapi.com/api/asset/map/o6b2/17`);
    let isLoaded = $state(false);

    let map;
    let position;

    let names: Record<number, number[]> = $state({});

    // Define the tuple type representing each entry in the array
    type TreasureData = [object, number, number];

    // The converted dataset map
    const territoryNorthHorn: TreasureData[] = [
            [{ x: -857.4, y: 71.45287, z: 379.6 }, 0, 0],
            [{ x: 7.60699, y: 4.3169565, z: -35.67316 }, 0, 0],
            [{ x: 287.2872, y: 142.99992, z: -366.9024 }, 0, 0],
            [{ x: -608.8, y: 59.286507, z: 373.9 }, 0, 0],
            [{ x: -254, y: 54.388798, z: -739 }, 0, 0],
            [{ x: -560.9, y: 50.74249, z: -447 }, 0, 0],
            [{ x: -500, y: 48.000004, z: -867.6 }, 0, 0],
            [{ x: 226, y: 90.400055, z: 904 }, 0, 0],
            [{ x: -258.7481, y: 3.588304, z: 53.59217 }, 0, 0],
            [{ x: -604, y: 160.05638, z: 939.1 }, 0, 0],
            [{ x: 756.858, y: 68.92707, z: -79.33746 }, 0, 0],
            [{ x: -814.6948, y: 5.6813054, z: -561.0853 }, 0, 0],
            [{ x: -129.7795, y: 8.029996, z: -171.18 }, 0, 0],
            [{ x: -847.9, y: 114, z: 196.6 }, 0, 0],
            [{ x: -808, y: 6.3495464, z: -879 }, 0, 0],
            [{ x: 960, y: 97.05797, z: -879 }, 0, 0],
            [{ x: 625.8, y: 61.06923, z: -846.3 }, 0, 0],
            [{ x: -956.1, y: 157.8, z: 720.2 }, 0, 0],
            [{ x: -581, y: 160, z: 791 }, 0, 0],
            [{ x: 108, y: 22.332209, z: -556 }, 0, 0],
            [{ x: -35, y: 72.89336, z: -860 }, 0, 0],
            [{ x: 882.1526, y: 53.999996, z: 115.9092 }, 0, 0],
            [{ x: 923, y: 80.26997, z: -277 }, 0, 0],
            [{ x: 853.9, y: 70.20017, z: -343.3 }, 0, 0],
            [{ x: -124, y: 76.75548, z: 777 }, 0, 0],

    ];

    onMount(async () => {
        leaflet = await import("leaflet");

        await tick();

        isLoaded = true;
    })

    function createMap(container) {
        leaflet.CRS.XY = leaflet.Util.extend({}, leaflet.CRS.Simple, {
            code: 'XY',
            projection: leaflet.Projection.LonLat,
            transformation: new leaflet.Transformation(1, 0, 1, 0)
        });

        let boundMaxCoord = convertSizeFactorToMapMaxCoord(selectedMap);
        let m = leaflet.map(container, {
            minZoom: 2.5,
            maxZoom: 20.0,
            center: [boundMaxCoord / 2, boundMaxCoord / 2],
            zoom: 6.5,
            zoomSnap: 0.5,
            crs: leaflet.CRS.XY,
            wheelPxPerZoomLevel: 50,
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

    function clearMarkers() {
        for (const marker of Object.values(createdMarkersDict)) {
            map.removeLayer(marker);
        }

        createdMarkersDict = {};
    }

    function createMarkers(mapId: number) {
        if (map === undefined)
            return;

        const carrot = leaflet.icon({
            iconUrl: getIconPath(getFormattedIconId(25207)),

            iconSize:     [32, 32], // size of the icon
            popupAnchor:  [0, -20] // point from which the popup should open relative to the iconAnchor
        });

        const bronze = leaflet.icon({
            iconUrl: getIconPath(getFormattedIconId(60356)),

            iconSize:     [32, 32], // size of the icon
            popupAnchor:  [0, -20] // point from which the popup should open relative to the iconAnchor
        });

        const silver = leaflet.icon({
            iconUrl: getIconPath(getFormattedIconId(60355)),

            iconSize:     [32, 32], // size of the icon
            popupAnchor:  [0, -20] // point from which the popup should open relative to the iconAnchor
        });

        // Always clear markers before replacing them
        clearMarkers();

        for (const row of territoryNorthHorn) {
            // console.log(row);
            // if (row[2] !== 1244)
            //     continue;

            let location = new Vector3(row[0].x, row[0].y, row[0].z);
            let ingameCoords = convertToMapCoords(location, mapId);
            let coords = swapCoords(ingameCoords);
            let marker;

            // marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: row[1] === 1597 ? silver : bronze}).addTo(map);
            marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: carrot}).addTo(map);
            marker.bindPopup(`X: ${ingameCoords.X.toFixed(2)} Y: ${ingameCoords.Y.toFixed(2)}`);

            createdMarkersDict[1] = marker;
        }

        if (!showMarkers)
            return;

        let mapRow = SimpleMapSheet[mapId];
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
        }
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

    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>

<div class="col-12 col-lg-12 order-0 order-lg-2 justify-content-center">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if isLoaded}
            <Input class="mb-0" type="checkbox" bind:checked={showMarkers} label="Show Map Markers" on:change={showStateChanged}></Input>
            <button on:click={() => createMarkers(1135)}>X</button>
<!--            <button on:click={() => createMarkers(1244)}>X</button>-->
            <div class="map" style="height:1024px" use:mapAction />
        {/if}
    </div>
</div>