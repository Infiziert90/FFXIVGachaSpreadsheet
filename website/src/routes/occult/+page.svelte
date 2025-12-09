<script lang="ts">
    import { page } from '$app/state';
    import {replaceState} from "$app/navigation";
    import type {Coffer, Reward} from "$lib/interfaces";
    import {onMount} from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import {FullColumnSetup} from "$lib/table";
    import CofferAccordion from "../../component/CofferAccordion.svelte";
    import { Icon } from '@sveltestrap/sveltestrap';
    import {tryGetCoffer} from "$lib/cofferHelper";
    import {tryGetCofferSearchParams} from "$lib/searchParamHelper";

    interface Props {
        content: Coffer[];
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data }: Props = $props();
    let patches: string[] = $state([])
    let selectedPatch = $state(0);

    let cofferData: Coffer[] = data.content;

    // Table data
    let tableItems: Reward[] = $state([]);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    for (const patch of Object.keys(cofferData[0].Variants[0].Patches)) {
        patches.push(patch)
    }

    // Initialize with default values (first territory and first coffer variant)
    let territory = $state(cofferData[0].TerritoryId);
    let coffer = $state(cofferData[0].Variants[0].Id);

    // Set default meta data
    let title = $state('Occult');
    let description = $state('Possibilities for Treasures, Occult Pots and Bunnies.');

    // Override defaults with URL parameters if they exist
    let cofferSearchParams = tryGetCofferSearchParams(page.url.searchParams);
    if (cofferSearchParams !== undefined) {
        territory = cofferSearchParams.territoryId;
        coffer = cofferSearchParams.cofferId;

        // svelte-ignore state_referenced_locally
        const selection = tryGetCoffer(cofferData, territory, coffer);
        if (selection !== undefined) {
            title = `Occult - ${selection.coffer.Name}`;
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

        const selection = tryGetCoffer(cofferData, territory, coffer);
        if (selection === undefined) return;

        // Check if the selected patch is invalid, if so reset to default
        let availablePatches = Object.keys(selection.variant.Patches);
        if (availablePatches.length !== patches.length || patches.length <= selectedPatch || !availablePatches.includes(patches[selectedPatch])) {
            // Update the available patches list
            patches.length = 0;
            for (const key of availablePatches) {
                patches.push(key);
            }

            selectedPatch = 0;
        }

        // Get the patch data for the selected patch
        const requestedPatch = patches[selectedPatch];
        const patchData = selection.variant.Patches[requestedPatch];

        // Update table data
        tableItems = patchData.Items;

        // Update stats display
        titleStats = `${selection.coffer.Name} Stats`;

        // Calculate total across all variants in this territory
        let total = 0;
        selection.coffer.Variants.forEach(c => {
            total += c.Patches[requestedPatch].Total;
        });
        totalStats = `Total: ${total.toLocaleString()}`;
        selectedStats = `${selection.variant.Name}: ${patchData.Total.toLocaleString()}`;

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Occult - ${selection.coffer.Name}`
    }

    /**
     * Called when user changes the patch selection dropdown
     */
    function patchSelectionChanged(event: Event) {
        if (!event.currentTarget) return;

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
            <CofferAccordion
                {cofferData} 
                {territory}
                {coffer}
                {openTab} 
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
        {#if tableItems.length > 0}
            <DropsTable items={tableItems} columns={FullColumnSetup} />
        {:else}
            <p>No data found</p>
        {/if}
    </div>
</div>