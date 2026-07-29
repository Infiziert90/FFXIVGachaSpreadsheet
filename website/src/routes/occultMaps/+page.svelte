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
            [{ x: -455.989, y: 39.688915, z: -365.5418 }, 2072, 0],
            [{ x: 714.698, y: 69.24771, z: 262.6901 }, 2072, 0],
            [{ x: 452.6, y: 57.10005, z: -310.3 }, 2072, 0],
            [{ x: 593, y: 39.622505, z: 34 }, 2072, 0],
            [{ x: 47.6, y: 3.8843424, z: -218.3 }, 2073, 0],
            [{ x: -223.8233, y: 10.891144, z: -353.9438 }, 2072, 0],
            [{ x: 1.768392, y: 71.555756, z: -872.2798 }, 2072, 0],
            [{ x: -172.6, y: 6.0019975, z: 103.2 }, 2073, 0],
            [{ x: -747.4032, y: 28.970308, z: -492.1095 }, 2073, 0],
            [{ x: -330, y: 42, z: -628 }, 2073, 0],
            [{ x: -184.5137, y: 71.1816, z: 667.8036 }, 2073, 0],
            [{ x: -975.4507, y: 17.57744, z: -526.2878 }, 2073, 0],
            [{ x: 889.2178, y: 53.999996, z: 155.9825 }, 2073, 0],
            [{ x: -252.1626, y: 66.55432, z: -879.5855 }, 2072, 0],
            [{ x: 440.298, y: 60.615795, z: -926.5872 }, 2072, 0],
            [{ x: -512, y: 41.999996, z: -389 }, 2073, 0],
            [{ x: -269.6122, y: 107.93719, z: 875.6997 }, 2073, 0],
            [{ x: 28.10088, y: 3.9999995, z: -16.69861 }, 2073, 0],
            [{ x: -109.5452, y: 8.047999, z: -210.1855 }, 2073, 0],
            [{ x: 52, y: 25.316154, z: 552 }, 2073, 0],
            [{ x: -127, y: 71.47446, z: 808.4 }, 2073, 0],
            [{ x: 190.3622, y: 3.880325, z: -204.7095 }, 2073, 0],
            [{ x: -259.6, y: 3.6823246, z: 56.9 }, 2073, 0],
            [{ x: 912.2978, y: 61.18964, z: -461.5099 }, 2072, 0],
            [{ x: 782.4979, y: 70.34123, z: -56.4099 }, 2072, 0],
            [{ x: -190, y: 61.75258, z: -763 }, 2072, 0],
            [{ x: 939.2178, y: 80.269966, z: -273.1175 }, 2072, 0],
            [{ x: -834, y: 18.913685, z: -587.4 }, 2073, 0],
            [{ x: -86, y: 60.596237, z: -737 }, 2072, 0],
            [{ x: 32.4, y: 56.835186, z: -777.3 }, 2072, 0],
            [{ x: 948.5978, y: 63.594563, z: -567.0099 }, 2072, 0],
            [{ x: -628.4385, y: 49.07533, z: -449.5009 }, 2073, 0],
            [{ x: -15.89468, y: 4.0000005, z: -20.29277 }, 2073, 0],
            [{ x: -498.7, y: 11.051006, z: 128.9 }, 2072, 0],
            [{ x: -530, y: 67.77658, z: -58 }, 2072, 0],
            [{ x: 546.56, y: 36.120197, z: 143.3104 }, 2072, 0],
            [{ x: 927.0178, y: 54, z: -155.2175 }, 2072, 0],
            [{ x: 210, y: 98.400055, z: 916 }, 2073, 0],
            [{ x: 237.9156, y: -0.29999995, z: 309.4334 }, 2073, 0],
            [{ x: 0.9425046, y: 41.80327, z: 623.2599 }, 2073, 0],
            [{ x: -339.8588, y: 85.47024, z: 861.5197 }, 2073, 0],
            [{ x: -88.43135, y: 2.400001, z: 4.891054 }, 2073, 0],
            [{ x: 830.0979, y: 77.75924, z: -148.9099 }, 2072, 0],
            [{ x: 928.8978, y: 74.0003, z: -332.8099 }, 2072, 0],
            [{ x: 321.198, y: 59.85, z: -889.8872 }, 2072, 0],
            [{ x: -536.1014, y: 87.01824, z: 149.8447 }, 2072, 0],
            [{ x: -586.3, y: 47.81013, z: -715.2 }, 2073, 0],
            [{ x: 194.2296, y: -0.3000001, z: 352.9844 }, 2073, 0],
            [{ x: 11.98766, y: 68.15505, z: 795.707 }, 2073, 0],
            [{ x: 929.4178, y: 54, z: -1.817501 }, 2072, 0],
            [{ x: 810.8979, y: 78.39757, z: -278.8099 }, 2072, 0],
            [{ x: -251.781, y: 65.949005, z: -864.3828 }, 2072, 0],
            [{ x: 93.4, y: 3.7155468, z: -114.3 }, 2073, 0],
            [{ x: 71.10001, y: 81.074875, z: 942.3 }, 2073, 0],
            [{ x: -596, y: 41.869873, z: -285 }, 2072, 0],
            [{ x: -113.4943, y: 5.0879984, z: -74.15943 }, 2073, 0],
            [{ x: -853.493, y: 58, z: -323.8983 }, 2073, 0],
            [{ x: 151.9998, y: 61.106945, z: -842.0175 }, 2072, 0],
            [{ x: 385, y: 33, z: -177 }, 2072, 0],
            [{ x: -960, y: 48, z: -425.8 }, 2073, 0],
            [{ x: 782.8808, y: 60.390976, z: -611.7695 }, 0, 0],
            [{ x: 909, y: 97.05797, z: -961.8 }, 0, 0],
            [{ x: 925.6533, y: 70.21527, z: -906.2195 }, 0, 0],
            [{ x: -661, y: 160, z: 937 }, 0, 0],
            [{ x: -527, y: 160.1012, z: 834 }, 0, 0],
            [{ x: -631.9453, y: 160, z: 808.8979 }, 0, 0],
            [{ x: 701, y: 59.999992, z: -945 }, 0, 0],
            [{ x: -656.9, y: 23.036425, z: -799.3 }, 0, 0],
            [{ x: -809, y: 6.3495464, z: -879 }, 0, 0],
            [{ x: 671.2, y: 60.99496, z: -550.1 }, 0, 0],
            [{ x: -623, y: 160, z: 883 }, 0, 0],
            [{ x: -585, y: 160, z: 842 }, 0, 0],
            [{ x: -839.9977, y: 160, z: 740 }, 0, 0],
            [{ x: -487.8, y: 48.000015, z: -953.2 }, 0, 0],
            [{ x: -603, y: 32, z: -869 }, 0, 0],
            [{ x: -637.2283, y: 32, z: -950.4841 }, 0, 0],
            [{ x: -866, y: -41.01304, z: -775 }, 0, 0],
            [{ x: 626.3, y: 61.119125, z: -844.9 }, 0, 0],
            [{ x: 943.4631, y: 70.21487, z: -879.5159 }, 0, 0],
            [{ x: -449.6, y: 45.6567, z: -967.0001 }, 0, 0],
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

    function createMarkers(mapId: number, rarity: number) {
        if (map === undefined)
            return;

        const carrot = leaflet.icon({
            iconUrl: "carrot.png",

            iconSize:     [32, 32], // size of the icon
            popupAnchor:  [0, -20] // point from which the popup should open relative to the iconAnchor
        });

        const reroll = leaflet.icon({
            iconUrl: getIconPath(getFormattedIconId(61473)),

            iconSize:     [32, 32], // size of the icon
            popupAnchor:  [0, -20] // point from which the popup should open relative to the iconAnchor
        });

        const gold = leaflet.icon({
            iconUrl: getIconPath(getFormattedIconId(60354)),

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
            if (row[1] !== rarity)
                continue;
            // console.log(row);
            // if (row[2] !== 1244)
            //     continue;

            let location = new Vector3(row[0].x, row[0].y, row[0].z);
            let ingameCoords = convertToMapCoords(location, mapId);
            let coords = swapCoords(ingameCoords);
            let marker;

            // marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: row[1] === 1597 ? silver : bronze}).addTo(map);
            marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: rarity == 0 ? reroll : gold}).addTo(map);
            marker.bindPopup(`X: ${ingameCoords.X.toFixed(2)} Y: ${ingameCoords.Y.toFixed(2)}`);

            createdMarkersDict[coords.X] = marker;
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
        createMarkers(selectedMap, 0);
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
            <div class="btn-group" role="group" aria-label="Basic example">
                <button on:click={() => createMarkers(1135, 0)}>Reroll</button>
                <button on:click={() => createMarkers(1135, 2072)}>North</button>
                <button on:click={() => createMarkers(1135, 2073)}>South</button>
            </div>
<!--            <button on:click={() => createMarkers(1244)}>X</button>-->
            <div class="map" style="height:1024px" use:mapAction />
        {/if}
    </div>
</div>