<script lang="ts">
    import AccordionItem from "./AccordionItem.svelte";
    import { ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import type {Reduction} from "$lib/structs/reduction";
    import {Mappings} from "$lib/mappings";

    interface Props {
        reductionData: Reduction;
        source: number;
        openTab: (source: number, addQuery: boolean) => void;
    }

    let { reductionData, source, openTab }: Props = $props();

    function handleItemClick(sourceId: number, e: Event) {
        e.stopPropagation?.();
        openTab(sourceId, true);
    }
</script>

<div class="accordion w-100">
    <!-- /**
     * Iterates through cofferData array and uses cofferItem.TerritoryId
     * as the unique key for each item in the each-block.
     */ -->
    <AccordionItem open={true}>
        {#snippet header()}
            {"Aetherial Reduction"}
        {/snippet}
        <ListGroup>
            {#each Object.keys(reductionData.Sources) as sourceId}
                <ListGroupItem
                    id="{sourceId}-tab"
                    active={sourceId === source}
                    tag="button"
                    action
                    onclick={(e) => handleItemClick(sourceId, e)}
                >
                    {Mappings[sourceId].Name}
                </ListGroupItem>
            {/each}
        </ListGroup>
    </AccordionItem>
</div>
