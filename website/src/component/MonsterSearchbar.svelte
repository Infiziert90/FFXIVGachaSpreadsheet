<script lang="ts">
    import { onMount } from 'svelte';
    import { ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import {BNpcNameSheet} from "$lib/sheets";

    interface Props {
        names: Record<number, number[]>;
        selectedMonsterId: number;
        onMonsterButtonClick: (id: number, usedData: number[], addQuery: boolean) => void;
        tabMonsterElements: { [key: string]: HTMLButtonElement };
    }

    let { names, selectedMonsterId, onMonsterButtonClick, tabMonsterElements }: Props = $props();

    $inspect(names);
    // Search state
    let searchQuery = $state('');

    // Convert Sources and Rewards records to arrays for iteration
    const allSourcesArray = $derived(Object.entries(names).map(([id, value]) => ({
        id: parseInt(id),
        data: value,
        name: BNpcNameSheet[parseInt(id)] ?? 'Unknown'
    })).sort((a, b) => a.name.localeCompare(b.name)));

    // Get the current array based on search type
    const currentArray = $derived(allSourcesArray);
    const currentData = $derived(names);

    // Intelligent filtering function
    function getMatchScore(itemName: string, query: string): number {
        const lowerName = itemName.toLowerCase();
        const lowerQuery = query.toLowerCase();
        
        // Exact match gets highest score
        if (lowerName === lowerQuery) return 1000;
        
        // Starts with query gets high score
        if (lowerName.startsWith(lowerQuery)) return 500;
        
        // Contains query gets medium score
        if (lowerName.includes(lowerQuery)) return 100;
        
        // Fuzzy match: check if all query characters appear in order
        let queryIndex = 0;
        for (let i = 0; i < lowerName.length && queryIndex < lowerQuery.length; i++) {
            if (lowerName[i] === lowerQuery[queryIndex]) {
                queryIndex++;
            }
        }
        if (queryIndex === lowerQuery.length) return 50;
        
        return 0;
    }

    // Filter array with intelligent matching and sorting, limit to first 25 results
    const filteredArray = $derived(
        searchQuery.trim() === '' 
            ? [] 
            : currentArray
                .map(item => ({
                    ...item,
                    score: getMatchScore(item.name, searchQuery)
                }))
                .filter(item => item.score > 0)
                .sort((a, b) => {
                    // Sort by score (descending), then alphabetically
                    if (b.score !== a.score) {
                        return b.score - a.score;
                    }
                    return a.name.localeCompare(b.name);
                })
                .slice(0, 25)
    );

    // Auto-select if there's only one item in search results
    let lastAutoSelectedId = $state<number | null>(null);
    $effect(() => {
        if (filteredArray.length === 1 && searchQuery.trim() !== '') {
            const singleItem = filteredArray[0];
            // Only auto-select if it's not already selected and we haven't auto-selected it already
            if (selectedMonsterId !== singleItem.id && lastAutoSelectedId !== singleItem.id) {
                lastAutoSelectedId = singleItem.id;
                onMonsterButtonClick(singleItem.id, singleItem.data, true);
                searchQuery = '';
            }
        } else if (filteredArray.length !== 1) {
            // Reset tracking when there's not exactly one result
            lastAutoSelectedId = null;
        }
    });

    // Load from URL on mount
    onMount(() => {
    });
</script>

<div class="d-block w-100 gap-2">
    <div class="input-group">
        <input 
            type="text"
            class="form-control"
            placeholder="Search items..."
            aria-label="Search"
            bind:value={searchQuery}
        >
    </div>
    {#if searchQuery.trim() === ''}
        <p class="text-muted">Enter a search query to find any monster</p>
    {:else if filteredArray.length === 0}
        <p class="text-muted">No monster found</p>
    {:else}
        <ListGroup>
            {#each filteredArray as item}
                <ListGroupItem 
                    class="list-group-item-xiv-item"
                    active={selectedMonsterId === item.id}
                    onclick={() => {
                        onMonsterButtonClick(item.id, item.data, true)
                        searchQuery = '';
                    }}
                    style="cursor: pointer;"
                >
                    <span class="list-group-item-xiv-item-name">
                        {item.name}
                    </span>
                </ListGroupItem>
            {/each}
        </ListGroup>
    {/if}
</div>