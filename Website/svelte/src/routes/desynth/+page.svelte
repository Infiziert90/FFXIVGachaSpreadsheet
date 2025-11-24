<script lang="ts">
    import { page } from '$app/state';
    import {replaceState} from "$app/navigation";
    import type { DesynthBase, DesynthHistory, Reward } from "$lib/interfaces";
    import { IconPaths } from "$lib/data";
    import {onMount} from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import type { ColumnTemplate } from "$lib/table";
    import DesynthSearchbar from "../../component/DesynthSearchbar.svelte";

    interface Props {
        content: DesynthBase;
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state(<HTMLDivElement>(document.createElement('div')));
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data }: Props = $props();

    const desynthBase: DesynthBase = data.content;

    // Table data
    let tableItems: Reward[] = $state([]);
    let tableColumns: ColumnTemplate[] = $state([]);

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
    getSearchParams();

    onMount(() => {
        if (sourceParam > 0) {
            onButtonClick(sourceParam, desynthBase.Sources, 'Desynths', false)
        } else if (rewardParam > 0) {
            onButtonClick(rewardParam, desynthBase.Rewards, 'Received', false)
        }
    })

    function onButtonClick(id: number, usedData: Record<number, DesynthHistory>, statsType: string, addQuery: boolean) {
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

        let loadedData = usedData[id];

        // Update selected tracking
        selectedId = id;
        selectedStatsType = statsType;

        // Update table data
        tableItems = loadedData.Rewards;
        tableColumns = [
            {
                header: '',
                sortable: false,
                templateRenderer: (row) => {
                    return `<img width="40" height="40" loading="lazy" src="https://v2.xivapi.com/api/asset?path=ui/icon/${IconPaths[row.Id]}_hr1.tex&format=png">`
                },
                classExtension: ['icon']
            },
            {header: 'Name', field: 'Name', valueRenderer: (row) => desynthBase.ToItem[row.Id].Name}, // TODO Fix sorting for field not existing
            {header: 'Obtained', field: 'Amount', classExtension: ['number', 'text-center']},
            {
                header: 'Min-Max',
                field: 'Min',
                valueRenderer: (row) => `${row.Min}–${row.Max}`,
                classExtension: ['number', 'text-center']
            },
            {
                header: 'Chance',
                field: 'Percentage',
                defaultSort: 'asc',
                valueRenderer: (row) => `${(row.Percentage * 100).toFixed(2)}%`,
                classExtension: ['percentage', 'text-end']
            },
        ];

        // Update stats
        titleStats = `${desynthBase.ToItem[id].Name} Stats`;
        totalStats = `${statsType}: ${loadedData.Records.toLocaleString()}`;
        selectedStats = ` `;

        // Update button highlighting in accordion
        for (const element of Object.values(tabElements)) {
            element.classList.remove('btn-success');
        }
        const sourceButton = tabElements[`source-${id}`];
        const rewardButton = tabElements[`reward-${id}`];
        if (sourceButton) sourceButton.classList.add('btn-success');
        if (rewardButton) rewardButton.classList.add('btn-success');
    }

    function getSearchParams() {
        if (page.url.searchParams.has('source')) {
            sourceParam = parseInt(page.url.searchParams.get('source')!);
        }

        if (page.url.searchParams.has('reward')) {
            rewardParam = parseInt(page.url.searchParams.get('reward')!);
        }
    }
</script>

<button class="btn btn-primary btn-lg rounded-xl d-lg-none position-fixed bottom-0 end-0 m-3 w-auto z-3" type="button" data-bs-toggle="offcanvas" data-bs-target="#offcanvasFilter" aria-controls="offcanvasFilter">
    <i class="fas fa-filter"></i>
</button>

<div class="col-12 col-lg-3 order-0 order-lg-1 sticky-left-col">
    <div class="offcanvas-lg offcanvas-start" tabindex="-1" id="offcanvasFilter" aria-labelledby="offcanvasFilterLabel">
        <div class="offcanvas-header">
            <h5 class="offcanvas-title" id="offcanvasFilterLabel">Filter your category</h5>
            <button type="button" class="btn-close" data-bs-dismiss="offcanvas" data-bs-target="#offcanvasFilter" aria-label="Close"></button>
        </div>
        <div class="offcanvas-body">
            <DesynthSearchbar
                {desynthBase}
                {selectedId}
                {onButtonClick}
                {tabElements}
            />
        </div>
    </div>
</div>
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
        {#if tableItems.length > 0 && tableColumns.length > 0}
            <DropsTable items={tableItems} columns={tableColumns} />
        {:else}
            <p>No data found</p>
        {/if}
    </div>
</div>