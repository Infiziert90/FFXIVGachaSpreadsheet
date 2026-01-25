<script lang="ts">
    import { onMount } from 'svelte';
    import { ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import type {UniqueLocation} from "$lib/interfaces";
    import {SimpleMapSheet, SimpleTerritorySheet} from "$lib/sheets/simplifiedSheets";

    interface Props {
        uniqueLocations: UniqueLocation[];
        selectedId: number;
        onButtonClick: (id: number, usedData: UniqueLocation, addQuery: boolean) => void;
        tabElements: { [key: string]: HTMLButtonElement };
    }

    let { uniqueLocations, selectedId, onButtonClick, tabElements }: Props = $props();

    // Search state
    let searchQuery = $state('');

    // Convert Sources and Rewards records to arrays for iteration
    const allSourcesArray = $derived(uniqueLocations.map((value, index) => ({
        id: index,
        data: value,
        name: getMapName(value)
    })).sort((a, b) => a.name.localeCompare(b.name)));

    // Get the current array based on search type
    const currentArray = $derived(allSourcesArray);
    const currentData = $derived(uniqueLocations);

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
            if (selectedId !== singleItem.id && lastAutoSelectedId !== singleItem.id) {
                searchQuery = '';

                lastAutoSelectedId = singleItem.id;
                onButtonClick(singleItem.id, singleItem.data, true);
            }
        } else if (filteredArray.length !== 1) {
            // Reset tracking when there's not exactly one result
            lastAutoSelectedId = null;
        }
    });

    // Load from URL on mount
    onMount(() => {
    });

    function getMapName(location: UniqueLocation): string {
        let map = SimpleMapSheet[location.Map];
        let territory = SimpleTerritorySheet[location.Territory];

        let subName = map.PlaceNameSub.Name.length > 1 ? ` - ${map.PlaceNameSub.Name}` : '';
        let instancedWarning = territory.ContentFinderCondition > 0 ? ` [Instanced]` : '';

        return `${map.PlaceName.Name}${subName}${instancedWarning} (${location.Map} - ${location.Territory})`
    }
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
        <p class="text-muted">Enter a search query to find a map</p>
    {:else if filteredArray.length === 0}
        <p class="text-muted">No area found</p>
    {:else}
        <ListGroup>
            {#each filteredArray as item}
                <ListGroupItem 
                    class="list-group-item-xiv-item"
                    active={selectedId === item.id}
                    onclick={() => {
                        searchQuery = '';
                        onButtonClick(item.id, item.data, true);
                    }}
                    style="cursor: pointer;"
                >
                    <span class="list-group-item-xiv-map-name">
                        {item.name}
                    </span>
                </ListGroupItem>
            {/each}
        </ListGroup>
    {/if}
</div>