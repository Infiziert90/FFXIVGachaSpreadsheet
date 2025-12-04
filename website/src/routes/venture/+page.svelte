<script lang="ts">
    import { page } from '$app/state';
    import { replaceState } from "$app/navigation";
    import type { Reward, Venture } from "$lib/interfaces";
    import {Mappings} from "$lib/mappings";
    import {onMount} from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import type {ColumnTemplate} from "$lib/table";
    import { Icon } from '@sveltestrap/sveltestrap';
    import {description, title} from "$lib/title.svelte";
    import VentureAccordion from "../../component/VentureAccordion.svelte";
    import ItemCard from "../../component/ItemCard.svelte";

    interface Props {
        content: Venture[];
    }

    // Set meta data
    title.set('Ventures')
    description.set('Possibilities for all venture tasks.')

    // html elements
    let tabContentElement: HTMLDivElement = $state(<HTMLDivElement>(document.createElement('div')));

    let { data }: Props = $props();
    let patches: string[] = $state([])
    let selectedPatch = $state(0);

    let ventureData: Venture[] = data.content;

    // Table data
    let primaryRewards: Reward[] = $state([]);
    let secondaryRewards: Reward[] = $state([]);
    let tableColumns: ColumnTemplate[] = $state([]);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    for (const patch of Object.keys(ventureData[1].Tasks[0].Patches)) {
        patches.push(patch)
    }

    // Initialize with default values (skipping first because it is a huge quick venture list)
    let category = $state(ventureData[1].Category);
    let taskType = $state(ventureData[1].Tasks[0].Type);
    
    // Override defaults with URL parameters if they exist
    getSearchParams();

    // When page loads, open the tab for the current category/taskType
    onMount(() => {
        openTab(category, taskType, false)
    })

    /**
     * Opens a tab and displays its data
     * @param categoryId - The category to display
     * @param taskTypeId - The task type to display
     * @param addQuery - If true, update the URL with these parameters
     */
    function openTab(categoryId: number, taskTypeId: number, addQuery: boolean = false) {
        // Update state variables
        category = categoryId;
        taskType = taskTypeId;
        
        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('category', category.toString());
            page.url.searchParams.set('task_type', taskType.toString());
            replaceState(page.url, page.state);
        }

        // Show the tab content area
        tabContentElement.style.display = "block";

        // Find the data for the selected category
        const variantData = ventureData.find((e) => e.Category === categoryId);
        if (!variantData) return;

        // Find the specific task variant
        const loadedTask = variantData.Tasks.find((e) => e.Type === taskTypeId);
        if (!loadedTask) return;

        let availablePatches = Object.keys(loadedTask.Patches);

        // Selected patch is invalid, reset to default
        if (availablePatches.length !== patches.length || patches.length < selectedPatch || !availablePatches.includes(patches[selectedPatch])) {
            selectedPatch = 0;
        }

        // Get the patch data for the selected patch
        const requestedPatch = patches[selectedPatch];
        const patchData = loadedTask.Patches[requestedPatch];

        // Update table data
        primaryRewards = patchData.Primaries;
        secondaryRewards = patchData.Secondaries;
        tableColumns = [
            {
                header: '',
                sortable: false,
                templateRenderer: (row) => {
                    return `<img width="40" height="40" loading="lazy" src="https://v2.xivapi.com/api/asset?path=ui/icon/${Mappings[row.Id].Icon}_hr1.tex&format=png" alt="${Mappings[row.Id].Name} Icon">`
                },
                classExtension: ['icon']
            },
            {
                header: 'Name',
                field: 'Id',
                mappingSort: true,
                templateRenderer: (row) => {
                    const name = Mappings[row.Id].Name;
                    const wikiName = name.replace(/\s+/g, '_');
                    return `<a href="https://ffxiv.consolegameswiki.com/wiki/${wikiName}" class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">${name}</a>`
                }
            },
            {
                header: 'Obtained',
                field: 'Amount',
                classExtension: ['number', 'text-center']
            },
            {
                header: 'Total',
                field: 'Total',
                classExtension: ['number', 'text-center']},
            {
                header: 'Min-Max',
                field: 'Min',
                valueRenderer: (row) => `${row.Min}–${row.Max}`,
                classExtension: ['number', 'text-center']
            },
            {
                header: 'Chance',
                field: 'Pct',
                defaultSort: 'asc',
                valueRenderer: (row) => `${(row.Pct * 100).toFixed(2)}%`,
                classExtension: ['percentage', 'text-end']
            },
        ];

        // Update stats display
        titleStats = `${variantData.Name} Stats`;
        totalStats = `${loadedTask.Name}`;
        selectedStats = `${patchData.Total.toLocaleString()}`;

        // Update available patches list for the selected coffer
        patches.length = 0;
        for (const key of availablePatches) {
            patches.push(key);
        }

        // Scroll to the top of the page
        window.scrollTo(0, 0);
    }

    /**
     * Called when user changes the patch selection dropdown
     */
    function patchSelectionChanged(event: Event) {
        if (!event.currentTarget) return;

        // Reload the current tab with the new patch selection
        getSearchParams();
        openTab(category, taskType, false);
    }

    /**
     * Reads territory and coffer from URL parameters
     */
    function getSearchParams() {
        if (page.url.searchParams.has('category') && page.url.searchParams.has('task_type')) {
            category = parseInt(page.url.searchParams.get('category')!);
            taskType = parseInt(page.url.searchParams.get('task_type')!);
        }
    }
</script>

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
            <VentureAccordion
                {ventureData}
                {category}
                {taskType}
                {openTab} 
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
                    <select id="patch" class="form-select" bind:value={selectedPatch} onchange={patchSelectionChanged}>
                        {#each patches as patch, idx}
                            <option value={idx}>
                                {patch}
                            </option>
                        {/each}
                    </select>
                </li>
            </ul>
        </div>
    </div>
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if primaryRewards.length > 0 && secondaryRewards.length > 0}
            <h3>Guaranteed</h3>
            <ItemCard reward={primaryRewards[0]} />
            <h3>Random</h3>
            <DropsTable items={secondaryRewards} columns={tableColumns} />
        {:else}
            {#if primaryRewards.length > 0 && tableColumns.length > 0}
                <DropsTable items={primaryRewards} columns={tableColumns} />
            {:else}
                <p>No data found</p>
            {/if}
        {/if}
    </div>
</div>