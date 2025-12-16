<script lang="ts">
    import type {Sector, SubLoot} from "$lib/interfaces";
    import DropsTable from "../../component/DropsTable.svelte";
    import {FullColumnSetup} from "$lib/table";
    import {Icon, Table} from '@sveltestrap/sveltestrap';
    import VentureAccordion from "../../component/VentureAccordion.svelte";
    import ItemCard from "../../component/ItemCard.svelte";
    import SubmarineAccordion from "../../component/SubmarineAccordion.svelte";
    import {MapToStartSector, ReversedMaps, SubmarineExplorationSheet, SubmarineMapSheet} from "$lib/sheets";
    import {page} from "$app/state";
    import {replaceState} from "$app/navigation";
    import {tryGetSubmarineSearchParams, tryGetVentureSearchParams} from "$lib/searchParamHelper";
    import {onMount} from "svelte";
    import {Mappings} from "$lib/mappings";
    import {ToName} from "$lib/sheets/submarineExploration";

    interface Props {
        content: SubLoot;
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;

    let { data }: Props = $props();
    let subData: SubLoot = data.content;
    let mapData = Object.values(SubmarineMapSheet).filter( (v) => v.RowId !== 0);
    let sectorData: Sector[] = $state([]);

    let map = $state(1);

    // Set default meta data
    let title = $state('Submarine Loot');
    let description = $state('Sector loot overview with possibilities and quantities.');

    // Override defaults with URL parameters if they exist
    let submarineSearchParams = tryGetSubmarineSearchParams(page.url.searchParams);
    if (submarineSearchParams !== undefined) {
        map = submarineSearchParams.mapId;

        // svelte-ignore state_referenced_locally
        if (map in SubmarineMapSheet) {
            title = `Submarine Loot - ${SubmarineMapSheet[map].Name}`;
        }
    }

    // When page loads, open the tab for the current category/taskType
    onMount(() => {
        openTab(map, false)
    })

    function openTab(mapId: number, addQuery: boolean = false) {
        // Check if map exists, if not default to Deep-sea Site
        if (!(mapId in SubmarineMapSheet)) {
            mapId = 1;
        }

        // Update state variables
        map = mapId;

        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('map', map.toString());
            replaceState(page.url, page.state);
        }

        let start: number;
        start = MapToStartSector[map].RowId + 1;

        sectorData.length = 0;
        for (let i = start; SubmarineExplorationSheet[i].SurveyDistance !== 0; i++) {
            sectorData.push(subData.Sectors[i]);
        }

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Submarine Loot - ${SubmarineMapSheet[map].Name}`;
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:site_name" content={title}>
    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>

<button class="btn btn-primary btn-lg rounded-xl d-lg-none position-fixed bottom-0 end-0 m-3 w-auto z-3" type="button" data-bs-toggle="offcanvas" data-bs-target="#offcanvasFilter" aria-controls="offcanvasFilter">
    <Icon name="funnel-fill" />
</button>

<div class="col-12 col-lg-3 order-0 order-lg-1 sticky-left-col">
    <div class="offcanvas-lg offcanvas-start" tabindex="-1" id="offcanvasFilter" aria-labelledby="offcanvasFilterLabel">
        <div class="offcanvas-header">
            <h5 class="offcanvas-title" id="offcanvasFilterLabel">Filter your category</h5>
            <button type="button" class="btn-close" data-bs-dismiss="offcanvas" data-bs-target="#offcanvasFilter" aria-label="Close"></button>
        </div>
        <div class="offcanvas-body">
            <SubmarineAccordion
                {mapData}
                {map}
                {openTab} 
            />
        </div>
    </div>
</div>
<div class="col-12 col-lg-2 order-0 order-lg-3">
<!--    <div id="stats" class="stats">-->
<!--        <div class="card">-->
<!--            <div class="card-header">-->
<!--                <strong>{titleStats}</strong>-->
<!--            </div>-->
<!--            <ul class="list-group list-group-flush">-->
<!--                <li class="list-group-item">{totalStats}</li>-->
<!--                <li class="list-group-item">{selectedStats}</li>-->
<!--                <li class="list-group-item">-->
<!--                    <label for="patch">Patch</label>-->
<!--                    <select id="patch" class="form-select" bind:value={selectedPatch} onchange={patchSelectionChanged}>-->
<!--                        {#each patches as patch, idx}-->
<!--                            <option value={idx}>-->
<!--                                {patch}-->
<!--                            </option>-->
<!--                        {/each}-->
<!--                    </select>-->
<!--                </li>-->
<!--            </ul>-->
<!--        </div>-->
<!--    </div>-->
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if sectorData.length > 0}
            {#each sectorData as sector}
                <h3>{sector.Name}</h3>

                {#each Object.entries(sector.Pools) as [tier, pool]}
                    <h5>{tier}</h5>
                    <Table striped size="sm" hover borderless class="align-middle">
                        <thead>
                        <tr>
                            <th>Name</th>
                            <th>Amount</th>
                            <th>Poor</th>
                            <th>Normal</th>
                            <th>Optimal</th>
                        </tr>
                        </thead>
                        <tbody>
                        {#each Object.values(pool.Rewards) as row}
                            <tr>
                                <td>{Mappings[row.Id].Name}</td>
                                <td>{row.Amount}</td>
                                <td>{row.MinMax['Poor'][0]} - {row.MinMax['Poor'][1]}</td>
                                <td>{row.MinMax['Normal'][0]} - {row.MinMax['Normal'][1]}</td>
                                <td>{row.MinMax['Optimal'][0]} - {row.MinMax['Optimal'][1]}</td>
                            </tr>
                        {/each}
                        </tbody>
                    </Table>

                {/each}
            {/each}
        {/if}
    </div>
</div>