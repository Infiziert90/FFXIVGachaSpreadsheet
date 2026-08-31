<script lang="ts">
    import Collapse from "./Collapse.svelte";
    import { Icon } from "@sveltestrap/sveltestrap";
    import { getIconPath } from "$lib/utils";
    import type {FateReward} from "$lib/structs/fate";
    import {SimpleExVersion, SimpleTerritorySheet} from "$lib/sheets/simplifiedSheets";

    interface Props {
        fateData: FateReward;
        expansion: number;
        territory: number;
        fateType: number;
        openTab: (expansionId: number, territoryId: number, fateTypeId: number, addQuery: boolean) => void;
    }

    let { fateData, expansion, territory, fateType, openTab }: Props = $props();

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
    interface ExpansionStyle {
        icon: string;
        hue: number;
    }

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
    function getPath(parentPath: string, id: number, level: 'expansion' | 'territory' | 'fateType'): string {
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
        const expansionEntry = fateData.Expansions.find(e => e.Id === expansion);
        if (!expansionEntry) return;

        const expansionPath = getPath('', expansion, 'expansion');
        openNodeExclusive(expansionPath);

        const territoryEntry = expansionEntry.Territories.find(e => e.Id === territory);
        if (!territoryEntry) return;

        const territoryPath = getPath(expansionPath, territory, 'territory');
        openNodeExclusive(territoryPath);

        // Auto-open if only one header exists
        if (territoryEntry.FateTypes.length === 1) {
            const headerPath = getPath(territoryPath, territoryEntry.FateTypes[0].Id, 'fateType');
            openNodeExclusive(headerPath);
        } else {
            const headerPath = getPath(territoryPath, fateType, 'fateType');
            openNodeExclusive(headerPath);
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

<div class="loot-tree w-100">
    {#each fateData.Expansions as expansionEntry}
        {@render renderExpansion(expansionEntry, '')}
    {/each}
</div>

{#snippet renderExpansion(expansionEntry: FateExpansion, parentPath: string)}
    {@const path = getPath(parentPath, expansionEntry.Id, 'expansion')}
    {@const open = isOpen(path)}

        {@const expansionStyle = getExpansionStyle(SimpleExVersion[expansionEntry.Id].Name)}
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
                {SimpleExVersion[expansionEntry.Id].Name || expansionEntry.Id}
            </span>
        </div>

        <Collapse isOpen={open} style={expansionStyle ? `--node-hue: ${expansionStyle.hue}` : ''}>
            <div class="tree-indent">
                {#each expansionEntry.Territories as territoryEntry}
                    {@render renderTerritory(territoryEntry, path, expansionEntry.Id)}
                {/each}
            </div>
        </Collapse>
{/snippet}

{#snippet renderTerritory(territoryEntry: FateTerritory, parentPath: string, expansionId: number)}
    {@const path = getPath(parentPath, territoryEntry.Id, 'territory')}

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
            <span class="tree-label flex-grow-1">{SimpleTerritorySheet[territoryEntry.Id].PlaceName.Name || territoryEntry.Id}</span>
        </div>
        <Collapse isOpen={open} style="--node-hue: 216">
            <div class="tree-indent">
                {#each territoryEntry.FateTypes as fateTypeEntry}
                    <button
                            id="{expansionId}{territoryEntry.Id}{fateTypeEntry.Id}-tab"
                            class="tree-node-element w-100 text-start border-0"
                            class:active={expansion === expansionId && territory === territoryEntry.Id && fateType === fateTypeEntry.Id}
                            onclick={() => openTab(expansionId, territoryEntry.Id, fateTypeEntry.Id, true)}
                    >
                        {fateTypeEntry.Id === 0 ? 'Fates' : 'Critical Engagements' || fateTypeEntry.Id}
                    </button>
                {/each}
            </div>
        </Collapse>
{/snippet}
