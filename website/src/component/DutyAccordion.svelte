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

    const categoryIcons: Record<string | number, string> = {
        'Dungeons': iconIdToPath(60831),
        'Dungeon': iconIdToPath(60831),
        'Raids': iconIdToPath(60832),
        'Raid': iconIdToPath(60832),
        'Trials': iconIdToPath(60833),
        'Trial': iconIdToPath(60833),
        'Treasure Hunts': iconIdToPath(60838),
        'Treasure Hunt': iconIdToPath(60838),
        'Chaotic Alliance Raids': iconIdToPath(60855),
        'Chaotic Alliance Raid': iconIdToPath(60855),
        'Open World': iconIdToPath(60857),
        'Alliance Raids': iconIdToPath(61804),
        'Alliance Raid': iconIdToPath(61804),
        'Deep Dungeons': iconIdToPath(61805),
        'Deep Dungeon': iconIdToPath(61805),
        'Ultimate Raids': iconIdToPath(61806),
        'Ultimate': iconIdToPath(61806),
        'Extreme Trials': iconIdToPath(61807),
        'Extreme': iconIdToPath(61807),
        'Savage Raids': iconIdToPath(61808),
        'Savage': iconIdToPath(61808),
    };

    const expansionIcons: Record<string, string> = {
        'A Realm Reborn': iconIdToPath(61875),
        'ARR': iconIdToPath(61875),
        'Heavensward': iconIdToPath(61876),
        'HW': iconIdToPath(61876),
        'Stormblood': iconIdToPath(61877),
        'SB': iconIdToPath(61877),
        'Shadowbringers': iconIdToPath(61878),
        'ShB': iconIdToPath(61878),
        'Endwalker': iconIdToPath(61879),
        'EW': iconIdToPath(61879),
        'Endw': iconIdToPath(61879),
        'Dawntrail': iconIdToPath(61880),
        'DT': iconIdToPath(61880),
        '2.x': iconIdToPath(61875),
        '3.x': iconIdToPath(61876),
        '4.x': iconIdToPath(61877),
        '5.x': iconIdToPath(61878),
        '6.x': iconIdToPath(61879),
        '7.x': iconIdToPath(61880),
    };

    function getIconUrl(iconPath: string): string {
        return `https://v2.xivapi.com/api/asset?path=ui/icon/${iconPath}_hr1.tex&format=png`;
    }

    function getCategoryIcon(categoryName: string, categoryId: number): string | null {
        return categoryIcons[categoryName] || categoryIcons[categoryId] || null;
    }

    function getExpansionIcon(expansionName: string): string | null {
        return expansionIcons[expansionName] || null;
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

    function isOpen(path: string): boolean {
        return openNodes.has(path);
    }

    function toggleNode(path: string) {
        if (openNodes.has(path)) {
            openNodes.delete(path);
        } else {
            openNodes.add(path);
        }
        // Create new Set instance to trigger reactivity
        openNodes = new Set(openNodes);
    }

    /**
     * Ensures the path to the current selection is open
     * Preserves existing open nodes and adds required paths for the current selection
     * Automatically opens nodes that have only one child
     */
    function ensureSelectionPathOpen() {
        const categoryEntry = chestDropData.find(c => c.Id === category);
        if (!categoryEntry) return;

        const categoryPath = getPath('', category, 'category');
        openNodes.add(categoryPath);

        if (categoryEntry.Expansions.length === 1) {
            const expansionEntry = categoryEntry.Expansions[0];
            const expansionPath = getPath(categoryPath, expansionEntry.Id, 'expansion');
            openNodes.add(expansionPath);

            if (expansionEntry.Headers.length === 1) {
                const headerEntry = expansionEntry.Headers[0];
                const headerPath = getPath(expansionPath, headerEntry.Id, 'header');
                openNodes.add(headerPath);
            } else {
                const selectedExpansionPath = getPath(categoryPath, expansion, 'expansion');
                openNodes.add(selectedExpansionPath);
                const headerPath = getPath(selectedExpansionPath, header, 'header');
                openNodes.add(headerPath);
            }
        } else {
            const expansionPath = getPath(categoryPath, expansion, 'expansion');
            openNodes.add(expansionPath);

            const expansionEntry = categoryEntry.Expansions.find(e => e.Id === expansion);
            if (expansionEntry && expansionEntry.Headers.length === 1) {
                const headerPath = getPath(expansionPath, expansionEntry.Headers[0].Id, 'header');
                openNodes.add(headerPath);
            } else {
                const headerPath = getPath(expansionPath, header, 'header');
                openNodes.add(headerPath);
            }
        }

        // Create new Set instance to trigger reactivity
        openNodes = new Set(openNodes);
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

    <div class="tree-node">
        <div
                class="tree-node-folder d-flex align-items-center gap-2 rounded user-select-none"
                role="button"
                tabindex="0"
                onclick={() => toggleNode(path)}
                onkeydown={(e) => e.key === 'Enter' && toggleNode(path)}
        >
            <span class="tree-icon d-inline-flex align-items-center">
                <Icon name={open ? 'chevron-down' : 'chevron-right'} />
            </span>
            <span class="tree-label flex-grow-1 d-flex align-items-center gap-2">
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
                class="tree-node-folder d-flex align-items-center gap-2 rounded user-select-none"
                role="button"
                tabindex="0"
                onclick={() => toggleNode(path)}
                onkeydown={(e) => e.key === 'Enter' && toggleNode(path)}
        >
            <span class="tree-icon d-inline-flex align-items-center">
                <Icon name={open ? 'chevron-down' : 'chevron-right'} />
            </span>
            <span class="tree-label flex-grow-1 d-flex align-items-center gap-2">
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
                                class="tree-node-element w-100 text-start border-0"
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
                    class="tree-node-folder d-flex align-items-center gap-2 rounded user-select-none"
                    role="button"
                    tabindex="0"
                    onclick={() => toggleNode(path)}
                    onkeydown={(e) => e.key === 'Enter' && toggleNode(path)}
            >
                <span class="tree-icon d-inline-flex align-items-center">
                    <Icon name={open ? 'chevron-down' : 'chevron-right'} />
                </span>
                <span class="tree-label flex-grow-1">{headerEntry.Name}</span>
            </div>
            <Collapse isOpen={open}>
                <div class="tree-indent">
                    {#each headerEntry.Duties as dutyEntry}
                        <button
                                id="{categoryId}{expansionId}{headerEntry.Id}{dutyEntry.Id}-tab"
                                class="tree-node-element w-100 text-start border-0"
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
                    class="tree-node-element w-100 text-start border-0"
                    class:active={category === categoryId && expansion === expansionId && header === headerEntry.Id && duty === dutyEntry.Id}
                    onclick={() => openTab(categoryId, expansionId, headerEntry.Id, dutyEntry.Id, true)}
            >
                {dutyEntry.Name}
            </button>
        {/each}
    {/if}
{/snippet}
