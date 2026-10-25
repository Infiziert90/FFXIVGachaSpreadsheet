<script lang="ts">
    import {onMount, tick} from 'svelte';
    import { ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import {tryGetBnpcSearchParams} from "$lib/searchParamHelper";
    import {page} from "$app/state";
    import {localizedBNpcName} from "$lib/localization";
    import {currentLanguage} from "$lib/stores/language";
    import {getMatchScore} from "$lib/utils";

    interface Props {
        monsters: { id: number, zones: number }[];
        selectedId: number;
        onButtonClick: (id: number, addQuery: boolean) => Promise<void>;
    }

    let { monsters, selectedId, onButtonClick }: Props = $props();

    // Search state
    let searchQuery = $state('');

    // Filter array with intelligent matching and sorting, limit to first 25 results
    const filteredArray = $derived(
        searchQuery.trim() === ''
            ? []
            : monsters
                .map(monster => ({
                    ...monster,
                    name: localizedBNpcName(monster.id, $currentLanguage)
                }))
                .map(monster => ({
                    ...monster,
                    score: getMatchScore(monster.name, searchQuery)
                }))
                .filter(monster => monster.score > 0)
                .sort((a, b) => {
                    // Sort by score (descending), then alphabetically
                    if (b.score !== a.score) {
                        return b.score - a.score;
                    }
                    return a.name.localeCompare(b.name);
                })
                .slice(0, 25)
    );

    // Auto-select if there's only one monster in search results
    let preventClicksUntilAfterMount = true;
    let lastAutoSelectedId = $state<number | null>(null);
    $effect(() => {
        if (filteredArray.length === 1 && searchQuery.trim() !== '') {
            const singleMonster = filteredArray[0];
            // Only auto-select if it's not already selected and we haven't auto-selected it already
            if (selectedId !== singleMonster.id && lastAutoSelectedId !== singleMonster.id) {
                lastAutoSelectedId = singleMonster.id;

                if (preventClicksUntilAfterMount) {
                    return;
                }

                onButtonClick(singleMonster.id, true).then();
            }
        } else if (filteredArray.length !== 1) {
            // Reset tracking when there's not exactly one result
            lastAutoSelectedId = null;
        }
    });

    // Load from URL on mount
    onMount(() => {
        // Override defaults with URL parameters if they exist
        let params = tryGetBnpcSearchParams(page.url.searchParams);
        if (params !== undefined) {
            searchQuery = localizedBNpcName(params.nameId, $currentLanguage);
        }

        tick().then(() => {
            preventClicksUntilAfterMount = false;
        })
    });
</script>

<div class="d-flex w-100 flex-column gap-2">
    <div class="input-group">
        <input
            type="text"
            class="form-control"
            placeholder="Search monsters..."
            aria-label="Search"
            bind:value={searchQuery}
        >
    </div>
    {#if searchQuery.trim() === ''}
        <p class="text-muted">Enter a search query to find a monster</p>
    {:else if filteredArray.length === 0}
        <p class="text-muted">No monster found</p>
    {:else}
        <ListGroup>
            {#each filteredArray as monster}
                <ListGroupItem
                    class="list-group-item-xiv-item justify-content-between"
                    active={selectedId === monster.id}
                    onclick={async () => {
                        await onButtonClick(monster.id, true)
                    }}
                    style="cursor: pointer;"
                >
                    <span class="list-group-item-xiv-map-name">
                        {monster.name}
                    </span>
                    <small class="text-nowrap opacity-75 pe-2">{monster.zones} {monster.zones === 1 ? 'zone' : 'zones'}</small>
                </ListGroupItem>
            {/each}
        </ListGroup>
    {/if}
</div>
