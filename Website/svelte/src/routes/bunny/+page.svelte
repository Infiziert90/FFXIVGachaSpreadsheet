<script lang="ts">
    import { page } from '$app/state';
    import {replaceState} from "$app/navigation";
    import type {Coffer, Reward} from "$lib/interfaces";
    import {Mappings} from "$lib/mappings";
    import {onMount} from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import type {ColumnTemplate} from "$lib/table";
    import CofferAccordion from "../../component/CofferAccordion.svelte";
    import { Icon } from '@sveltestrap/sveltestrap';
    import {description, title} from "$lib/title.svelte";

    interface Props {
        content: Coffer[];
    }

    // Set meta data
    title.set('Eureka Bunnies')
    description.set('Possibilities for eureka bunny coffer content.')

    // html elements
    let tabContentElement: HTMLDivElement = $state(<HTMLDivElement>(document.createElement('div')));
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data }: Props = $props();
    let patches: string[] = $state([])
    let selectedPatch = $state(0);

    let cofferData: Coffer[] = data.content;

    // Table data
    let tableItems: Reward[] = $state([]);
    let tableColumns: ColumnTemplate[] = $state([]);

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
    
    // Override defaults with URL parameters if they exist
    getSearchParams();

    // When page loads, open the tab for the current territory/coffer
    onMount(() => {
        openTab(territory, coffer, false)
    })

    /**
     * Opens a tab and displays its data
     * @param territory - The territory ID to display
     * @param coffer - The coffer variant ID to display
     * @param addQuery - If true, update the URL with these parameters
     */
    function openTab(territory: number, coffer: number, addQuery: boolean = false) {
        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('territory', territory.toString());
            page.url.searchParams.set('coffer', coffer.toString());
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
        const buttonKey = `${territory}${coffer}`;
        if (tabElements[buttonKey]) {
            tabElements[buttonKey].classList.add('active');
        }

        // Find the coffer data for the selected territory
        const variantData = cofferData.find((e) => e.TerritoryId === territory);
        if (!variantData) return;

        // Find the specific coffer variant
        const loadedCoffer = variantData.Variants.find((e) => e.Id === coffer);
        if (!loadedCoffer) return;

        // Get the patch data for the selected patch
        const requestedPatch = patches[selectedPatch];
        const patchData = loadedCoffer.Patches[requestedPatch];

        // Update table data
        tableItems = patchData.Items;
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
                header: 'Chance',
                field: 'Pct',
                defaultSort: 'asc',
                valueRenderer: (row) => `${(row.Pct * 100).toFixed(2)}%`,
                classExtension: ['percentage', 'text-end']
            },
        ];

        // Update stats display
        titleStats = `${variantData.Name} Stats`;

        // Calculate total across all variants in this territory
        let total = 0;
        variantData.Variants.forEach(coffer => {
            total += coffer.Patches[requestedPatch].Total;
        });
        totalStats = `Total: ${total.toLocaleString()}`;
        selectedStats = `${loadedCoffer.Name}: ${patchData.Total.toLocaleString()}`;

        // Update available patches list for the selected coffer
        patches.length = 0;
        for (const key of Object.keys(loadedCoffer.Patches)) {
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
        openTab(territory, coffer, false);
    }

    /**
     * Reads territory and coffer from URL parameters
     */
    function getSearchParams() {
        if (page.url.searchParams.has('territory') && page.url.searchParams.has('coffer')) {
            territory = parseInt(page.url.searchParams.get('territory')!);
            coffer = parseInt(page.url.searchParams.get('coffer')!);
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
        {#if tableItems.length > 0 && tableColumns.length > 0}
            <DropsTable items={tableItems} columns={tableColumns} />
        {:else}
            <p>No data found</p>
        {/if}
    </div>
</div>