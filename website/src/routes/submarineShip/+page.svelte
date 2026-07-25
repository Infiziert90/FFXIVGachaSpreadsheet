<script lang="ts">
    import PageSidebar from "../../component/PageSidebar.svelte";
    import {onMount, tick} from "svelte";
    import {Mappings} from "$lib/mappings";
    import {page} from "$app/state";
    import {replaceState} from "$app/navigation";
    import {LastRank} from "$lib/sheets/sheetHelper";
    import type {SubRankRow} from "$lib/sheets/structure/submarines/subRank";
    import {TargetValues} from "$lib/submarines/target";
    import {
        type BestRoute,
        CalculateDuration, EmptyBestRoute,
        FindCalculatedRoute,
        SectorsToPath,
        ToExplorationArray
    } from "$lib/submarines/voyage";
    import {SubmarineBuild} from "$lib/submarines/build";
    import {getDuration, getIconPath, getWikiUrl} from "$lib/utils";
    import MultiSelect, {type Option} from "svelte-multiselect";
    import {SimpleSubExplorationSheet, SimpleSubMapSheet} from "$lib/sheets/simplifiedSheets";
    import {type SubMapRow, ToMapName} from "$lib/sheets/structure/submarines/subMap";
    import {ToLetterName} from "$lib/sheets/structure/submarines/subExploration";
    import {Input} from "@sveltestrap/sveltestrap";
    import {type Breakpoint, CalculateBreakpoint, EmptyBreakpoint} from "$lib/submarines/sector";
    import {VList, type VListHandle} from "virtua/svelte";

    // const
    const PartsCount: number = 10;

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: { [key: string]: HTMLButtonElement } = $state({});

    let allBuilds: SubmarineBuild[] = [];
    let rank: SubRankRow;
    let selectedRank = $state(145);
    let target: TargetValues = $state(new TargetValues());
    let lockedTarget: TargetValues = $state(new TargetValues());

    let ignoreBreakpoints: boolean = $state(false);
    let sectors: number[] = [13, 15, 10, 18, 26];
    let map: SubMapRow = $state(SimpleSubMapSheet[0])

    let selectedSectors: number[] = $state([]);
    let availableSectors: number[] = $state([]);
    let bestSectorPath: BestRoute = $state(EmptyBestRoute());
    let sectorBreakpoints: Breakpoint = $state(EmptyBreakpoint());
    let filteredPath: FilteredBuild[] = $state([]);

    let mapOptions: string[] = $state([]);
    let optionsToId: Record<number, number> = $state({});
    let selectedOption: Option = $state('' as Option);
    let selectOptionId = $state(0);

    let ref: VListHandle;

    const jumpToTop = () => {
        ref?.scrollTo(0);
    };

    initialize();

    for (const mapKey of Object.keys(SimpleSubMapSheet)) {
        let id = parseInt(mapKey);
        if (id === 0)
            continue;

        let idx = mapOptions.length;
        mapOptions.push(ToMapName(SimpleSubMapSheet[id]));

        optionsToId[idx] = id;
    }
    selectedOption = mapOptions[0];
    selectOptionId = optionsToId[0];
    changeMapSelection(1);

    // Set default meta data
    let title = $state('Submarine Ship Finder');
    let description = $state('Find the perfect build for the selected sectors.');

    // // Override defaults with URL parameters if they exist
    // let submarineSearchParams = tryGetSubmarineSearchSearchParams(page.url.searchParams);
    // if (submarineSearchParams !== undefined) {
    //     tableItemId = submarineSearchParams.itemId;
    //
    //     // svelte-ignore state_referenced_locally
    //     if (tableItemId in Mappings) {
    //         title = `Submarine Item Search - ${Mappings[tableItemId].Name}`;
    //         description = `All known sectors with drops for ${Mappings[tableItemId].Name}.`
    //     }
    // }

    // When page loads, open the tab for the current map
    onMount(async () => {
        // if (tableItemId > 0) {
        //     await onButtonClick(tableItemId, false);
        // }
    })

    async function onButtonClick(itemId: number, addQuery: boolean = false) {
        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('item', itemId.toString());
            replaceState(page.url, page.state);
        }

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Submarine Item Search - ${Mappings[itemId].Name}`;
    }

    function initialize() {
        allBuilds = [];

        rank = LastRank;
        selectedRank = rank.RowId;

        for (let hull = 0; hull < PartsCount; hull++)
        {
            for (let stern = 0; stern < PartsCount; stern++)
            {
                for (let bow = 0; bow < PartsCount; bow++)
                {
                    for (let bridge = 0; bridge < PartsCount; bridge++)
                    {
                        allBuilds.push(new SubmarineBuild(selectedRank, (hull * 4) + 3, (stern * 4) + 4, (bow * 4) + 1, (bridge * 4) + 2));
                    }
                }
            }
        }

        lockedTarget = TargetValues.FromBuilds(allBuilds);
        target = TargetValues.FromTarget(lockedTarget);
    }

    async function refreshList() {
        let newList: SubmarineBuild[] = [];
        for (let hull = 0; hull < PartsCount; hull++)
        {
            for (let stern = 0; stern < PartsCount; stern++)
            {
                for (let bow = 0; bow < PartsCount; bow++)
                {
                    for (let bridge = 0; bridge < PartsCount; bridge++)
                    {
                        newList.push(new SubmarineBuild(selectedRank, (hull * 4) + 3, (stern * 4) + 4, (bow * 4) + 1, (bridge * 4) + 2));
                    }
                }
            }
        }

        allBuilds = newList;
        lockedTarget = TargetValues.FromBuilds(allBuilds);
    }

    export interface FilteredBuild {
        Build: SubmarineBuild;
        Time: number;
    }

    function filterBuilds(): FilteredBuild[] {
        console.log("Rebuild triggered")
        sectorBreakpoints = CalculateBreakpoint([]);

        let distance: number = 0;
        let hasRoute: boolean = sectors.length > 0;
        if (hasRoute) {
            sectors = bestSectorPath.Path;
            distance = bestSectorPath.Distance;

            sectorBreakpoints = CalculateBreakpoint(bestSectorPath.Path);
        }

        let builds = allBuilds
            .filter(b => selectedRank >= b.HighestRankPart && b.Range >= distance && b.BuildCost <= rank.Capacity)
            .filter(b => hasRoute && !ignoreBreakpoints
                ? target.SectorFilter(b, sectorBreakpoints)
                : target.Filter(b))
            .map(t => {
                return {
                    Build: t,
                    Time: 43200
                }
            });

        if (hasRoute) {
            builds = builds.map(b => {
                return {
                    Build: b.Build,
                    Time: CalculateDuration(ToExplorationArray(sectors), b.Build.Speed)
                }
            });
        }

        return builds;
    }

    function sortBuilds(filteredBuilds: FilteredBuild[]): FilteredBuild[] {
        return filteredBuilds.sort((a, b) => a.Time - b.Time);
    }

    async function refreshFiltering() {
        filteredPath = sortBuilds(filterBuilds());
    }

    function changeMapSelection(mapId: number) {
        map = SimpleSubMapSheet[mapId];

        availableSectors = Object.values(SimpleSubExplorationSheet).filter(s => s.Map === mapId && !s.StartingPoint).map(s => s.RowId);
        selectedSectors = [];
    }

    async function optionChanged(payload: {type: 'add' | 'remove' | 'removeAll' | 'selectAll' | 'reorder', option: Option}) {
        if (payload.type === 'selectAll' || payload.type === 'selectAll' || payload.type === 'reorder' || payload.type === 'removeAll' || payload.type === 'remove')
            return;

        let optionIndex = mapOptions.indexOf(payload.option.toString());
        if (optionIndex === -1) {
            console.error(`Option ${payload.option} not found in options array`);
            return;
        }

        changeMapSelection(optionsToId[optionIndex]);
    }

    async function selectItem(sector: number) {
        if (selectedSectors.length >= 5)
            return;

        if (!selectedSectors.includes(sector)) {
            selectedSectors.push(sector);

            bestSectorPath = FindCalculatedRoute(selectedSectors);
            selectedSectors = [...bestSectorPath.Path];
        }

        if (availableSectors.includes(sector)) {
            let idx = availableSectors.indexOf(sector);
            if (idx !== -1) {
                availableSectors.splice(idx, 1);
            }

            availableSectors.sort((a, b) => a - b);
        }

        await refreshFiltering();
    }

    async function deselectItem(sector: number) {
        if (selectedSectors.includes(sector)) {
            let idx = selectedSectors.indexOf(sector);
            if (idx !== -1) {
                selectedSectors.splice(idx, 1);
            }

            bestSectorPath = FindCalculatedRoute(selectedSectors);
            selectedSectors = [...bestSectorPath.Path];
        }

        if (!availableSectors.includes(sector)) {
            availableSectors.push(sector);
            availableSectors.sort((a, b) => a - b);
        }

        await refreshFiltering();
    }

    async function checkboxOptionsChanged() {
        await refreshFiltering();
    }

    async function targetOptionsChanged(target: TargetValues, targetOption: boolean) {
        targetOption = !targetOption;

        await refreshFiltering();
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>

<PageSidebar>
    <div class="d-flex flex-column gap-2 max-w-100 overflow-x-hidden">
        <MultiSelect
                bind:value={selectedOption}
                options={mapOptions}
                ulSelectedClass="multiSelect-selection"
                ulOptionsStyle="padding-left:0.5rem;"
                placeholder="Select a map"
                onchange={optionChanged}
                maxSelect={1}
                minSelect={1}
                required={true}
                portal={{ active: true }}
        />

        <select class="w-100 form-select" size="5">
            {#each selectedSectors as item}
                <option
                        class="border-bottom"
                        value={item}
                        onclick={async () => await deselectItem(item)}
                >
                    {ToLetterName(SimpleSubExplorationSheet[item])}
                </option>
            {/each}
        </select>

        <select class="w-100 form-select" size="15" disabled={selectedSectors.length >= 5}>
            {#each availableSectors as item}
                <option
                        class="border-bottom"
                        value={item}
                        onclick={async () => await selectItem(item)}
                >
                    {ToLetterName(SimpleSubExplorationSheet[item])}
                </option>
            {/each}
        </select>

        <h5 class="mt-3">Options:</h5>
        <Input class="mb-0" type="checkbox" bind:checked={ignoreBreakpoints} label="Ignore Breakpoints" on:change={async () => await checkboxOptionsChanged()}></Input>
        {#if !ignoreBreakpoints}
            <Input class="mb-0" type="checkbox" bind:checked={target.UseT1} label="Only Tier 1" on:change={async () => await targetOptionsChanged(target, target.UseT1)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.UseT2} label="Only Tier 2" on:change={async () => await targetOptionsChanged(target, target.UseT2)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.UsePoor} label="Only Poor" on:change={async () => await targetOptionsChanged(target, target.UsePoor)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.UseNormal} label="Only Normal" on:change={async () => await targetOptionsChanged(target, target.UseNormal)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.IgnoreFavor} label="Ignore Favor" on:change={async () => await targetOptionsChanged(target, target.IgnoreFavor)}></Input>
            <Input class="mb-0" type="checkbox" bind:checked={target.NoModded} label="No Modified Parts" on:change={async () => await targetOptionsChanged(target, target.NoModded)}></Input>
        {/if}
    </div>
</PageSidebar>
<div class="col-12 col-lg-9 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        <div class="container mb-5 p-2 rounded border tier-anchor" style="background-color: var(--bs-tertiary-bg);">
            {#if bestSectorPath.Path.length > 0}
                <div class="card mb-5 bg-dark">
                    <div class="row g-0 align-items-center">
                        <div class="col-2">
                            <div class="item-card-icon px-3 d-flex align-items-center justify-content-center rounded-start h-100">
                                <h4 class="m-0">Route</h4>
                            </div>
                        </div>
                        <div class="col">
                            <div class="card-body">
                                <h5 class="card-title">{SectorsToPath(" -> ", bestSectorPath.Path)}</h5>
                                <p class="card-text m-0">
                                    Tier 2: {sectorBreakpoints.T2} - Tier 3: {sectorBreakpoints.T3}
                                    <br>
                                    Normal: {sectorBreakpoints.Normal} - Optimal: {sectorBreakpoints.Optimal}
                                    <br>
                                    Favor: {sectorBreakpoints.Favor}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
                <button type="button" onclick={jumpToTop}> jump to top </button>
                <VList bind:this={ref} data={filteredPath} style="height: 75vh;">
                    {#snippet children(item)}
                        <div class="card mb-3">
                            <div class="row g-0 align-items-center">
                                <div class="col-2">
                                    <div class="item-card-icon px-3 d-flex align-items-center justify-content-center rounded-start h-100">
                                        <h4 class="m-0">{item.Build.FullIdentifier()}</h4>
                                    </div>
                                </div>
                                <div class="col">
                                    <div class="card-body">
                                        <h5 class="card-title">{getDuration(item.Time)}</h5>
                                        <p class="card-text m-0">
                                            Surv: {item.Build.Surveillance} - Ret: {item.Build.Retrieval} - Favor: {item.Build.Favor}
                                            <br>
                                            Speed: {item.Build.Speed} - Range: {item.Build.Range}
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    {/snippet}
                </VList>
            {:else}
                <p>No sectors selected.</p>
            {/if}
        </div>
    </div>
</div>