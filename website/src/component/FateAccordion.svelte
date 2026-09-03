<script lang="ts">
    import Collapse from "./Collapse.svelte";
    import TreeSearchInput from "./TreeSearchInput.svelte";
    import { Icon } from "@sveltestrap/sveltestrap";
    import { getIconPath } from "$lib/utils";
    import { getExpansionStyle } from "$lib/expansionStyles";
    import { createTreeState, getPath, filterLeaves, filterLevel } from "$lib/treeNodes.svelte";
    import type {FateReward, FateExpansion, FateTerritory} from "$lib/structs/fate";
    import {SimpleExVersion, SimpleTerritorySheet} from "$lib/sheets/simplifiedSheets";

    interface Props {
        fateData: FateReward;
        expansion: number;
        territory: number;
        fateType: number;
        openTab: (expansionId: number, territoryId: number, fateTypeId: number, addQuery: boolean) => void;
    }

    let { fateData, expansion, territory, fateType, openTab }: Props = $props();

    // Search state
    let searchQuery = $state('');
    const trimmedQuery = $derived(searchQuery.trim().toLowerCase());
    const isSearching = $derived(trimmedQuery !== '');

    /**
     * Filters the territory/expansion tree down to territories whose name matches
     * the query, or expansions whose own name matches (which keeps all their
     * territories). Fate types have no name of their own, so they always follow
     * their parent territory.
     */
    function filterTerritories(territories: FateTerritory[], query: string): FateTerritory[] {
        return filterLeaves(territories, query, t => SimpleTerritorySheet[t.Id]?.PlaceName?.Name || '');
    }

    function filterExpansions(expansions: FateExpansion[], query: string): FateExpansion[] {
        return filterLevel(expansions, query, e => SimpleExVersion[e.Id]?.Name || '', e => e.Territories, (e, Territories) => ({ ...e, Territories }), filterTerritories);
    }

    const filteredData = $derived(isSearching ? filterExpansions(fateData.Expansions, trimmedQuery) : fateData.Expansions);

    const tree = createTreeState();
    const { isOpen } = tree;

    /**
     * Ensures the path to the current selection is open.
     * Closes siblings at each level to maintain single-open-per-level behavior.
     * Automatically opens nodes that have only one child.
     */
    function ensureSelectionPathOpen() {
        const expansionEntry = fateData.Expansions.find(e => e.Id === expansion);
        if (!expansionEntry) return;

        const expansionPath = getPath('', expansion, 'expansion');
        tree.openExclusive(expansionPath);

        const territoryEntry = expansionEntry.Territories.find(e => e.Id === territory);
        if (!territoryEntry) return;

        const territoryPath = getPath(expansionPath, territory, 'territory');
        tree.openExclusive(territoryPath);

        // Auto-open if only one header exists
        if (territoryEntry.FateTypes.length === 1) {
            const headerPath = getPath(territoryPath, territoryEntry.FateTypes[0].Id, 'fateType');
            tree.openExclusive(headerPath);
        } else {
            const headerPath = getPath(territoryPath, fateType, 'fateType');
            tree.openExclusive(headerPath);
        }
    }

    // Track previous selection to only sync when selection changes externally
    let previousSelection = $state<string>('');

    // Ensure selection path is open when selection props change externally
    // This follows the same pattern as CofferAccordion and VentureAccordion
    $effect(() => {
        const currentSelection = `${expansion}-${territory}-${fateType}`;
        // Only sync if the selection actually changed (external prop change)
        if (currentSelection !== previousSelection) {
            previousSelection = currentSelection;
            ensureSelectionPathOpen();
        }
    });
</script>

<div class="d-flex flex-column gap-2 w-100">
    <TreeSearchInput bind:value={searchQuery} placeholder="Search areas..." ariaLabel="Search areas" />

    <div class="loot-tree w-100">
        {#if isSearching && filteredData.length === 0}
            <p class="text-muted m-0 p-2">No area found</p>
        {:else}
            {#each filteredData as expansionEntry (expansionEntry.Id)}
                {@render renderExpansion(expansionEntry, '')}
            {/each}
        {/if}
    </div>
</div>

{#snippet renderExpansion(expansionEntry: FateExpansion, parentPath: string)}
    {@const path = getPath(parentPath, expansionEntry.Id, 'expansion')}
    {@const open = isSearching || isOpen(path)}

        {@const expansionStyle = getExpansionStyle(SimpleExVersion[expansionEntry.Id]?.Name || '')}
        <div
                class="tree-node-folder d-flex align-items-center gap-2 user-select-none"
                data-open={open ? 'true' : 'false'}
                style={expansionStyle ? `--node-hue: ${expansionStyle.hue}` : ''}
                role="button"
                tabindex="0"
                onclick={() => tree.toggle(path)}
                onkeydown={(e) => e.key === 'Enter' && tree.toggle(path)}
        >
            <span class="tree-icon d-inline-flex align-items-center">
                <Icon name={open ? 'chevron-down' : 'chevron-right'} />
            </span>
            <span class="tree-label flex-grow-1 d-flex align-items-center gap-2">
                {#if expansionStyle}
                    <img
                            src={getIconPath(expansionStyle.icon, true)}
                            alt=""
                            class="expansion-icon"
                    />
                {/if}
                {SimpleExVersion[expansionEntry.Id]?.Name || expansionEntry.Id}
            </span>
        </div>

        <Collapse isOpen={open} animate={!isSearching} style={expansionStyle ? `--node-hue: ${expansionStyle.hue}` : ''}>
            <div class="tree-indent">
                {#each expansionEntry.Territories as territoryEntry (territoryEntry.Id)}
                    {@render renderTerritory(territoryEntry, path, expansionEntry.Id)}
                {/each}
            </div>
        </Collapse>
{/snippet}

{#snippet renderTerritory(territoryEntry: FateTerritory, parentPath: string, expansionId: number)}
    {@const path = getPath(parentPath, territoryEntry.Id, 'territory')}

    {@const open = isSearching || isOpen(path)}
        <div
            class="tree-node-folder d-flex align-items-center gap-2 user-select-none"
            data-open={open ? 'true' : 'false'}
            role="button"
            tabindex="0"
            onclick={() => tree.toggle(path)}
            onkeydown={(e) => e.key === 'Enter' && tree.toggle(path)}
            style="--node-hue: 216"
        >
            <span class="tree-icon d-inline-flex align-items-center">
                <Icon name={open ? 'chevron-down' : 'chevron-right'} />
            </span>
            <span class="tree-label flex-grow-1">{SimpleTerritorySheet[territoryEntry.Id]?.PlaceName?.Name || territoryEntry.Id}</span>
        </div>
        <Collapse isOpen={open} animate={!isSearching} style="--node-hue: 216">
            <div class="tree-indent">
                {#each territoryEntry.FateTypes as fateTypeEntry (fateTypeEntry.Id)}
                    <button
                            id="{expansionId}{territoryEntry.Id}{fateTypeEntry.Id}-tab"
                            class="tree-node-element w-100 text-start border-0"
                            class:active={expansion === expansionId && territory === territoryEntry.Id && fateType === fateTypeEntry.Id}
                            onclick={() => openTab(expansionId, territoryEntry.Id, fateTypeEntry.Id, true)}
                    >
                        {fateTypeEntry.Id === 0 ? 'Fates' : fateTypeEntry.Id === 1 ? 'Critical Engagements' : fateTypeEntry.Id}
                    </button>
                {/each}
            </div>
        </Collapse>
{/snippet}
