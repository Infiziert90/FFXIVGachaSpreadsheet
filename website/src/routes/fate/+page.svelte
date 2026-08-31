<script lang="ts">
    import { page } from '$app/state';
    import {replaceState} from "$app/navigation";
    import {onMount} from "svelte";
    import {NameObtainedChanceSetup} from "$lib/table";
    import {tryGetFateSearchParams} from "$lib/searchParamHelper";
    import DropsTable from "../../component/DropsTable.svelte";
    import PageSidebar from "../../component/PageSidebar.svelte";
    import type {Fate, FateExpansion, FateReward, FateTerritory, FateType} from "$lib/structs/fate";
    import {
        SimpleDynamicEvent,
        SimpleFate,
        SimpleTerritorySheet
    } from "$lib/sheets/simplifiedSheets.ts";
    import FateAccordion from "../../component/FateAccordion.svelte";

    interface Props {
        data: { content: FateReward };
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;

    let { data }: Props = $props();

    let fateData: FateReward = data.content;

    // Table data
    let tables: Fate[] = $state([]);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // Initialize with default values (expansion, territory, fateType, fate)
    let expansion = $state(fateData.Expansions[0].Id);
    let territory = $state(fateData.Expansions[0].Territories[0].Id);
    let fateType = $state(fateData.Expansions[0].Territories[0].FateTypes[0].Id);

    // Set default meta data
    let title = $state('Fate');
    let description = $state('An overview of all fates and critical engagements in any region.');

    // Override defaults with URL parameters if they exist
    let fateSearchParams = tryGetFateSearchParams(page.url.searchParams);
    if (fateSearchParams !== undefined) {
        expansion = fateSearchParams.expansionId;
        territory = fateSearchParams.territoryId;
        fateType = fateSearchParams.fateTypeId;

        // svelte-ignore state_referenced_locally
        const selection = tryGetFates(expansion, territory, fateType);
        if (selection !== undefined) {
            title = `Fate - ${SimpleTerritorySheet[selection.territory.Id].PlaceName.Name}`;
            description = `A list of all ${selection.fateType.Id === 0 ? 'fates' : 'critical engagements'} and their drop table.`;
        }
    }

    // When page loads, open the tab for the current expansion/territory/fate type
    onMount(() => {
        openTab(expansion, territory, fateType, false)
    })

    /**
     * Opens a tab and displays its data
     * @param expansionId - The expansion ID to display
     * @param territoryId - The territory ID to display
     * @param fateTypeId - The fate type ID to display
     * @param addQuery - If true, update the URL with these parameters
     */
    function openTab(expansionId: number, territoryId: number, fateTypeId: number, addQuery: boolean = false) {
        // Update state variables
        expansion = expansionId;
        territory = territoryId;
        fateType = fateTypeId;

        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('ex', expansionId.toString());
            page.url.searchParams.set('terri', territoryId.toString());
            page.url.searchParams.set('type', fateTypeId.toString());
            replaceState(page.url, page.state);
        }

        // Show the tab content area
        tabContentElement.style.display = "block";

        const selection = tryGetFates(expansion, territory, fateType);
        if (selection === undefined) return;

        // Update table data
        tables = selection.fateType.Fates;

        // Update stats display
        titleStats = `${SimpleTerritorySheet[selection.territory.Id].PlaceName.Name}`;
        totalStats = `${selection.fateType.Id === 0 ? 'Fates' : 'Critical Engagements'}`;
        selectedStats = `${selection.fateType.Records.toLocaleString()}`;

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Fate - ${SimpleTerritorySheet[selection.territory.Id].PlaceName.Name}`;
    }

    interface FateSelection {
        expansion: FateExpansion,
        territory: FateTerritory,
        fateType: FateType,
    }

    /**
     * Try to get the specific expansion, territory and fate type from the data.
     * @param expansionId - The expansion id to resolve
     * @param territoryId - The territory id to resolve
     * @param fateTypeId - The fate type id to resolve
     * @returns FateSelection if successful, undefined otherwise.
     */
    export function tryGetFates(expansionId: number, territoryId: number, fateTypeId: number): FateSelection | undefined {
        // Find the chest drop for the selected category
        const fateExpansion = fateData.Expansions.find((cd) => cd.Id === expansionId);
        if (!fateExpansion) return undefined;

        // Find the specific expansion
        const fateTerritory = fateExpansion.Territories.find((e) => e.Id === territoryId);
        if (!fateTerritory) return undefined;

        // Find the specific header title
        const fateType = fateTerritory.FateTypes.find((h) => h.Id === fateTypeId);
        if (!fateType) return undefined;

        return { expansion: fateExpansion, territory: fateTerritory, fateType: fateType };
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>

<PageSidebar>
    <FateAccordion {fateData} {expansion} {territory} {fateType} {openTab} />
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
            </ul>
        </div>
    </div>
</div>
<div class="col-12 col-lg-7 order-0 order-lg-2">
    <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
        {#each tables as fateTableData}
            <div id="fate-{fateTableData.Id}" class="container mb-5 p-2 rounded border tier-anchor" style="background-color: var(--bs-tertiary-bg);">
                {#if fateType === 0}
                    <h4>{SimpleFate[fateTableData.Id].Name}</h4>
                {:else}
                    <h4>{SimpleDynamicEvent[fateTableData.Id].Name}</h4>
                {/if}
                <p>Records: {fateTableData.Records}</p>
                <DropsTable items={fateTableData.Rewards} columns={NameObtainedChanceSetup} />
            </div>
        {/each}
    </div>
</div>