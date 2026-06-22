<script lang="ts">
    import type {Reduction, ReductionSource, ReductionTier} from "$lib/structs/reduction";
    import PageSidebar from "../../component/PageSidebar.svelte";
    import ReductionAccordion from "../../component/ReductionAccordion.svelte";
    import ReductionPieChart from "../../component/ReductionPieChart.svelte";
    import {Mappings} from "$lib/mappings";
    import {tryGetReductionSearchParams} from "$lib/searchParamHelper";
    import {page} from "$app/state";
    import {onMount} from "svelte";
    import {replaceState} from "$app/navigation";
    import {ReduceSpecial} from "$lib/table";
    import DropsTable from "../../component/DropsTable.svelte";
    import {SimpleReductionReward} from "$lib/sheets/simplifiedSheets";

    interface Props {
        content: Reduction[];
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data }: Props = $props();
    let reductionData: Reduction = data.content;

    // let patches: string[] = $state(Object.keys(reductionData[0]))
    // // svelte-ignore state_referenced_locally
    // let selectedPatches: Option[] = $state([...patches.values()])

    // Table data
    let tableItems: ReductionSource = $state(reductionData.Jobs[0].Sources[0]);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // Initialize with default
    let job = $state(reductionData.Jobs[0].Id);
    let source = $state(reductionData.Jobs[0].Sources[0].Id);

    // Set default meta data
    let title = $state('Aetherial Reduction');
    let description = $state('Possibilities for all types of Aetherial Reduction.');

    // Override defaults with URL parameters if they exist
    let reductionSearchParams = tryGetReductionSearchParams(page.url.searchParams);
    if (reductionSearchParams !== undefined) {
        job = reductionSearchParams.jobId;
        source = reductionSearchParams.sourceId;

        // svelte-ignore state_referenced_locally
        let jobSelection = reductionData.Jobs.find(j => j.Id === job);
        if (jobSelection !== undefined) {
            let sourceSelection = jobSelection.Sources.find(s => s.Id === source);
            if (sourceSelection !== undefined) {
                title = `Aetherial Reduction - ${Mappings[source].Name}`;
                description = `All tiers for this reduction source.`;
            }
        }
    }

    // When page loads, open the tab for the current sourceId
    onMount(() => {
        openTab(job, source, false)
    })

    /**
     * Opens a tab and displays its data
     * @param jobId - The job ID that this belongs to
     * @param sourceId - The item source ID to display
     * @param addQuery - If true, update the URL with these parameters
     */
    function openTab(jobId: number, sourceId: number, addQuery: boolean = false) {
        // Update state variables
        job =  jobId;
        source = sourceId;

        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('job', jobId.toString());
            page.url.searchParams.set('source', sourceId.toString());
            replaceState(page.url, page.state);
        }

        let jobSelection = reductionData.Jobs.find(j => j.Id === jobId);
        if (jobSelection === undefined) {
            console.log(`jobId: ${jobId} not found in Jobs.`);
            return;
        }

        let sourceSelection = jobSelection.Sources.find(s => s.Id === sourceId);
        if (sourceSelection === undefined) {
            console.log(`sourceId: ${sourceId} not found in Sources.`);
            return;
        }

        // // Check if the selected patch is invalid, if so reset to default
        // let availablePatches = Object.keys(selection.variant.Patches);
        // if (availablePatches.length !== patches.length || !selectedPatches.every(v => availablePatches.includes(v.toString()))) {
        //     // Update the available patches list
        //     patches.length = 0;
        //     patches = availablePatches;
        //
        //     selectedPatches.length = 0;
        //     selectedPatches = [...patches.values()];
        // }
        //
        // // Get the patch data for the selected patch
        // const patchData = selectedPatches.length === 1
        //     ? selection.variant.Patches[selectedPatches[0]]
        //     : combineCoffer(selection.variant.Patches, selectedPatches);

        // Update table data
        tableItems = sourceSelection;

        // Update stats display
        titleStats = `${Mappings[sourceId].Name} Stats`;

        // Calculate total across all variants in this territory
        totalStats = `Total: ${sourceSelection.Records.toLocaleString()}`;

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Aetherial Reduction - ${Mappings[sourceId].Name}`;
    }

    // /**
    //  * Called when user changes the patch selection dropdown
    //  */
    // function patchSelectionChanged(event: Event) {
    //     openTab(territory, coffer, false);
    // }

    interface BonusChance {
        hasBonus: boolean;
        bonusChance: string;
    }

    /**
     * Calculate the chance of getting bonus for this tier
    */
    function getBonusChance(tierData: ReductionTier): BonusChance {
        let patchData = Object.values(tierData.Patches)[0];

        if (patchData.BonusCount === 0)
            return { hasBonus: false, bonusChance: '' };

        let bonusChance = patchData.BonusCount / tierData.Records * 100;
        return { hasBonus: true, bonusChance: bonusChance.toFixed(2) };
    }

    function scrollToTier(subTier: number) {
        const el = document.getElementById(`tier-${subTier}`);
        if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    type VizMode = 'cards' | 'pie';
    let vizMode = $state<VizMode>('cards');

    function goToTierCard(subTier: number) {
        vizMode = 'cards';
        setTimeout(() => scrollToTier(subTier), 50);
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>

<PageSidebar>
    <ReductionAccordion
            {reductionData}
            {job}
            {source}
            {openTab}
            {tabElements}
    />
</PageSidebar>
<div class="col-12 col-lg-2 order-0 order-lg-3">
    <div id="stats" class="stats">
        <div class="card">
            <div class="card-header">
                <strong>{titleStats}</strong>
            </div>
            <ul class="list-group list-group-flush">
                <li class="list-group-item">{totalStats}</li>
<!--                <li class="list-group-item">-->
<!--                    <label for="patch">Patches</label>-->
<!--                    <MultiSelect-->
<!--                            bind:selected={selectedPatches}-->
<!--                            options={patches}-->
<!--                            required={true}-->
<!--                            ulSelectedClass="multiSelect-selection"-->
<!--                            ulOptionsStyle="padding-left:0.5rem;"-->
<!--                            onchange={patchSelectionChanged}-->
<!--                    />-->
<!--                </li>-->
            </ul>
        </div>
    </div>
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <!-- Viz mode tabs -->
    <ul class="nav nav-pills nav-fill mb-3">
        <li class="nav-item">
            <button class="nav-link" class:active={vizMode === 'cards'}
                    onclick={() => vizMode = 'cards'}>
                <i class="bi bi-card-list"></i> Cards
            </button>
        </li>
        <li class="nav-item">
            <button class="nav-link" class:active={vizMode === 'pie'}
                    onclick={() => vizMode = 'pie'}>
                <i class="bi bi-pie-chart"></i> Pie
            </button>
        </li>
    </ul>

    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if vizMode === 'cards'}
            <p>
                Lowest Sand: {tableItems.LowestSand === 10000 ? 'No Data' : tableItems.LowestSand}
                {#if tableItems.LowestBonus !== 10000}
                    <br>
                    Lowest Bonus: {tableItems.LowestBonus}
                {/if}
            </p>

            {#each tableItems.Tiers as tierData}
                {@const bonusData = getBonusChance(tierData)}
                <div id="tier-{tierData.SubTier}" class="container mb-5 p-2 rounded border tier-anchor" style="background-color: var(--bs-tertiary-bg);">
                    <h4>Tier ({SimpleReductionReward[tableItems.MainTier][tierData.SubTier].MinimumCollectability})</h4>
                    {#each Object.values(tierData.Patches) as patchData}
                        {#if patchData.NormalCount > 0}
                            <p>Records: {patchData.NormalCount}</p>
                            <DropsTable items={patchData.Normal} columns={ReduceSpecial} />
                        {/if}

                        {#if patchData.BonusCount > 0}
                            <h4>Bonus</h4>
                            <p>
                                Records: {patchData.BonusCount}
                                <br>
                                Bonus chance: {bonusData.bonusChance}%
                            </p>
                            <DropsTable items={patchData.Bonus} columns={ReduceSpecial} />
                        {/if}
                    {/each}
                </div>
            {/each}

        {:else if vizMode === 'pie'}
            <ReductionPieChart source={tableItems} mainTier={tableItems.MainTier} onTierClick={goToTierCard} />
        {/if}
    </div>
</div>

<style>
    .tier-anchor {
        scroll-margin-top: calc(80px + .5rem);
    }
</style>
