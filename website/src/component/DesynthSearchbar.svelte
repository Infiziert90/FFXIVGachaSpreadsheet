<script lang="ts">
    import { onMount } from 'svelte';
    import { page } from '$app/state';
    import { Button, ButtonGroup, ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import type { DesynthBase } from "$lib/interfaces";
    import { Mappings } from "$lib/mappings";
    import { getIconPath } from '$lib/utils';

    interface Props {
        desynthBase: DesynthBase;
        selectedId: number;
        onButtonClick: (id: number, usedData: Record<number, any>, statsType: string, addQuery: boolean) => void;
        tabElements: { [key: string]: HTMLButtonElement };
    }

    let { desynthBase, selectedId, onButtonClick, tabElements }: Props = $props();

    // Search state
    let searchType = $state<'sources' | 'rewards'>('sources');
    let searchQuery = $state('');

    // Convert Sources and Rewards records to arrays for iteration
    const allSourcesArray = $derived(Object.entries(desynthBase.Sources).map(([id, data]) => ({
        id: parseInt(id),
        history: data
    })).sort((a, b) => Mappings[a.id].Name.localeCompare(Mappings[b.id].Name)));

    const allRewardsArray = $derived(Object.entries(desynthBase.Rewards).map(([id, data]) => ({
        id: parseInt(id),
        history: data
    })).sort((a, b) => Mappings[a.id].Name.localeCompare(Mappings[b.id].Name)));

    // Get the current array based on search type
    const currentArray = $derived(searchType === 'sources' ? allSourcesArray : allRewardsArray);
    const currentData = $derived(searchType === 'sources' ? desynthBase.Sources : desynthBase.Rewards);
    const currentStatsType = $derived(searchType === 'sources' ? 'Desynths' : 'Received');

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
                    score: getMatchScore(Mappings[item.id].Name, searchQuery)
                }))
                .filter(item => item.score > 0)
                .sort((a, b) => {
                    // Sort by score (descending), then alphabetically
                    if (b.score !== a.score) {
                        return b.score - a.score;
                    }
                    return Mappings[a.id].Name.localeCompare(Mappings[b.id].Name);
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
                lastAutoSelectedId = singleItem.id;
                onButtonClick(singleItem.id, currentData, currentStatsType, true);
            }
        } else if (filteredArray.length !== 1) {
            // Reset tracking when there's not exactly one result
            lastAutoSelectedId = null;
        }
    });

    // Load from URL on mount
    onMount(() => {
        if (page.url.searchParams.has('source')) {
            const sourceId = parseInt(page.url.searchParams.get('source')!);
            if (Mappings[sourceId]) {
                searchType = 'sources';
                searchQuery = Mappings[sourceId].Name;
            }
        } else if (page.url.searchParams.has('reward')) {
            const rewardId = parseInt(page.url.searchParams.get('reward')!);
            if (Mappings[rewardId]) {
                searchType = 'rewards';
                searchQuery = Mappings[rewardId].Name;
            }
        }
    });
</script>

<div class="d-flex w-100 flex-column gap-2">
    <div class="input-group">
        <input 
            type="text"
            class="form-control"
            placeholder="Search items..."
            aria-label="Search"
            bind:value={searchQuery}
        >
    </div>
    <ButtonGroup>
        <Button 
            color={searchType === 'sources' ? 'primary' : 'outline-primary'}
            onclick={() => searchType = 'sources'}
        >
            Sources
        </Button>
        <Button 
            color={searchType === 'rewards' ? 'primary' : 'outline-primary'}
            onclick={() => searchType = 'rewards'}
        >
            Rewards
        </Button>
    </ButtonGroup>
    {#if searchQuery.trim() === ''}
        <p class="text-muted">Enter a search query to find {searchType}</p>
    {:else if filteredArray.length === 0}
        <p class="text-muted">No {searchType} found</p>
    {:else}
        <ListGroup>
            {#each filteredArray as item}
                <ListGroupItem 
                    class="list-group-item-xiv-item"
                    active={selectedId === item.id}
                    onclick={() => onButtonClick(item.id, currentData, currentStatsType, true)}
                    style="cursor: pointer;"
                >
                    <img 
                        width="40" 
                        height="40" 
                        loading="lazy" 
                        src={getIconPath(Mappings[item.id].Icon, true)}
                        style="margin-right: 0.5rem; vertical-align: middle;"
                        alt=""
                    />
                    <span class="list-group-item-xiv-item-name">
                        {Mappings[item.id].Name}
                    </span>
                </ListGroupItem>
            {/each}
        </ListGroup>
    {/if}
</div>