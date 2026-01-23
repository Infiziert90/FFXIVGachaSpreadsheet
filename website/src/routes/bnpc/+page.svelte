<script lang="ts">
    import type {BnpcPairing, Location, Pairing, UniqueLocation} from "$lib/interfaces";
    import {onMount} from "svelte";
    import MapSearchbar from "../../component/MapSearchbar.svelte";
    import {MapSheet} from "$lib/sheets";
    import {convertToMapCoords, type SimpleCoords, swapCoords} from "$lib/coordHelper";
    import {Vector3} from "$lib/math/vector3";
    import {BNpcNameSheet} from "$lib/sheets.ts";
    import StackedSidebar from "../../component/StackedSidebar.svelte";
    import {getFormattedIconId, getIconPath} from "$lib/utils";
    import MultiSelect, {type Option} from "svelte-multiselect";

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
            const uniqueLocation: UniqueLocation = {Territory: location.Territory, Map: location.Map};
            if (!uniqueLocations.some((e) => e.Territory === uniqueLocation.Territory && e.Map === uniqueLocation.Map))
                uniqueLocations.push(uniqueLocation);
        }
    }

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
        names = {};
        deduplicateNames = {};
        pairs = [];

        for (const pairing of Object.values(pairingData.BnpcPairings).filter((pair) => Object.values(pair.Locations).some((l) => l.Territory === location.Territory && l.Map === location.Map)).sort((a, b) => a.Base - b.Base)) {
            let idx = pairs.length;
            pairs.push(pairing);

            // Some names are duplicates and need to be sorted into the same Id
            let name = pairing.Name;
            let evaluatedName = BNpcNameSheet[name];
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
            nameOptions.push(BNpcNameSheet[name] ?? 'Unknown');

            optionsToId[idx] = parseInt(name);
        }
    }

    onMount(async () => {
        leaflet = await import("leaflet");
    })

    function createMap(container) {
        leaflet.CRS.XY = leaflet.Util.extend({}, leaflet.CRS.Simple, {
            code: 'XY',
            projection: leaflet.Projection.LonLat,
            transformation: new leaflet.Transformation(1, 0, 1, 0)
        });
        let m = leaflet.map(container, {
            minZoom: 3.0,
            maxZoom: 8.0,
            center: [21, 21],
            zoom: 4.5,
            zoomSnap: 0.5,
            crs: leaflet.CRS.XY,
            wheelPxPerZoomLevel: 200,
        });

        let bounds = new leaflet.LatLngBounds( [1, 1], [42, 42]);
        let maxBounds = new leaflet.LatLngBounds( [-5, -5], [50, 50]);
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

            return () => {
                map.remove();
                map = null;
            }
        });
    }

    let createdMarkersDict: Record<number, object[]> = {};
    function createMarkers(selectedMonster: number) {
        let indexes = names[selectedMonster];
        for (const idx of indexes) {
            for (const [_, location] of Object.entries(pairs[idx].Locations).filter(([_, l]) => l.Territory === selectedLocation.Territory && l.Map === selectedLocation.Map)) {
                for (const worldPos of Object.values(location.Positions)) {
                    console.log(`World: `, worldPos);
                    let coords = convertToMapCoords(new Vector3(worldPos.X, worldPos.Y, worldPos.Z), location.Map);
                    console.log(`Coords: `, coords);
                    coords = swapCoords(coords);

                    const iconUrl = getIconPath(getFormattedIconId(93047));
                    const iconMarker = leaflet.icon({
                        iconUrl: iconUrl,
                        shadowUrl: iconUrl,

                        iconSize:     [64, 64], // size of the icon
                        shadowSize:   [0, 0], // size of the shadow
                        iconAnchor:   [32, 64], // point of the icon which will correspond to marker's location
                        shadowAnchor: [0, 0],  // the same for the shadow
                        popupAnchor:  [0, -48] // point from which the popup should open relative to the iconAnchor
                    });

                    let marker = leaflet.marker([coords.X, coords.Y], {draggable: false, icon: iconMarker}).addTo(map);
                    marker.bindPopup(`${BNpcNameSheet[pairs[idx].Name]}<br>Level: ${location.Level}`);

                    if (!(selectedMonster in createdMarkersDict))
                        createdMarkersDict[selectedMonster] = [marker]
                    else
                        createdMarkersDict[selectedMonster].push(marker)
                }
            }
        }
    }

    // Set default meta data
    let title = $state('BattleNPC Locations');
    let description = $state('A searchable map with all monster locations.');


    function onButtonClick(id: number, usedData: UniqueLocation, addQuery: boolean) {
        selectedMapId.name = MapSheet[usedData.Map].Id;
        resolvedMapUrl = `https://v2.xivapi.com/api/asset/map/${selectedMapId.name}`
        selectedLocation = usedData;

        console.log(selectedMapId)

        fillPairs(usedData);
        console.log(names)
    }

    /**
     * Called when user changes the patch selection dropdown
     */
    function nameOptionChanged(payload: {type: 'add' | 'remove' | 'removeAll' | 'selectAll' | 'reorder', option: Option}) {
        if (payload.type === 'selectAll' || payload.type === 'selectAll' && payload.type === 'reorder')
            return;

        if (payload.type === 'removeAll') {
            for (const markers of Object.values(createdMarkersDict)) {
                for (const marker of markers) {
                    map.removeLayer(marker);
                }
            }

            createdMarkersDict = {};
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
            for (const marker of createdMarkersDict[selectedMonster]) {
                map.removeLayer(marker);
            }

            createdMarkersDict[selectedMonster] = [];
        }
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
        <MapSearchbar
                {uniqueLocations}
                {selectedId}
                {onButtonClick}
                {tabElements}
        />

        <div class="m-5"></div>

        {#if selectedMapId.name !== ''}
            <MultiSelect
                    bind:selected={selectedOptions}
                    options={nameOptions}
                    ulSelectedStyle="width: 85%;"
                    ulOptionsStyle="background-color: var(--bs-body-bg);"
                    onchange={nameOptionChanged}
                    placeholder="Select a monster"
            />
        {/if}
    </div>
</StackedSidebar>
<div class="col-12 col-lg-10 order-0 order-lg-2">
    <h1 class="text-center">Work in Progress, feedback and ideas welcome</h1>
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if selectedMapId.name !== ''}
            <div class="map" style="height:1024px;width:1024px" use:mapAction />
        {:else}
            <p>No map loaded</p>
        {/if}
    </div>
</div>