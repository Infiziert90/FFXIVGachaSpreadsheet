<script lang="ts">
    import { Collapse, Icon } from "@sveltestrap/sveltestrap";
    import type { ChestDrop, Expansion, Header } from "$lib/interfaces";
    import { getIconPath } from "$lib/utils";

    interface Props {
        chestDropData: ChestDrop[];
        category: number;
        expansion: number;
        header: number;
        duty: number;
        openTab: (category: number, expansion: number, header: number, duty: number, addQuery: boolean) => void;
    }

    let { chestDropData, category, expansion, header, duty, openTab }: Props = $props();

    /**
     * Converts an icon ID to the XIVAPI asset path format
     * @param iconId - The numeric icon ID from the game
     * @returns Asset path string in format "XXXXXX/XXXXXX"
     */
    function iconIdToPath(iconId: number): string {
        const paddedId = iconId.toString().padStart(6, '0');
        const folder = paddedId.substring(0, 3) + '000';
        return `${folder}/${paddedId}`;
    }

    interface CategoryStyle {
        icon: string;
        hue: number;
    }

    interface ExpansionStyle {
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

    const expansionStyles: Record<string, ExpansionStyle> = {
        // ARR
        'A Realm Reborn': { icon: iconIdToPath(61875), hue: 236 },
        'ARR': { icon: iconIdToPath(61875), hue: 236 },
        '2.x': { icon: iconIdToPath(61875), hue: 236 },
        
        // HW
        'Heavensward': { icon: iconIdToPath(61876), hue: 225 },
        'HW': { icon: iconIdToPath(61876), hue: 225 },
        '3.x': { icon: iconIdToPath(61876), hue: 225 },
        
        // SB
        'Stormblood': { icon: iconIdToPath(61877), hue: 348 },
        'SB': { icon: iconIdToPath(61877), hue: 348 },
        '4.x': { icon: iconIdToPath(61877), hue: 348 },
        
        // ShB
        'Shadowbringers': { icon: iconIdToPath(61878), hue: 260 },
        'ShB': { icon: iconIdToPath(61878), hue: 260 },
        '5.x': { icon: iconIdToPath(61878), hue: 260 },
        
        // EW
        'Endwalker': { icon: iconIdToPath(61879), hue: 51 },
        'EW': { icon: iconIdToPath(61879), hue: 51 },
        'Endw': { icon: iconIdToPath(61879), hue: 51 },
        '6.x': { icon: iconIdToPath(61879), hue: 51 },
        
        // DT
        'Dawntrail': { icon: iconIdToPath(61880), hue: 39 },
        'DT': { icon: iconIdToPath(61880), hue: 39 },
        '7.x': { icon: iconIdToPath(61880), hue: 39 },
    };

    function getCategoryStyle(categoryName: string, categoryId: number): CategoryStyle | null {
        return categoryStyles[categoryName] || categoryStyles[categoryId] || null;
    }

    function getExpansionStyle(expansionName: string): ExpansionStyle | null {
        return expansionStyles[expansionName] || null;
    }

    let openNodes = $state<Set<string>>(new Set());

    /**
     * Generates a unique path identifier for a tree node
     * @param parentPath - Path of the parent node (empty string for root)
     * @param id - The ID of the current node
     * @param level - The type/level of the node in the hierarchy
     * @returns Unique path string for the node
     */
    function getPath(parentPath: string, id: number, level: 'category' | 'expansion' | 'header'): string {
        return parentPath ? `${parentPath}/${level}-${id}` : `${level}-${id}`;
    }

    const isCategory = (path: string): boolean => path.startsWith('category-') && !path.includes('/');

    function closeAllCategories(nodes: Set<string>): Set<string> {
        return new Set(Array.from(nodes).filter(path => !isCategory(path)));
    }

    function toggleNode(path: string) {
        const newOpenNodes = new Set(openNodes);
        
        // If already open, close it
        if (newOpenNodes.delete(path)) {
            openNodes = newOpenNodes;
            return;
        }
        
        // If opening a category, close all other categories first
        if (isCategory(path)) {
            const filtered = closeAllCategories(newOpenNodes);
            filtered.add(path);
            openNodes = filtered;
            return;
        }
        
        // For non-category nodes, just toggle normally
        newOpenNodes.add(path);
        openNodes = newOpenNodes;
    }

    function openNodeExclusive(path: string) {
        const newOpenNodes = new Set(openNodes);
        
        // If opening a category, close all other categories first
        if (isCategory(path)) {
            const filtered = closeAllCategories(newOpenNodes);
            filtered.add(path);
            openNodes = filtered;
            return;
        }
        
        // For non-category nodes, just add them
        newOpenNodes.add(path);
        openNodes = newOpenNodes;
    }

    const isOpen = (path: string): boolean => openNodes.has(path);

    /**
     * Ensures the path to the current selection is open.
     * Closes siblings at each level to maintain single-open-per-level behavior.
     * Automatically opens nodes that have only one child.
     */
    function ensureSelectionPathOpen() {
        const categoryEntry = chestDropData.find(c => c.Id === category);
        if (!categoryEntry) return;

        const categoryPath = getPath('', category, 'category');
        openNodeExclusive(categoryPath);

        const expansionEntry = categoryEntry.Expansions.find(e => e.Id === expansion);
        if (!expansionEntry) return;

        const expansionPath = getPath(categoryPath, expansion, 'expansion');
        openNodeExclusive(expansionPath);

        // Auto-open if only one header exists
        if (expansionEntry.Headers.length === 1) {
            const headerPath = getPath(expansionPath, expansionEntry.Headers[0].Id, 'header');
            openNodeExclusive(headerPath);
        } else {
            const headerPath = getPath(expansionPath, header, 'header');
            openNodeExclusive(headerPath);
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

<div class="loot-tree w-100">
    {#each chestDropData as chestDropEntry}
        {@render renderCategory(chestDropEntry, '')}
    {/each}
</div>

{#snippet renderCategory(chestDropEntry: ChestDrop, parentPath: string)}
    {@const path = getPath(parentPath, chestDropEntry.Id, 'category')}
    {@const open = isOpen(path)}

        {@const categoryStyle = getCategoryStyle(chestDropEntry.Name, chestDropEntry.Id)}
        <div
                class="tree-node-folder d-flex align-items-center gap-2 user-select-none"
                data-open={open ? 'true' : 'false'}
                style={categoryStyle ? `--node-hue: ${categoryStyle.hue}` : ''}
                role="button"
                tabindex="0"
                onclick={() => toggleNode(path)}
                onkeydown={(e) => e.key === 'Enter' && toggleNode(path)}
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

        <Collapse isOpen={open} style={categoryStyle ? `--node-hue: ${categoryStyle.hue}` : ''}>
            <div class="tree-indent">
                {#each chestDropEntry.Expansions as expansionEntry}
                    {@render renderExpansion(expansionEntry, path, chestDropEntry.Id)}
                {/each}
            </div>
        </Collapse>
{/snippet}

{#snippet renderExpansion(expansionEntry: Expansion, parentPath: string, categoryId: number)}
    {@const path = getPath(parentPath, expansionEntry.Id, 'expansion')}
    {@const open = isOpen(path)}
    {@const hasSingleHeader = expansionEntry.Headers.length === 1}

        {@const expansionStyle = getExpansionStyle(expansionEntry.Name)}
        <div
                class="tree-node-folder d-flex align-items-center gap-2 user-select-none"
                data-open={open ? 'true' : 'false'}
                style={expansionStyle ? `--node-hue: ${expansionStyle.hue}` : ''}
                role="button"
                tabindex="0"
                onclick={() => toggleNode(path)}
                onkeydown={(e) => e.key === 'Enter' && toggleNode(path)}
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

        <Collapse isOpen={open} style={expansionStyle ? `--node-hue: ${expansionStyle.hue}` : ''}>
            <div class="tree-indent">
                {#if hasSingleHeader}
                    {@const headerEntry = expansionEntry.Headers[0]}
                    {#each headerEntry.Duties as dutyEntry}
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
                    {#each expansionEntry.Headers as headerEntry}
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
        {@const open = isOpen(path)}
            <div
                class="tree-node-folder d-flex align-items-center gap-2 user-select-none"
                data-open={open ? 'true' : 'false'}
                role="button"
                tabindex="0"
                onclick={() => toggleNode(path)}
                onkeydown={(e) => e.key === 'Enter' && toggleNode(path)}
                style="--node-hue: 216"
            >
                <span class="tree-icon d-inline-flex align-items-center">
                    <Icon name={open ? 'chevron-down' : 'chevron-right'} />
                </span>
                <span class="tree-label flex-grow-1">{headerEntry.Name || headerEntry.Id}</span>
            </div>
            <Collapse isOpen={open} style="--node-hue: 216">
                <div class="tree-indent">
                    {#each headerEntry.Duties as dutyEntry}
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
        {#each headerEntry.Duties as dutyEntry}
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
