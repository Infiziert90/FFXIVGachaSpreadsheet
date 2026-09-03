<script lang="ts">
    import Collapse from "./Collapse.svelte";
    import TreeSearchInput from "./TreeSearchInput.svelte";
    import { Icon } from "@sveltestrap/sveltestrap";
    import { getIconPath } from "$lib/utils";
    import { iconIdToPath, getExpansionStyle } from "$lib/expansionStyles";
    import { createTreeState, getPath, filterLeaves, filterLevel } from "$lib/treeNodes.svelte";
    import type {ChestDrop, Expansion, Header, Duty} from "$lib/structs/chestDrop";

    interface Props {
        chestDropData: ChestDrop[];
        category: number;
        expansion: number;
        header: number;
        duty: number;
        openTab: (category: number, expansion: number, header: number, duty: number, addQuery: boolean) => void;
    }

    let { chestDropData, category, expansion, header, duty, openTab }: Props = $props();

    interface CategoryStyle {
        icon: string;
        hue: number;
    }

    const categoryStyles: Record<string | number, CategoryStyle> = {
        'Dungeons': { icon: iconIdToPath(60831), hue: 188 },
        'Dungeon': { icon: iconIdToPath(60831), hue: 188 },
        'Raids': { icon: iconIdToPath(60832), hue: 40 },
        'Raid': { icon: iconIdToPath(60832), hue: 40 },
        'Trials': { icon: iconIdToPath(60834), hue: 2 },
        'Trial': { icon: iconIdToPath(60834), hue: 2 },
        'Treasure Hunts': { icon: iconIdToPath(60838), hue: 199 },
        'Treasure Hunt': { icon: iconIdToPath(60838), hue: 199 },
        'V&C Dungeon Finder': { icon: iconIdToPath(61846), hue: 262 },
        'Chaotic Alliance Raids': { icon: iconIdToPath(60855), hue: 280 },
        'Chaotic Alliance Raid': { icon: iconIdToPath(60855), hue: 280 },
        'Open World': { icon: iconIdToPath(60857), hue: 46 },
        'Alliance Raids': { icon: iconIdToPath(61804), hue: 320 },
        'Alliance Raid': { icon: iconIdToPath(61804), hue: 320 },
        'Deep Dungeons': { icon: iconIdToPath(61805), hue: 60 },
        'Deep Dungeon': { icon: iconIdToPath(61805), hue: 60 },
        'Ultimate Raids': { icon: iconIdToPath(61806), hue: 340 },
        'Ultimate': { icon: iconIdToPath(61806), hue: 340 },
        'Extreme Trials': { icon: iconIdToPath(61807), hue: 30 },
        'Extreme': { icon: iconIdToPath(61807), hue: 30 },
        'Savage Raids': { icon: iconIdToPath(61808), hue: 270 },
        'Savage': { icon: iconIdToPath(61808), hue: 270 },
    };

    function getCategoryStyle(categoryName: string, categoryId: number): CategoryStyle | null {
        return categoryStyles[categoryName] || categoryStyles[categoryId] || null;
    }

    // Search state
    let searchQuery = $state('');
    const trimmedQuery = $derived(searchQuery.trim().toLowerCase());
    const isSearching = $derived(trimmedQuery !== '');

    /**
     * Filters the duty/header/expansion/category tree down to nodes whose name
     * matches the query, or that have a matching descendant. If a node's own
     * name matches, all of its children are kept as-is (no further filtering).
     */
    function filterDuties(duties: Duty[], query: string): Duty[] {
        return filterLeaves(duties, query, d => d.Name || '');
    }

    function filterHeaders(headers: Header[], query: string): Header[] {
        return filterLevel(headers, query, h => h.Name || '', h => h.Duties, (h, Duties) => ({ ...h, Duties }), filterDuties);
    }

    function filterExpansions(expansions: Expansion[], query: string): Expansion[] {
        return filterLevel(expansions, query, e => e.Name || '', e => e.Headers, (e, Headers) => ({ ...e, Headers }), filterHeaders);
    }

    function filterCategories(categories: ChestDrop[], query: string): ChestDrop[] {
        return filterLevel(categories, query, c => c.Name || '', c => c.Expansions, (c, Expansions) => ({ ...c, Expansions }), filterExpansions);
    }

    const filteredData = $derived(isSearching ? filterCategories(chestDropData, trimmedQuery) : chestDropData);

    const tree = createTreeState();
    const { isOpen } = tree;

    /**
     * Ensures the path to the current selection is open.
     * Closes siblings at each level to maintain single-open-per-level behavior.
     * Automatically opens nodes that have only one child.
     */
    function ensureSelectionPathOpen() {
        const categoryEntry = chestDropData.find(c => c.Id === category);
        if (!categoryEntry) return;

        const categoryPath = getPath('', category, 'category');
        tree.openExclusive(categoryPath);

        const expansionEntry = categoryEntry.Expansions.find(e => e.Id === expansion);
        if (!expansionEntry) return;

        const expansionPath = getPath(categoryPath, expansion, 'expansion');
        tree.openExclusive(expansionPath);

        // Auto-open if only one header exists
        if (expansionEntry.Headers.length === 1) {
            const headerPath = getPath(expansionPath, expansionEntry.Headers[0].Id, 'header');
            tree.openExclusive(headerPath);
        } else {
            const headerPath = getPath(expansionPath, header, 'header');
            tree.openExclusive(headerPath);
        }
    }

    // Track previous selection to only sync when selection changes externally
    let previousSelection = $state<string>('');

    // Ensure selection path is open when selection props change externally
    // This follows the same pattern as CofferAccordion and VentureAccordion
    $effect(() => {
        const currentSelection = `${category}-${expansion}-${header}-${duty}`;
        // Only sync if the selection actually changed (external prop change)
        if (currentSelection !== previousSelection) {
            previousSelection = currentSelection;
            ensureSelectionPathOpen();
        }
    });
</script>

<div class="d-flex flex-column gap-2 w-100">
    <TreeSearchInput bind:value={searchQuery} placeholder="Search duties..." ariaLabel="Search duties" />

    <div class="loot-tree w-100">
        {#if isSearching && filteredData.length === 0}
            <p class="text-muted m-0 p-2">No duty found</p>
        {:else}
            {#each filteredData as chestDropEntry (chestDropEntry.Id)}
                {@render renderCategory(chestDropEntry, '')}
            {/each}
        {/if}
    </div>
</div>

{#snippet renderCategory(chestDropEntry: ChestDrop, parentPath: string)}
    {@const path = getPath(parentPath, chestDropEntry.Id, 'category')}
    {@const open = isSearching || isOpen(path)}

        {@const categoryStyle = getCategoryStyle(chestDropEntry.Name, chestDropEntry.Id)}
        <div
                class="tree-node-folder d-flex align-items-center gap-2 user-select-none"
                data-open={open ? 'true' : 'false'}
                style={categoryStyle ? `--node-hue: ${categoryStyle.hue}` : ''}
                role="button"
                tabindex="0"
                onclick={() => tree.toggle(path)}
                onkeydown={(e) => e.key === 'Enter' && tree.toggle(path)}
        >
            <span class="tree-icon d-inline-flex align-items-center">
                <Icon name={open ? 'chevron-down' : 'chevron-right'} />
            </span>
            <span class="tree-label flex-grow-1 d-flex align-items-center gap-2">
                {#if categoryStyle}
                    <img
                            src={getIconPath(categoryStyle.icon, true)}
                            alt=""
                            class="category-icon"
                    />
                {/if}
                {chestDropEntry.Name || chestDropEntry.Id}
            </span>
        </div>

        <Collapse isOpen={open} animate={!isSearching} style={categoryStyle ? `--node-hue: ${categoryStyle.hue}` : ''}>
            <div class="tree-indent">
                {#each chestDropEntry.Expansions as expansionEntry (expansionEntry.Id)}
                    {@render renderExpansion(expansionEntry, path, chestDropEntry.Id)}
                {/each}
            </div>
        </Collapse>
{/snippet}

{#snippet renderExpansion(expansionEntry: Expansion, parentPath: string, categoryId: number)}
    {@const path = getPath(parentPath, expansionEntry.Id, 'expansion')}
    {@const open = isSearching || isOpen(path)}
    {@const hasSingleHeader = expansionEntry.Headers.length === 1}

        {@const expansionStyle = getExpansionStyle(expansionEntry.Name)}
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
                {expansionEntry.Name || expansionEntry.Id}
            </span>
        </div>

        <Collapse isOpen={open} animate={!isSearching} style={expansionStyle ? `--node-hue: ${expansionStyle.hue}` : ''}>
            <div class="tree-indent">
                {#if hasSingleHeader}
                    {@const headerEntry = expansionEntry.Headers[0]}
                    {#each headerEntry.Duties as dutyEntry (dutyEntry.Id)}
                        <button
                                id="{categoryId}{expansionEntry.Id}{headerEntry.Id}{dutyEntry.Id}-tab"
                                class="tree-node-element w-100 text-start border-0"
                                class:active={category === categoryId && expansion === expansionEntry.Id && header === headerEntry.Id && duty === dutyEntry.Id}
                                onclick={() => openTab(categoryId, expansionEntry.Id, headerEntry.Id, dutyEntry.Id, true)}
                        >
                            {dutyEntry.Name || dutyEntry.Id}
                        </button>
                    {/each}
                {:else}
                    {#each expansionEntry.Headers as headerEntry (headerEntry.Id)}
                        {@render renderHeader(headerEntry, path, categoryId, expansionEntry.Id)}
                    {/each}
                {/if}
            </div>
        </Collapse>
{/snippet}

{#snippet renderHeader(headerEntry: Header, parentPath: string, categoryId: number, expansionId: number)}
    {@const path = getPath(parentPath, headerEntry.Id, 'header')}
    {@const hasMultipleDuties = headerEntry.Duties.length > 1}

    {#if hasMultipleDuties}
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
                <span class="tree-label flex-grow-1">{headerEntry.Name || headerEntry.Id}</span>
            </div>
            <Collapse isOpen={open} animate={!isSearching} style="--node-hue: 216">
                <div class="tree-indent">
                    {#each headerEntry.Duties as dutyEntry (dutyEntry.Id)}
                        <button
                                id="{categoryId}{expansionId}{headerEntry.Id}{dutyEntry.Id}-tab"
                                class="tree-node-element w-100 text-start border-0"
                                class:active={category === categoryId && expansion === expansionId && header === headerEntry.Id && duty === dutyEntry.Id}
                                onclick={() => openTab(categoryId, expansionId, headerEntry.Id, dutyEntry.Id, true)}
                        >
                            {dutyEntry.Name || dutyEntry.Id}
                        </button>
                    {/each}
                </div>
            </Collapse>
    {:else}
        {#each headerEntry.Duties as dutyEntry (dutyEntry.Id)}
            <button
                    id="{categoryId}{expansionId}{headerEntry.Id}{dutyEntry.Id}-tab"
                    class="tree-node-element w-100 text-start border-0"
                    class:active={category === categoryId && expansion === expansionId && header === headerEntry.Id && duty === dutyEntry.Id}
                    onclick={() => openTab(categoryId, expansionId, headerEntry.Id, dutyEntry.Id, true)}
            >
                {dutyEntry.Name || dutyEntry.Id}
            </button>
        {/each}
    {/if}
{/snippet}
