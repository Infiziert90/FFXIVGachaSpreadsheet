<script lang="ts">
    import type {Reduction, ReductionSource} from "$lib/structs/reduction";
    import PageSidebar from "../../component/PageSidebar.svelte";
    import ReductionAccordion from "../../component/ReductionAccordion.svelte";
    import {Mappings} from "$lib/mappings";
    import {tryGetReductionSearchParams} from "$lib/searchParamHelper";
    import {page} from "$app/state";
    import {onMount} from "svelte";
    import {replaceState} from "$app/navigation";

    interface Props {
        content: Reduction[];
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data }: Props = $props();
    let reductionData: Reduction = data.content;

    let firstSource = Object.values(reductionData.Sources)[0];

    // let patches: string[] = $state(Object.keys(reductionData[0]))
    // // svelte-ignore state_referenced_locally
    // let selectedPatches: Option[] = $state([...patches.values()])

    // Table data
    let tableItems: ReductionSource = $state(firstSource);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // Initialize with default values
    let source = $state(parseInt(Object.keys(reductionData.Sources)[0]));

    // Set default meta data
    let title = $state('Aetherial Reduction');
    let description = $state('Possibilities for all types of Aetherial Reduction.');

    // Override defaults with URL parameters if they exist
    let reductionSearchParams = tryGetReductionSearchParams(page.url.searchParams);
    if (reductionSearchParams !== undefined) {
        source = reductionSearchParams.sourceId;

        // svelte-ignore state_referenced_locally
        if (source in reductionData.Sources) {
            title = `Aetherial Reduction - ${Mappings[source].Name}`;
            description = `All tiers for this reduction source.`;
        }
    }

    // When page loads, open the tab for the current sourceId
    onMount(() => {
        openTab(source, false)
    })

    /**
     * Opens a tab and displays its data
     * @param sourceId - The item source ID to display
     * @param addQuery - If true, update the URL with these parameters
     */
    function openTab(sourceId: number, addQuery: boolean = false) {
        // Update state variables
        source = sourceId;

        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('source', sourceId.toString());
            replaceState(page.url, page.state);
        }

        if (!(sourceId in reductionData.Sources))
            return;

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
        tableItems = reductionData.Sources[sourceId];

        // Update stats display
        titleStats = `${Mappings[sourceId].Name} Stats`;

        // Calculate total across all variants in this territory
        totalStats = `Total: ${reductionData.Sources[sourceId].Records.toLocaleString()}`;

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
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        <p>
            Lowest Sand: {tableItems.LowestSand}
            <br>
            Lowest Bonus: {tableItems.LowestBonus}
        </p>

        {#each Object.entries(tableItems.Tiers) as [tier, tierData]}
            <h4>Tier {tier} ({tierData.Minimum})</h4>
            {#each Object.values(tierData.Patches) as patchData}
                {#if patchData.NormalCount > 0}
                    <h6>Normal</h6>
                    <p>
                        {#each Object.entries(patchData.Normal) as [rewardId, rewardData]}
                            {Mappings[rewardId].Name} - Min: {rewardData.Min} Max: {rewardData.Max}<br>
                        {/each}
                    </p>
                {/if}

                {#if patchData.BonusCount > 0}
                    <h5>Bonus</h5>
                    <p>
                        {#each Object.entries(patchData.Bonus) as [rewardId, rewardData]}
                            {Mappings[rewardId].Name} - Min: {rewardData.Min} Max: {rewardData.Max}<br>
                        {/each}
                    </p>
                {/if}
            {/each}
        {/each}
    </div>
</div>
