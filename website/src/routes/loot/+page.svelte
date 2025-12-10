<script lang="ts">
    import { page } from '$app/state';
    import {replaceState} from "$app/navigation";
    import type {ChestDrop, Duty, Expansion, Header, Reward} from "$lib/interfaces";
    import {onMount} from "svelte";
    import {FullColumnSetup} from "$lib/table";
    import { Icon } from '@sveltestrap/sveltestrap';
    import {tryGetDutyLootSearchParams} from "$lib/searchParamHelper";
    import DropsTable from "../../component/DropsTable.svelte";
    // import LootAccordion from "../../component/LootAccordion.svelte";

    interface Props {
        data: { content: ChestDrop[] };
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;

    let { data }: Props = $props();

    let chestDropData: ChestDrop[] = data.content;

    // Table data
    let tables: Record<number, Reward[]> = $state({});

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // Initialize with default values (first category, expansion, header, and duty)
    let category = $state(chestDropData[0].Id);
    let expansion = $state(chestDropData[0].Expansions[0].Id);
    let header = $state(chestDropData[0].Expansions[0].Headers[0].Id);
    let duty = $state(chestDropData[0].Expansions[0].Headers[0].Duties[0].Id);

    // Set default meta data
    let title = $state('Duty Loot');
    let description = $state('Possibilities for all types of chest loot in Duties, Trials, Raids and more.');

    // Override defaults with URL parameters if they exist
    let dutyLootSearchParams = tryGetDutyLootSearchParams(page.url.searchParams);
    if (dutyLootSearchParams !== undefined) {
        category = dutyLootSearchParams.categoryId;
        expansion = dutyLootSearchParams.expansionId;
        header = dutyLootSearchParams.headerId;
        duty = dutyLootSearchParams.dutyId;

        // svelte-ignore state_referenced_locally
        const selection = tryGetChestDrop(chestDropData, category, expansion, header, duty);
        if (selection !== undefined) {
            title = `Duty Loot - ${selection.chestDrop.Name}`;
            description = `All possible drops in ${selection.duty.Name}`;
        }
    }

    // When page loads, open the tab for the current category/expansion/header/duty
    onMount(() => {
        openTab(category, expansion, header, duty, false)
    })

    /**
     * Opens a tab and displays its data
     * @param categoryId - The category ID to display
     * @param expansionId - The expansion ID to display
     * @param headerId - The header ID to display
     * @param dutyId - The duty ID to display
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

        // Show the tab content area
        tabContentElement.style.display = "block";

        const selection = tryGetChestDrop(chestDropData, category, expansion, header, duty);
        if (selection === undefined) return;

        // Update table data
        tables = {};
        selection.duty.Chests.forEach((c) => {
            tables[c.Id] = c.Rewards;
        })

        // Update stats display
        titleStats = `${selection.chestDrop.Name} Stats`;
        totalStats = `${selection.duty.Name}`;
        selectedStats = `${selection.duty.Records.toLocaleString()}`;

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Duty Loot - ${selection.chestDrop.Name}`;
    }

    interface DutyLootSelection {
        chestDrop: ChestDrop,
        expansion: Expansion,
        header: Header,
        duty: Duty,
    }

    /**
     * Try to get the specific category, expansion, header and duty from the data.
     * @param data - Dictionary to search through
     * @param categoryId - The category id to resolve
     * @param expansionId - The expansion id to resolve
     * @param headerId - The header id to resolve
     * @param dutyId - The duty id to resolve
     * @returns DutyLoot selection if successful, undefined otherwise.
     */
    export function tryGetChestDrop(data: ChestDrop[], categoryId: number, expansionId: number, headerId: number, dutyId: number): DutyLootSelection | undefined {
        // Find the chest drop for the selected category
        const chestDrop = data.find((cd) => cd.Id === categoryId);
        if (!chestDrop) return undefined;

        // Find the specific expansion
        const expansion = chestDrop.Expansions.find((e) => e.Id === expansionId);
        if (!expansion) return undefined;

        // Find the specific header title
        const header = expansion.Headers.find((h) => h.Id === headerId);
        if (!header) return undefined;

        // Find the specific duty
        const duty = header.Duties.find((d) => d.Id === dutyId);
        if (!duty) return undefined;

        return { chestDrop, expansion, header, duty };
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
<!--            <LootAccordion {chestDropData} {category} {expansion} {header} {duty} {openTab} />-->
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
            </ul>
        </div>
    </div>
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#each Object.entries(tables) as [tableId, tableItems]}
            {#if tableItems.length > 0}
                <DropsTable items={tableItems} columns={FullColumnSetup} />
            {:else}
                <p>No data found</p>
            {/if}
        {/each}
    </div>
</div>