<script lang="ts">
    import AccordionItem from "./AccordionItem.svelte";
    import { ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import {type SubMapRow, ToMapName} from "$lib/sheets/structure/subMap";

    interface Props {
        mapData: SubMapRow[];
        map: number;
        openTab: (map: number, addQuery: boolean) => void;
    }

    let { mapData, map, openTab }: Props = $props();
    
    // Open accordion when territory changes - sync with territory prop
    let isOpen = $state<boolean>(true);

    function toggleOpen() {
        isOpen = !isOpen;
    }

    function handleItemClick(mapId: number, e: Event) {
        e.stopPropagation?.();
        openTab(mapId, true);
    }
</script>

<div class="accordion w-100">
    <AccordionItem open={isOpen} ontoggle={toggleOpen}>
        {#snippet header()}
            Maps
        {/snippet}
        <ListGroup>
            <!-- /** 
             * Iterates through mapData array and uses mapItem.RowId
             * as the unique key for each item in the each-block.
             */ -->
            {#each mapData as mapItem (mapItem.RowId)}
                <ListGroupItem 
                    id="{mapItem.RowId}-tab"
                    active={map === mapItem.RowId}
                    tag="button"
                    action
                    onclick={(e) => handleItemClick(mapItem.RowId, e)}
                >
                    {ToMapName(mapItem)}
                </ListGroupItem>
            {/each}
        </ListGroup>
    </AccordionItem>
</div>
