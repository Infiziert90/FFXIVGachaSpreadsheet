<script lang="ts">
    import { page } from '$app/state';
    import {replaceState} from "$app/navigation";
    import type {ChestDrop, Coffer, Reward} from "$lib/interfaces";
    import {Mappings} from "$lib/mappings";
    import {onMount} from "svelte";
    import DropsTable from "../../component/DropsTable.svelte";
    import type {ColumnTemplate} from "$lib/table";
    import CofferAccordion from "../../component/CofferAccordion.svelte";
    import {Accordion, AccordionItem, Icon, ListGroup, ListGroupItem} from '@sveltestrap/sveltestrap';
    import {description, title} from "$lib/title.svelte";

    interface Props {
        content: ChestDrop[];
    }

    // Set meta data
    title.set('Duty Loot')
    description.set('Possibilities for all types of chest loot in Duties, Trials, Raids and more.')

    // html elements
    let tabContentElement: HTMLDivElement = $state(<HTMLDivElement>(document.createElement('div')));
    let tabElements: {[key: string]: HTMLButtonElement} = $state({});

    let { data }: Props = $props();
    let patches: string[] = $state([])
    let selectedPatch = $state(0);

    let chestDropData: ChestDrop[] = data.content;

    // Table data
    let tables: Record<number, Reward[]> = $state({});
    let tableColumns: ColumnTemplate[] = $state([]);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // for (const patch of Object.keys(chestDropData[0].Variants[0].Patches)) {
    //     patches.push(patch)
    // }

    // Initialize with default values (first territory and first coffer variant)
    let category = $state(chestDropData[0].Id);
    let expansion = $state(chestDropData[0].Expansions[0].Id);
    let header = $state(chestDropData[0].Expansions[0].Headers[0].Id);
    let duty = $state(chestDropData[0].Expansions[0].Headers[0].Duties[0].Id);

    // Override defaults with URL parameters if they exist
    getSearchParams();

    // When page loads, open the tab for the current territory/coffer
    onMount(() => {
        openTab(category, expansion, header, duty, false)
    })

    /**
     * Opens a tab and displays its data
     * @param categoryId - The territory ID to display
     * @param expansionId - The coffer variant ID to display
     * @param headerId - The coffer variant ID to display
     * @param dutyId - The coffer variant ID to display
     * @param addQuery - If true, update the URL with these parameters
     */
    function openTab(categoryId: number, expansionId: number, headerId: number, dutyId: number, addQuery: boolean = false) {
        // Update state variables
        category = categoryId;
        expansion = expansionId;
        header = headerId;
        duty = dutyId;

        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('category', category.toString());
            page.url.searchParams.set('expansion', expansion.toString());
            page.url.searchParams.set('header', header.toString());
            page.url.searchParams.set('duty', duty.toString());
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
        const buttonKey = `${categoryId}${expansionId}`;
        if (tabElements[buttonKey]) {
            tabElements[buttonKey].classList.add('active');
        }

        // Find the chest drop for the selected category
        const chestDropEntry = chestDropData.find((cd) => cd.Id === categoryId);
        if (!chestDropEntry) return;

        // Find the specific expansion
        const expansionDetail = chestDropEntry.Expansions.find((e) => e.Id === expansionId);
        if (!expansionDetail) return;

        // Find the specific header title
        const headerDetail = expansionDetail.Headers.find((h) => h.Id === headerId);
        if (!headerDetail) return;

        // Find the specific duty
        const dutyDetail = headerDetail.Duties.find((d) => d.Id === dutyId);
        if (!dutyDetail) return;

        // Update table data
        tables = {};
        dutyDetail.Chests.forEach((c) => {
            tables[c.Id] = c.Rewards;
        })
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
                classExtension: ['number', 'text-center']
            },
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
        titleStats = `${chestDropEntry.Name} Stats`;
        totalStats = `${dutyDetail.Name}`;
        selectedStats = `${dutyDetail.Records.toLocaleString()}`;

        // // Update available patches list for the selected coffer
        // patches.length = 0;
        // for (const key of Object.keys(expansionDetail.Patches)) {
        //     patches.push(key);
        // }

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
        openTab(category, expansion, header, duty, false);
    }

    /**
     * Reads territory and coffer from URL parameters
     */
    function getSearchParams() {
        if (page.url.searchParams.has('territory') && page.url.searchParams.has('coffer')) {
            category = parseInt(page.url.searchParams.get('territory')!);
            expansion = parseInt(page.url.searchParams.get('coffer')!);
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
            <Accordion class="w-100">
                {#each chestDropData as chestDropEntry}
                    <AccordionItem
                            active={category === chestDropEntry.Id}
                            on:toggle={(e) => handleToggle(chestDropEntry.Id, e)}
                    >
                        <div slot="header">{chestDropEntry.Name}</div>
                        <Accordion class="w-100">
                            {#each chestDropEntry.Expansions as expansionEntry}
                                <AccordionItem
                                        active={expansion === expansionEntry.Id}
                                        on:toggle={(e) => handleToggle(expansionEntry.Id, e)}
                                >
                                    <div slot="header">{expansionEntry.Name}</div>
                                    <Accordion class="w-100">
                                        {#each expansionEntry.Headers as headerEntry}
                                            <AccordionItem
                                                    active={header === headerEntry.Id}
                                                    on:toggle={(e) => handleToggle(headerEntry.Id, e)}
                                            >
                                                <div slot="header">{headerEntry.Name}</div>
                                                <ListGroup>
                                                    {#each headerEntry.Duties as dutyEntry}
                                                        <ListGroupItem
                                                                id="{chestDropEntry.Id}{expansionEntry.Id}{headerEntry.Id}{dutyEntry.Id}-tab"
                                                                active={category === chestDropEntry.Id && expansion === expansionEntry.Id && header === headerEntry.Id && duty === dutyEntry.Id}
                                                                tag="button"
                                                                action
                                                                on:click={() => openTab(chestDropEntry.Id, expansionEntry.Id, headerEntry.Id, dutyEntry.Id, true)}
                                                        >
                                                            {dutyEntry.Name}
                                                        </ListGroupItem>
                                                    {/each}
                                                </ListGroup>
                                            </AccordionItem>
                                        {/each}
                                    </Accordion>
                                </AccordionItem>
                            {/each}
                        </Accordion>
                    </AccordionItem>
                {/each}
            </Accordion>
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
<!--                    <select id="patch" class="form-select" bind:value={selectedPatch} onchange={patchSelectionChanged}>-->
<!--                        {#each patches as patch, idx}-->
<!--                            <option value={idx}>-->
<!--                                {patch}-->
<!--                            </option>-->
<!--                        {/each}-->
<!--                    </select>-->
                </li>
            </ul>
        </div>
    </div>
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#each Object.entries(tables) as [tableId, tableItems]}
            {#if tableItems.length > 0 && tableColumns.length > 0}
                <DropsTable items={tableItems} columns={tableColumns} />
            {:else}
                <p>No data found</p>
            {/if}
        {/each}
    </div>
</div>