<script lang="ts">
    import { page } from '$app/state';
    import { replaceState } from "$app/navigation";
    import type {DesynthesisBase} from "$lib/interfaces";
    import { Mappings } from "$lib/mappings";
    import {onMount} from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import {NameObtainedMinChanceSetup, RewardDesynthSpecial} from "$lib/table";
    import DesynthSearchbar from "../../component/DesynthSearchbar.svelte";
    import PageSidebar from "../../component/PageSidebar.svelte";
    import {type DesynthBase2, isSource, type RewardHistory, type SourceHistory} from "$lib/structs/desynthesis";
    import {Alert} from "@sveltestrap/sveltestrap";
    import {pad} from "$lib/utils";
    import {loadDesynth2} from "$lib/loadHelpers";
    import {tryGetDesynthSearchParams} from "$lib/searchParamHelper";
    import {SimpleJobSheet} from "$lib/sheets/simplifiedSheets";

    interface Props {
        content: DesynthBase2;
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: { [key: string]: HTMLButtonElement } = $state({});

    let { data }: Props = $props();

    const loadedSplits: string[] = [];

    const desynthesisData: DesynthBase2 = {Patches: {}};
    const desynthesisBase: DesynthesisBase = data.content;

    // Table data
    let tableItems: SourceHistory | RewardHistory | undefined = $state(undefined);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let totalStats2 = $state('');
    let selectedStats = $state('');

    // Selected item tracking
    let selectedId = $state(0);

    // add defaults if things aren't set correctly
    let sourceParam = $state(0);
    let rewardParam = $state(0);

    let searchKey: number = $state(0);
    let errorKey: string = $state('');

    // Set default meta data
    let title = $state('Desynthesis V2');
    let description = $state('An improved work in progress version for desynthesis stats.');

    // Override defaults with URL parameters if they exist
    let desynthSearchParams = tryGetDesynthSearchParams(page.url.searchParams);
    if (desynthSearchParams !== undefined) {
        if (desynthSearchParams.sourceId > 0) sourceParam = desynthSearchParams.sourceId;
        if (desynthSearchParams.rewardId > 0) rewardParam = desynthSearchParams.rewardId;

        // svelte-ignore state_referenced_locally
        let isSourceSearch = sourceParam > 0;

        // svelte-ignore state_referenced_locally
        let id = isSourceSearch ? sourceParam : rewardParam;
        let descriptionAddition = isSourceSearch ? 'rewards' : 'sources';

        if (id in Mappings) {
            title = `Desynthesis V2 - ${Mappings[id].Name}`;
            description = `All known ${descriptionAddition} for ${Mappings[id].Name}.`;
        }
    }

    onMount(async () => {
        if (sourceParam > 0) {
            await onButtonClick(sourceParam, 0, false);
        } else if (rewardParam > 0) {
            await onButtonClick(rewardParam, 1, false);
        }
    });

    async function onButtonClick(id: number, statsType: number, addQuery: boolean) {
        if (addQuery) {
            // Clear the other param and set the current one
            if (statsType === 0) {
                page.url.searchParams.delete('reward');
                page.url.searchParams.set('source', id.toString());
            } else {
                page.url.searchParams.delete('source');
                page.url.searchParams.set('reward', id.toString());
            }

            replaceState(page.url, page.state);
        }

        // Update search type
        searchKey = statsType;

        const selection = await tryGetDesynth(searchKey, id);
        if (selection === undefined) {
            errorKey = 'Unable to find item!'
            return;
        }

        // Update selected tracking
        selectedId = id;

        // Update table data
        tableItems = selection;

        // Update stats
        titleStats = `${Mappings[id].Name} Stats`;
        if (isSource(selection)) {
            let s = selection as SourceHistory;

            totalStats = `Above: ${s.A.toLocaleString()}`;
            totalStats2 = `Below: ${s.B.toLocaleString()}`;
        } else {
            let s = selection as RewardHistory;

            totalStats = `Received: ${s.Records.toLocaleString()}`;
            totalStats2 = ``;
        }
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
        let titleAddition = searchKey === 0 ? 'Source Search' : 'Reward Search';
        document.title = `Desynthesis - ${titleAddition}`
    }

    /**
     * Try to get the specific venture and task.
     * @param searchType - The search type to resolve
     * @param requestedId - The item id to resolve
     * @returns Desynth selection if successful, undefined otherwise.
     */
    async function tryGetDesynth(searchType: number, requestedId: number): Promise<SourceHistory | RewardHistory | undefined> {
        // load split data if not already in memory
        await loadSplitData(requestedId);

        // Find the history for the selected id
        const selectedPatch = desynthesisData.Patches['7.5'];
        let selectedSearchData = searchType === 0 ? selectedPatch.Sources : selectedPatch.Rewards;

        let history = selectedSearchData[requestedId]
        if (!history) return undefined;

        return history;
    }

    async function loadSplitData(itemId: number) {
        let paddedSplitId = pad(Math.ceil(itemId/1000)*1000, 6)
        if (loadedSplits.includes(paddedSplitId)) return;

        loadedSplits.push(paddedSplitId);
        let split = await loadDesynth2(`/data/desynthesis2/${paddedSplitId}.json`, fetch);
        for (const [patch, patchData] of Object.entries(split.content.Patches)) {
            if (!(patch in desynthesisData.Patches))
                desynthesisData.Patches[patch] = {Records: 0, Sources: {}, Rewards: {}};

            desynthesisData.Patches[patch].Records = patchData.Records;
            for (const [sourceId, history] of Object.entries(patchData.Sources))
                desynthesisData.Patches[patch].Sources[sourceId] = history;

            for (const [rewardId, history] of Object.entries(patchData.Rewards))
                desynthesisData.Patches[patch].Rewards[rewardId] = history;
        }
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>
{#if tableItems !== undefined}
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
                    <li class="list-group-item">{totalStats}<br>{totalStats2}</li>
                    <li class="list-group-item">{selectedStats}</li>
                </ul>
            </div>
        </div>
    </div>
    <div class="col-12 col-lg-7 order-0 order-lg-2">
        {#if errorKey.length > 0}
            <Alert content={errorKey} color="danger" dismissible/>
        {/if}
        <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
            {#if isSource(tableItems)}
                {#if tableItems.Above.length > 0 || tableItems.Below.length > 0}
                    <div class="container mb-5 p-2 rounded border tier-anchor" style="background-color: var(--bs-tertiary-bg);">
                        <h4>{`${SimpleJobSheet[tableItems.Job].NameEnglish} ≥ ${tableItems.ILvl}`}</h4>
                        {#if tableItems.Above.length > 0}
                            <DropsTable items={tableItems.Above} columns={NameObtainedMinChanceSetup} />
                        {:else}
                            <p>No above data for the selected item.</p>
                        {/if}
                    </div>

                    <div class="container mb-5 p-2 rounded border tier-anchor" style="background-color: var(--bs-tertiary-bg);">
                        <h4>Below</h4>
                        {#if tableItems.Below.length > 0}
                            <DropsTable items={tableItems.Below} columns={NameObtainedMinChanceSetup} />
                        {:else}
                            <p>No below data for the selected item.</p>
                        {/if}
                    </div>
                {:else}
                    <Alert content="No source data for the selected item." color="danger" dismissible/>
                {/if}
            {:else}
                {#if tableItems.Rewards.length > 0}
                    <div class="container mb-5 p-2 rounded border tier-anchor" style="background-color: var(--bs-tertiary-bg);">
                        <h4>Received From</h4>
                        <DropsTable items={tableItems.Rewards} columns={RewardDesynthSpecial} />
                    </div>
                {:else}
                    <Alert content="No reward data for the selected item." color="danger" dismissible/>
                {/if}
            {/if}
        </div>
    </div>
{:else}
<div class="col-12">
    <DesynthSearchbar
            {desynthesisBase}
            {selectedId}
            {onButtonClick}
            {tabElements}
    />
</div>
{/if}