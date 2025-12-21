<script lang="ts">
    import { Table, Icon } from '@sveltestrap/sveltestrap';
    import type { Reward } from "$lib/interfaces";
    import type { ColumnTemplate } from "$lib/table";
    import {Mappings} from "$lib/mappings";

    /**
     * Component props interface
     * @param items - Array of reward items to display in the table
     * @param columns - Array of column definitions specifying how to render each column
     */
    interface Props {
        items: Reward[];
        columns: ColumnTemplate[];
    }

    // Destructure props using Svelte 5 runes syntax
    let { items, columns }: Props = $props();

    // Reactive state for tracking current sort field and direction
    let sortField = $state<string>('');
    let sortDirection = $state<'asc' | 'desc'>('asc');
    let sortWithMapping = $state(false);

    /**
     * Initialize default sort when columns change
     * This effect runs whenever the columns prop changes and sets up the default sort
     * if one is specified in the column configuration
     */
    $effect(() => {
        if (columns.length === 0) return;
        
        const defaultSortColumn = columns.find(col => col.defaultSort);
        if (defaultSortColumn && defaultSortColumn.field && !sortField) {
            sortField = defaultSortColumn.field;
            sortDirection = defaultSortColumn.defaultSort === 'asc' ? 'asc' : 'desc';
        }
    });

    /**
     * Handle sort column click/keyboard interaction
     * Toggles sort direction if clicking the same column, otherwise sets new sort field
     * @param column - The column template that was clicked
     */
    function handleSort(column: ColumnTemplate) {
        if (!column.field || column.sortable === false) return;

        sortWithMapping = !!column.mappingSort;
        if (sortField === column.field) {
            // Toggle direction if clicking the same column
            sortDirection = sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            // Set new sort field and default to ascending
            sortField = column.field;
            sortDirection = 'asc';
        }
    }

    /**
     * Get sorted array of items based on current sort state
     * Returns a new sorted array without mutating the original items array
     * @returns Sorted array of Reward items
     */
    function getSortedItems(): Reward[] {
        if (!sortField) return items;

        return [...items].sort((a, b) => {
            // Type assertion: sortField is validated to be a key of Reward
            let aVal = a[sortField as keyof Reward];
            let bVal = b[sortField as keyof Reward];

            if (sortWithMapping) {
                aVal = Mappings[a.Id].Name;
                bVal = Mappings[b.Id].Name;
            }

            // Use == (loose equality) to handle null/undefined comparisons
            if (aVal == bVal) return 0;

            // Compare values and apply sort direction
            const comparison = aVal < bVal ? -1 : 1;
            return sortDirection === 'asc' ? comparison : -comparison;
        });
    }

    /**
     * Get CSS classes for table cell (<td>) elements
     * Applies any custom class extensions specified in the column template
     * @param column - The column template
     * @returns Space-separated string of CSS classes
     */
    function getColumnClasses(column: ColumnTemplate): string {
        const classes: string[] = [];
        if (column.classExtension) {
            classes.push(...column.classExtension);
        }
        return classes.join(' ');
    }

    /**
     * Get CSS classes for table header (<th>) elements
     * Includes sortable class and sort direction indicators
     * @param column - The column template
     * @returns Space-separated string of CSS classes
     */
    function getHeaderClasses(column: ColumnTemplate): string {
        const classes: string[] = [];
        if (column.field && column.sortable !== false) {
            classes.push('sortable');
            // Add sort direction class if this column is currently sorted
            if (sortField === column.field) {
                classes.push(sortDirection === 'asc' ? 'sorted-asc' : 'sorted-desc');
            }
        }
        if (column.classExtension) {
            classes.push(...column.classExtension);
        }
        return classes.join(' ');
    }

    /**
     * Check if a column is sortable
     * A column is sortable if it has a field and sortable is not explicitly false
     * @param column - The column template to check
     * @returns True if the column can be sorted
     */
    function isSortable(column: ColumnTemplate): boolean {
        return !!column.field && column.sortable !== false;
    }

    interface CellContent {
        name?: string;
        wikiName?: string;

        itemId?: number;

        content?: string;

        type: number;
    }

    /**
     * Render cell content based on column configuration
     * Priority: templateRenderer > valueRenderer > field value
     * @param row - The reward item for this row
     * @param column - The column template defining how to render
     * @returns HTML string or plain text string to display in the cell
     */
    function renderCellContent(row: Reward, column: ColumnTemplate): CellContent {
        if (column.templateRenderer) {
            // Custom HTML template renderer (returns HTML string)

            if (column.header === 'Name') {
                const name = Mappings[row.Id].Name;
                const wikiName = name.replace(/\s+/g, '_');

                return { name, wikiName, type: 0 };
            }

            return { itemId: row.Id, type: 1 };
        } else if (column.valueRenderer) {
            // Custom value renderer (returns plain text)
            return { content: column.valueRenderer(row), type: 2 }
        } else if (column.field) {
            // Direct field access, convert to string
            // Type assertion: column.field is validated to be a key of Reward
            return { content: String(row[column.field as keyof Reward] ?? ''), type: 2 }
        }

        return { content: '', type: 2 };
    }
</script>

<Table striped size="sm" hover borderless class="align-middle">    
    <thead>
        <tr>
            {#each columns as column}
                <th
                    class={getHeaderClasses(column)}
                    class:sortable={isSortable(column)}
                    onclick={() => handleSort(column)}
                    role={isSortable(column) ? 'button' : undefined}
                    tabindex={isSortable(column) ? 0 : undefined}
                    onkeydown={(e) => {
                        if ((e.key === 'Enter' || e.key === ' ') && isSortable(column)) {
                            e.preventDefault();
                            handleSort(column);
                        }
                    }}
                >
                    {column.header}

                    <Icon name={sortField === column.field ? (sortDirection === 'asc' ? 'arrow-up' : 'arrow-down') : ''}/>
                </th>
            {/each}
        </tr>
    </thead>
    <tbody>
        <!-- 
            Render table rows from sorted items
            Each row displays data for one Reward item
        -->
        {#snippet templateRender(cellContent: CellContent)}
            {#if cellContent.type === 0}
                <a href="https://ffxiv.consolegameswiki.com/wiki/{cellContent.wikiName}" class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">{cellContent.name}</a>
            {:else if cellContent.type === 1}
                <img width="40" height="40" loading="lazy" src="https://v2.xivapi.com/api/asset?path=ui/icon/{Mappings[cellContent.itemId].Icon}_hr1.tex&format=png" alt="{Mappings[cellContent.itemId].Name} Icon">
            {:else if cellContent.type === 2}
                {cellContent.content}
            {/if}
        {/snippet}

        {#each getSortedItems() as row}
            <tr>
                {#each columns as column}
                    <!-- 
                        Table cell with custom rendering
                        Uses {@html} to render HTML content from templateRenderer
                        Falls back to plain text for valueRenderer or field values
                    -->
                    <td class={getColumnClasses(column)}>
                        {@render templateRender(renderCellContent(row, column))}
                    </td>
                {/each}
            </tr>
        {/each}
    </tbody>
</Table>

