<script lang="ts">
    import { page } from '$app/state';
    import {replaceState} from "$app/navigation";
    import type {Coffer, Reward} from "$lib/interfaces";
    import {onMount} from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import {NameObtainedChanceSetup} from "$lib/table";
    import CofferAccordion from "../../component/CofferAccordion.svelte";
    import {tryGetCofferSearchParams} from "$lib/searchParamHelper";
    import {tryGetCoffer} from "$lib/cofferHelper";
    import MultiSelect, {type Option} from 'svelte-multiselect'
    import {combineCoffer, combineVariantTotal} from "$lib/patchCombining";
    import PageSidebar from "../../component/PageSidebar.svelte";

    interface Props {
        content: Coffer[];
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data }: Props = $props();
    let cofferData: Coffer[] = data.content;

    let patches: string[] = $state(Object.keys(cofferData[0].Variants[0].Patches))
    // svelte-ignore state_referenced_locally
    let selectedPatches: Option[] = $state([...patches.values()])

    // Table data
    let tableItems: Reward[] = $state([]);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // Initialize with default values (first territory and first coffer variant)
    let territory = $state(cofferData[0].TerritoryId);
    let coffer = $state(cofferData[0].Variants[0].Id);

    // Set default meta data
    let title = $state('Random Coffers');
    let description = $state('Possibilities for all types of coffers, e.g Venture Coffer, or Materiel Container 3.0.');

    // Override defaults with URL parameters if they exist
    let cofferSearchParams = tryGetCofferSearchParams(page.url.searchParams);
    if (cofferSearchParams !== undefined) {
        territory = cofferSearchParams.territoryId;
        coffer = cofferSearchParams.cofferId;

        // svelte-ignore state_referenced_locally
        const selection = tryGetCoffer(cofferData, territory, coffer);
        if (selection !== undefined) {
            title = `Random Coffers - ${selection.coffer.Name}`;
            description = `Possibilities for ${selection.variant.Name}`;
        }
    }

    // When page loads, open the tab for the current territory/coffer
    onMount(() => {
        openTab(territory, coffer, false)
    })

    /**
     * Opens a tab and displays its data
     * @param territoryId - The territory ID to display
     * @param cofferId - The coffer variant ID to display
     * @param addQuery - If true, update the URL with these parameters
     */
    function openTab(territoryId: number, cofferId: number, addQuery: boolean = false) {
        // Update state variables
        territory = territoryId;
        coffer = cofferId;
        
        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('territory', territoryId.toString());
            page.url.searchParams.set('coffer', cofferId.toString());
            replaceState(page.url, page.state);
        }

        // Remove active class from all buttons
        // The accordion component will add it back to the correct button
        for (const element of Object.values(tabElements)) {
            element?.classList.remove('active');
        }

        // Show the tab content area
        tabContentElement.style.display = "block";
        
        // Mark the clicked button as active (if it exists)
        // Note: The accordion component also handles this, but we do it here too for immediate feedback
        const buttonKey = `${territoryId}${cofferId}`;
        if (tabElements[buttonKey]) {
            tabElements[buttonKey].classList.add('active');
        }

        const selection = tryGetCoffer(cofferData, territoryId, cofferId);
        if (selection === undefined) return;

        // Check if the selected patch is invalid, if so reset to default
        let availablePatches = Object.keys(selection.variant.Patches);
        if (availablePatches.length !== patches.length || !selectedPatches.every(v => availablePatches.includes(v.toString()))) {
            // Update the available patches list
            patches.length = 0;
            patches = availablePatches;

            selectedPatches.length = 0;
            selectedPatches = [...patches.values()];
        }

        // Get the patch data for the selected patch
        const patchData = selectedPatches.length === 1
            ? selection.variant.Patches[selectedPatches[0]]
            : combineCoffer(selection.variant.Patches, selectedPatches);

        // Update table data
        tableItems = patchData.Items;

        // Update stats display
        titleStats = `${selection.coffer.Name} Stats`;

        // Calculate total across all variants in this territory
        totalStats = `Total: ${combineVariantTotal(selection.coffer.Variants, selectedPatches).toLocaleString()}`;
        selectedStats = `${selection.variant.Name}: ${patchData.Total.toLocaleString()}`;

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Random Coffers - ${selection.coffer.Name}`
    }

    /**
     * Called when user changes the patch selection dropdown
     */
    function patchSelectionChanged(event: Event) {
        // Reload the current tab with the new patch selection
        openTab(territory, coffer, false);
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
    <CofferAccordion
        {cofferData} 
        {territory}
        {coffer}
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
                <li class="list-group-item">{selectedStats}</li>
                <li class="list-group-item">
                    <label for="patch">Patches</label>
                    <MultiSelect
                            bind:selected={selectedPatches}
                            options={patches}
                            required={true}
                            ulSelectedStyle="width: 91%;"
                            ulOptionsStyle="background-color: var(--bs-body-bg);"
                            onchange={patchSelectionChanged}
                    />
                </li>
            </ul>
        </div>
    </div>
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#if tableItems.length > 0}
            <DropsTable items={tableItems} columns={NameObtainedChanceSetup} />
        {:else}
            <p>No data found</p>
        {/if}
    </div>
</div>