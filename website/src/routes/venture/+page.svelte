<script lang="ts">
    import { page } from '$app/state';
    import { replaceState } from "$app/navigation";
    import type {Reward, Venture, VentureTask} from "$lib/interfaces";
    import {onMount} from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import {FullColumnSetup} from "$lib/table";
    import VentureAccordion from "../../component/VentureAccordion.svelte";
    import ItemCard from "../../component/ItemCard.svelte";
    import {tryGetVentureSearchParams} from "$lib/searchParamHelper.ts";
    import PageSidebar from "../../component/PageSidebar.svelte";
    import MultiSelect, {type Option} from "svelte-multiselect";
    import {combineCoffer, combineTaskTotal, combineVenture} from "$lib/patchCombining";

    interface Props {
        content: Venture[];
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;

    let { data }: Props = $props();

    let ventureData: Venture[] = data.content;

    let patches: string[] = $state(Object.keys(ventureData[1].Tasks[0].Patches))
    // svelte-ignore state_referenced_locally
    let selectedPatches: Option[] = $state([...patches.values()])

    // Table data
    let primaryRewards: Reward[] = $state([]);
    let secondaryRewards: Reward[] = $state([]);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // Initialize with default values (skipping first because it is a huge quick venture list)
    let category = $state(ventureData[1].Category);
    let taskType = $state(ventureData[1].Tasks[0].Type);

    // Set default meta data
    let title = $state('Ventures');
    let description = $state('Possibilities for all venture tasks.');

    // Override defaults with URL parameters if they exist
    let ventureSearchParams = tryGetVentureSearchParams(page.url.searchParams);
    if (ventureSearchParams !== undefined) {
        category = ventureSearchParams.categoryId;
        taskType = ventureSearchParams.taskTypeId;

        // svelte-ignore state_referenced_locally
        const selection = tryGetVenture(ventureData, category, taskType);
        if (selection !== undefined) {
            title = `Ventures - ${selection.venture.Name}`;
            description = `Possibilities for ${selection.task.Name}`;
        }
    }

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

        const selection = tryGetVenture(ventureData, category, taskType);
        if (selection === undefined) return;

        // Check if the selected patch is invalid, if so reset to default
        let availablePatches = Object.keys(selection.task.Patches);
        if (availablePatches.length !== patches.length || !selectedPatches.every(v => availablePatches.includes(v.toString()))) {
            // Update the available patches list
            patches.length = 0;
            patches = availablePatches;

            selectedPatches.length = 0;
            selectedPatches = [...patches.values()];
        }

        // Get the patch data for the selected patch
        const patchData = selectedPatches.length === 1
            ? selection.task.Patches[selectedPatches[0]]
            : combineVenture(selection.task.Patches, selectedPatches);

        // Update table data
        primaryRewards = patchData.Primaries;
        secondaryRewards = patchData.Secondaries;

        // Update stats display
        titleStats = `${selection.venture.Name} Stats`;
        totalStats = `${selection.task.Name}`;
        selectedStats = `${combineTaskTotal(selection.task.Patches, selectedPatches).toLocaleString()}`;

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Ventures - ${selection.venture.Name}`
    }

    /**
     * Called when user changes the patch selection dropdown
     */
    function patchSelectionChanged(event: Event) {
        // Reload the current tab with the new patch selection
        openTab(category, taskType, false);
    }

    interface VentureSelection {
        venture: Venture;
        task: VentureTask;
    }

    /**
     * Try to get the specific venture and task.
     * @param data - Dictionary to search through
     * @param categoryId - The category id to resolve
     * @param taskTypeId - The task id to resolve
     * @returns Venture selection if successful, undefined otherwise.
     */
    export function tryGetVenture(data: Venture[], categoryId: number, taskTypeId: number): VentureSelection | undefined {
        // Find the venture for the selected category
        const venture = data.find((e) => e.Category === categoryId);
        if (!venture) return undefined;

        // Find the specific venture task
        const task = venture.Tasks.find((e) => e.Type === taskTypeId);
        if (!task) return undefined;

        return { venture, task };
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>

<PageSidebar>
    <VentureAccordion
        {ventureData}
        {category}
        {taskType}
        {openTab} 
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
                    <label for="patch">Patches</label>
                    <MultiSelect
                            bind:selected={selectedPatches}
                            options={patches}
                            required={true}
                            ulSelectedClass="multiSelect-selection"
                            ulOptionsStyle="padding-left:0.5rem;"
                            onchange={patchSelectionChanged}
                    />
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
            <DropsTable items={secondaryRewards} columns={FullColumnSetup} />
        {:else}
            {#if primaryRewards.length > 0}
                <DropsTable items={primaryRewards} columns={FullColumnSetup} />
            {:else}
                <p>No data found</p>
            {/if}
        {/if}
    </div>
</div>