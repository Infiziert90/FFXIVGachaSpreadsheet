<script lang="ts">
    import { Collapse, Icon } from "@sveltestrap/sveltestrap";
    import type { ChestDrop } from "$lib/interfaces";

    interface Props {
        chestDropData: ChestDrop[];
        category: number;
        expansion: number;
        header: number;
        duty: number;
        openTab: (category: number, expansion: number, header: number, duty: number, addQuery: boolean) => void;
    }

    let { chestDropData, category, expansion, header, duty, openTab }: Props = $props();
    
    // Icon mappings for categories (by name or ID)
    // Values can be direct icon IDs (numbers) or asset paths (strings)
    const categoryIcons: Record<string | number, string | number> = {
        // Main content types (using provided icon IDs)
        'Dungeons': 60831,
        'Dungeon': 60831,
        'Raids': 60832,
        'Raid': 60832,
        'Trials': 60833, // Assuming 60833 since 60832 was listed twice
        'Trial': 60833,
        'Treasure Hunt': 60838,
        'Treasure Hunts': 60838,
        'Chaotic Alliance Raid': 60855,
        'Chaotic Alliance Raids': 60855,
        'Open World': 60857,
        'Alliance Raids': '061000/061804_hr1',
        'Alliance Raid': '061000/061804_hr1',
        'Deep Dungeons': '061000/061805_hr1',
        'Deep Dungeon': '061000/061805_hr1',
        'Ultimate Raids': '061000/061806_hr1',
        'Ultimate': '061000/061806_hr1',
        'Extreme Trials': '061000/061807_hr1',
        'Extreme': '061000/061807_hr1',
        'Savage Raids': '061000/061808_hr1',
        'Savage': '061000/061808_hr1',
    };
    
    // Icon mappings for expansions (by name)
    // Values can be direct icon IDs (numbers) or asset paths (strings)
    const expansionIcons: Record<string, string | number> = {
        // Main expansions (using provided icon IDs)
        'A Realm Reborn': 61875,
        'ARR': 61875,
        'Heavensward': 61876,
        'HW': 61876,
        'Stormblood': 61877,
        'SB': 61877,
        'Shadowbringers': 61878,
        'ShB': 61878,
        'Endwalker': 61879,
        'EW': 61879,
        'Endw': 61879,
        'Dawntrail': 61880,
        'DT': 61880,
        // Patch series
        '2.x': 61875,
        '3.x': 61876,
        '4.x': 61877,
        '5.x': 61878,
        '6.x': 61879,
        '7.x': 61880,
    };
    
    // Helper function to convert icon ID to asset path format
    // Icon IDs are converted: 60831 -> 060831 -> 060000/060831_hr1
    // Pattern: pad to 6 digits, use first 3 digits + "000" as folder, full 6 digits + _hr1 as file
    // Example: 61801 -> 061801 -> 061000/061801_hr1
    function iconIdToPath(iconId: number): string {
        const paddedId = iconId.toString().padStart(6, '0');
        const folder = paddedId.substring(0, 3) + '000'; // e.g., 060000 or 061000
        const file = paddedId + '_hr1'; // e.g., 060831_hr1 or 061801_hr1
        return `${folder}/${file}`;
    }
    
    // Helper function to get icon URL
    // Supports both direct icon IDs (numbers) and asset paths (strings)
    function getIconUrl(iconPathOrId: string | number): string {
        if (typeof iconPathOrId === 'number') {
            // Direct icon ID - convert to asset path format
            const assetPath = iconIdToPath(iconPathOrId);
            return `https://v2.xivapi.com/api/asset?path=ui/icon/${assetPath}.tex&format=png`;
        } else {
            // Asset path - use asset API
            return `https://v2.xivapi.com/api/asset?path=ui/icon/${iconPathOrId}.tex&format=png`;
        }
    }
    
    // Get category icon
    function getCategoryIcon(categoryName: string, categoryId: number): string | number | null {
        return categoryIcons[categoryName] || categoryIcons[categoryId] || null;
    }
    
    // Get expansion icon
    function getExpansionIcon(expansionName: string): string | number | null {
        return expansionIcons[expansionName] || null;
    }
    
    // Track open state for each node using paths
    let openNodes = $state<Set<string>>(new Set());
    
    // Helper function to generate path for tree nodes
    function getPath(parentPath: string, id: number, level: 'category' | 'expansion' | 'header'): string {
        return parentPath ? `${parentPath}/${level}-${id}` : `${level}-${id}`;
    }
    
    function isOpen(path: string): boolean {
        return openNodes.has(path);
    }
    
    function toggleNode(path: string) {
        if (openNodes.has(path)) {
            openNodes.delete(path);
        } else {
            openNodes.add(path);
        }
        openNodes = new Set(openNodes); // Trigger reactivity
    }
    
    // Initialize open nodes based on current selection
    // Also auto-open paths that have only one child (no need to collapse)
    function initializeOpenNodes() {
        const paths = new Set<string>();
        
        // Find current category
        const categoryEntry = chestDropData.find(c => c.Id === category);
        if (categoryEntry) {
            const categoryPath = getPath('', category, 'category');
            paths.add(categoryPath);
            
            // Auto-open if only one expansion
            if (categoryEntry.Expansions.length === 1) {
                const expansionEntry = categoryEntry.Expansions[0];
                const expansionPath = getPath(categoryPath, expansionEntry.Id, 'expansion');
                paths.add(expansionPath);
                
                // Auto-open if only one header
                if (expansionEntry.Headers.length === 1) {
                    const headerEntry = expansionEntry.Headers[0];
                    const headerPath = getPath(expansionPath, headerEntry.Id, 'header');
                    paths.add(headerPath);
                } else {
                    // Use current selection
                    const expansionPath2 = getPath(categoryPath, expansion, 'expansion');
                    paths.add(expansionPath2);
                    const headerPath = getPath(expansionPath2, header, 'header');
                    paths.add(headerPath);
                }
            } else {
                // Use current selection
                const expansionPath = getPath(categoryPath, expansion, 'expansion');
                paths.add(expansionPath);
                
                const expansionEntry = categoryEntry.Expansions.find(e => e.Id === expansion);
                if (expansionEntry && expansionEntry.Headers.length === 1) {
                    // Auto-open if only one header
                    const headerPath = getPath(expansionPath, expansionEntry.Headers[0].Id, 'header');
                    paths.add(headerPath);
                } else {
                    const headerPath = getPath(expansionPath, header, 'header');
                    paths.add(headerPath);
                }
            }
        }
        
        openNodes = paths;
    }
    
    // Update open nodes when props change
    $effect(() => {
        initializeOpenNodes();
    });
</script>

<style>
    .loot-tree {
        width: 100%;
    }
    
    .tree-node {
        margin-left: 0;
    }
    
    .tree-node-folder {
        cursor: pointer;
        padding: 8px 12px;
        user-select: none;
        display: flex;
        align-items: center;
        gap: 8px;
        border-radius: 4px;
        transition: background-color 0.2s;
    }
    
    .tree-node-folder:hover {
        background-color: rgba(255, 255, 255, 0.1);
    }
    
    .tree-node-file {
        padding: 6px 12px 6px 16px;
        width: 100%;
        text-align: left;
        border: none;
        background: transparent;
        cursor: pointer;
        transition: background-color 0.2s;
    }
    
    .tree-node-file:hover {
        background-color: rgba(255, 255, 255, 0.1);
    }
    
    .tree-node-file.active {
        background-color: rgba(13, 110, 253, 0.25);
        font-weight: 500;
    }
    
    .tree-indent {
        margin-left: 12px;
    }
    
    .tree-icon {
        display: inline-flex;
        align-items: center;
        width: 16px;
    }
    
    .tree-label {
        flex: 1;
        display: flex;
        align-items: center;
        gap: 8px;
    }
    
    .category-icon,
    .expansion-icon {
        width: 20px;
        height: 20px;
        object-fit: contain;
        flex-shrink: 0;
    }
</style>

<div class="loot-tree">
    {#each chestDropData as chestDropEntry}
        {@render renderCategory(chestDropEntry, '')}
    {/each}
</div>

{#snippet renderCategory(chestDropEntry: ChestDrop, parentPath: string)}
    {@const path = getPath(parentPath, chestDropEntry.Id, 'category')}
    {@const open = isOpen(path)}
    
    <div class="tree-node">
        <div 
            class="tree-node-folder" 
            role="button"
            tabindex="0"
            onclick={() => toggleNode(path)}
            onkeydown={(e) => e.key === 'Enter' && toggleNode(path)}
        >
            <span class="tree-icon">
                <Icon name={open ? 'chevron-down' : 'chevron-right'} />
            </span>
            <span class="tree-label">
                {#if getCategoryIcon(chestDropEntry.Name, chestDropEntry.Id)}
                    <img 
                        src={getIconUrl(getCategoryIcon(chestDropEntry.Name, chestDropEntry.Id)!)}
                        alt=""
                        class="category-icon"
                    />
                {/if}
                {chestDropEntry.Name}
            </span>
        </div>
        <Collapse isOpen={open}>
            <div class="tree-indent">
                {#each chestDropEntry.Expansions as expansionEntry}
                    {@render renderExpansion(expansionEntry, path, chestDropEntry.Id)}
                {/each}
            </div>
        </Collapse>
    </div>
{/snippet}

{#snippet renderExpansion(expansionEntry: any, parentPath: string, categoryId: number)}
    {@const path = getPath(parentPath, expansionEntry.Id, 'expansion')}
    {@const open = isOpen(path)}
    {@const hasSingleHeader = expansionEntry.Headers.length === 1}
    
    <div class="tree-node">
        <div 
            class="tree-node-folder" 
            role="button"
            tabindex="0"
            onclick={() => toggleNode(path)}
            onkeydown={(e) => e.key === 'Enter' && toggleNode(path)}
        >
            <span class="tree-icon">
                <Icon name={open ? 'chevron-down' : 'chevron-right'} />
            </span>
            <span class="tree-label">
                {#if getExpansionIcon(expansionEntry.Name)}
                    <img 
                        src={getIconUrl(getExpansionIcon(expansionEntry.Name)!)}
                        alt=""
                        class="expansion-icon"
                    />
                {/if}
                {expansionEntry.Name}
            </span>
        </div>
        <Collapse isOpen={open}>
            <div class="tree-indent">
                {#if hasSingleHeader}
                    {@const headerEntry = expansionEntry.Headers[0]}
                    {#each headerEntry.Duties as dutyEntry}
                        <button
                            id="{categoryId}{expansionEntry.Id}{headerEntry.Id}{dutyEntry.Id}-tab"
                            class="tree-node-file"
                            class:active={category === categoryId && expansion === expansionEntry.Id && header === headerEntry.Id && duty === dutyEntry.Id}
                            onclick={() => openTab(categoryId, expansionEntry.Id, headerEntry.Id, dutyEntry.Id, true)}
                        >
                            {dutyEntry.Name}
                        </button>
                    {/each}
                {:else}
                    {#each expansionEntry.Headers as headerEntry}
                        {@render renderHeader(headerEntry, path, categoryId, expansionEntry.Id)}
                    {/each}
                {/if}
            </div>
        </Collapse>
    </div>
{/snippet}

{#snippet renderHeader(headerEntry: any, parentPath: string, categoryId: number, expansionId: number)}
    {@const path = getPath(parentPath, headerEntry.Id, 'header')}
    {@const open = isOpen(path)}
    {@const hasMultipleDuties = headerEntry.Duties.length > 1}
    
    {#if hasMultipleDuties}
        <div class="tree-node">
            <div 
                class="tree-node-folder" 
                role="button"
                tabindex="0"
                onclick={() => toggleNode(path)}
                onkeydown={(e) => e.key === 'Enter' && toggleNode(path)}
            >
                <span class="tree-icon">
                    <Icon name={open ? 'chevron-down' : 'chevron-right'} />
                </span>
                <span class="tree-label">{headerEntry.Name}</span>
            </div>
            <Collapse isOpen={open}>
                <div class="tree-indent">
                    {#each headerEntry.Duties as dutyEntry}
                        <button
                            id="{categoryId}{expansionId}{headerEntry.Id}{dutyEntry.Id}-tab"
                            class="tree-node-file"
                            class:active={category === categoryId && expansion === expansionId && header === headerEntry.Id && duty === dutyEntry.Id}
                            onclick={() => openTab(categoryId, expansionId, headerEntry.Id, dutyEntry.Id, true)}
                        >
                            {dutyEntry.Name}
                        </button>
                    {/each}
                </div>
            </Collapse>
        </div>
    {:else}
        {#each headerEntry.Duties as dutyEntry}
            <button
                id="{categoryId}{expansionId}{headerEntry.Id}{dutyEntry.Id}-tab"
                class="tree-node-file"
                class:active={category === categoryId && expansion === expansionId && header === headerEntry.Id && duty === dutyEntry.Id}
                onclick={() => openTab(categoryId, expansionId, headerEntry.Id, dutyEntry.Id, true)}
            >
                {dutyEntry.Name}
            </button>
        {/each}
    {/if}
{/snippet}
