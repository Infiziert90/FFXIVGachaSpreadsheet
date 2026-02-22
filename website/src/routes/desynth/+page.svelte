<script lang="ts">
    import { page } from '$app/state';
    import { replaceState } from "$app/navigation";
    import type {DesynthBase, DesynthesisBase, DesynthHistory, Reward} from "$lib/interfaces";
    import { Mappings } from "$lib/mappings";
    import {onMount, tick} from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import {NameObtainedMinChanceSetup, RewardDesynthSpecial} from "$lib/table";
    import DesynthSearchbar from "../../component/DesynthSearchbar.svelte";
    import {tryGetDesynthSearchParams} from "$lib/searchParamHelper";
    import PageSidebar from "../../component/PageSidebar.svelte";
    import {pad} from "$lib/utils";
    import {loadDesynth} from "$lib/loadHelpers";

    interface Props {
        content: DesynthesisBase;
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: { [key: string]: HTMLButtonElement } = $state({});

    let { data }: Props = $props();

    const desynthesisBase: DesynthesisBase = data.content;

    // Loaded at runtime
    const loadedSplits: string[] = [];
    const desynthBase: DesynthBase = {Sources: {}, Rewards: {}};

    // Table data
    let tableItems: Reward[] = $state([]);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // Selected item tracking
    let selectedId = $state(0);
    let selectedStatsType = $state('');

    // add defaults if things aren't set correctly
    let sourceParam = $state(0);
    let rewardParam = $state(0);

    let searchType = $state(1);

    // Set default meta data
    let title = $state('Desynthesis');
    let description = $state('Possibilities for desynthesis material and their rewards.');

    // Override defaults with URL parameters if they exist
    let desynthSearchParams = tryGetDesynthSearchParams(page.url.searchParams);
    if (desynthSearchParams !== undefined) {
        if (desynthSearchParams.sourceId > 0) sourceParam = desynthSearchParams.sourceId;
        if (desynthSearchParams.rewardId > 0) rewardParam = desynthSearchParams.rewardId;

        // svelte-ignore state_referenced_locally
        let isSourceSearch = sourceParam > 0;

        // svelte-ignore state_referenced_locally
        let id = isSourceSearch ? sourceParam : rewardParam;
        let titleAddition = isSourceSearch ? 'Source Search' : 'Reward Search';
        let descriptionAddition = isSourceSearch ? 'Rewards' : 'Sources';

        if (id in Mappings[id]) {
            title = `Desynthesis - ${titleAddition}`;
            description = `${descriptionAddition} for ${Mappings[id].Name}.`;
        }
    }

    onMount(async () => {
        if (sourceParam > 0) {
            await onButtonClick(sourceParam, 'Desynths', false);
        } else if (rewardParam > 0) {
            await onButtonClick(rewardParam,'Received', false);
        }
    });

    async function onButtonClick(id: number, statsType: string, addQuery: boolean) {
        if (addQuery) {
            // Clear the other param and set the current one
            if (statsType === 'Desynths') {
                page.url.searchParams.delete('reward');
                page.url.searchParams.set('source', id.toString());
            } else {
                page.url.searchParams.delete('source');
                page.url.searchParams.set('reward', id.toString());
            }

            replaceState(page.url, page.state);
        }

        // Update search type
        searchType = statsType === 'Desynths' ? 1 : 2;
        let usedData = searchType === 1 ? desynthBase.Sources : desynthBase.Rewards;

        const selection = await tryGetDesynth(usedData, id);
        if (selection === undefined) return;

        // Update selected tracking
        selectedId = id;
        selectedStatsType = statsType;

        // Update table data
        tableItems = selection.history.Rewards;

        // Update stats
        titleStats = `${Mappings[id].Name} Stats`;
        totalStats = `${statsType}: ${selection.history.Records.toLocaleString()}`;
        selectedStats = ` `;

        // Update button highlighting in accordion
        for (const element of Object.values(tabElements)) {
            element.classList.remove('btn-success');
        }
        const sourceButton = tabElements[`source-${id}`];
        const rewardButton = tabElements[`reward-${id}`];
        if (sourceButton) sourceButton.classList.add('btn-success');
        if (rewardButton) rewardButton.classList.add('btn-success');

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        let titleAddition = searchType === 1 ? 'Source Search' : 'Reward Search';
        document.title = `Desynthesis - ${titleAddition}`
    }

    interface DesynthSelection {
        history: DesynthHistory;
    }

    /**
     * Try to get the specific venture and task.
     * @param data - Dictionary to search through
     * @param requestedId - The item id to resolve
     * @returns Desynth selection if successful, undefined otherwise.
     */
    async function tryGetDesynth(data: Record<number, DesynthHistory>, requestedId: number): Promise<DesynthSelection | undefined> {
        // load split data if not already in memory
        await loadSplitData(requestedId);

        // Find the history for the selected id
        const history = data[requestedId]
        if (!history) return undefined;

        return { history };
    }

    async function loadSplitData(itemId: number) {
        let paddedSplitId = pad(Math.ceil(itemId/1000)*1000, 6)
        if (loadedSplits.includes(paddedSplitId)) return;

        loadedSplits.push(paddedSplitId);
        let split = await loadDesynth(`/data/desynthesis/${paddedSplitId}.json`, fetch);

        for (const [sourceId, history] of Object.entries(split.content.Sources))
            desynthBase.Sources[sourceId] = history;

        for (const [rewardId, history] of Object.entries(split.content.Rewards))
            desynthBase.Rewards[rewardId] = history;
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:site_name" content={title}>
    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>

<PageSidebar>
    <DesynthSearchbar
        {desynthesisBase}
        {selectedId}
        {onButtonClick}
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
                <li class="list-group-item">{selectedStats}</li>
                <li class="list-group-item">
                    <label for="patch">Patch</label>
                </li>
            </ul>
        </div>
    </div>
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if tableItems.length > 0}
            <DropsTable items={tableItems} columns={searchType === 1 ? NameObtainedMinChanceSetup : RewardDesynthSpecial} />
        {:else}
            <p>No data found</p>
        {/if}
    </div>
</div>