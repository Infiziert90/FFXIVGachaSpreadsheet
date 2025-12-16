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

    const MAXINT32 = 2147483647;

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

    function getBorderColor(idx: number) {
        switch (idx) {
            case 0: return '--bs-secondary-color';
            case 1: return '--bs-warning';
            case 2: return '--bs-success';
            default: return '--bs-danger';
        }
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

<div class="col-12 col-lg-2 order-0 order-lg-1 sticky-left-col">
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
<div class="col-12 col-lg-10 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if sectorData.length > 0}
            {#each sectorData as sector}
                <div class="container mb-5" style="background-color: var(--bs-tertiary-bg);">
                    <div style="margin: -.5rem .5rem .5rem -.5rem" class="pt-1 px-1"><h3>{sector.Name}</h3></div>
                    <div class="row">
                        {#each Object.entries(sector.Pools) as [tier, pool], idx}
                            <div class="col-4 p-0 ps-2 pb-1">
                                <div style="height: 100%; border: 0.15rem solid var({getBorderColor(idx)});">
                                    <Table striped size="sm" hover borderless class="align-middle">
                                        <thead>
                                        <tr>
                                            <th>Name</th>
                                            <th>Pct</th>
                                            <th>Poor</th>
                                            <th>Norm</th>
                                            <th>Opti</th>
                                        </tr>
                                        </thead>
                                        <tbody>
                                        {#each Object.values(pool.Rewards) as row}
                                            <tr>
                                                <td>{Mappings[row.Id].Name}</td>
                                                <td>{row.Amount}</td>
                                                <td>{row.MinMax['Poor'][0] < MAXINT32 ? row.MinMax['Poor'][0] : 0} - {row.MinMax['Poor'][1]}</td>
                                                <td>{row.MinMax['Normal'][0] < MAXINT32 ? row.MinMax['Normal'][0] : 0} - {row.MinMax['Normal'][1]}</td>
                                                <td>{row.MinMax['Optimal'][0] < MAXINT32 ? row.MinMax['Optimal'][0] : 0} - {row.MinMax['Optimal'][1]}</td>
                                            </tr>
                                        {/each}
                                        </tbody>
                                    </Table>
                                </div>
                            </div>
                        {/each}
                        <div class="col-3">
                            <div class="card">
                                <div class="card-header">
                                    Additional information
                                </div>
                                <ul class="list-group list-group-flush">
                                    <li class="list-group-item border-0 px-4 pt-1 pb-1 text-warning-emphasis">
                                        Normal <div class="float-end">60</div>
                                    </li>
                                    <li class="list-group-item border-0 px-4 pt-0 pb-1 text-success-emphasis">
                                        Optimal <div class="float-end">70</div>
                                    </li>
                                    <li class="list-group-item border-0 px-4 pt-0 pb-1 text-primary-emphasis">
                                        Favor <div class="float-end">80</div>
                                    </li>
                                    <li class="list-group-item border-0 px-4 pt-0 pb-1 ">
                                        Double Dip Rate <div class="float-end">50%</div>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            {/each}
        {/if}
    </div>
</div>