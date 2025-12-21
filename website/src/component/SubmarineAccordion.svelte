<script lang="ts">
    import { Accordion, AccordionItem, ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import {type SubmarineMap, ToName} from "$lib/sheets/submarineMap";

    interface Props {
        mapData: SubmarineMap[];
        map: number;
        openTab: (map: number, addQuery: boolean) => void;
    }

    let { mapData, map, openTab }: Props = $props();
    
    // Open accordion when territory changes - sync with territory prop
    let openAccordionId = $state<number | null>(map);
    
    // Update openAccordionId when territory prop changes
    $effect(() => {
        const newId = mapData[map];
        if (newId !== null) {
            openAccordionId = newId;
        }
    });

    function handleToggle(mapId: number, e: CustomEvent) {
        const isActive = typeof e.detail === 'boolean' ? e.detail : (e.detail as { active?: boolean })?.active;
        if (isActive !== undefined) {
            openAccordionId = isActive ? mapId : (openAccordionId === mapId ? null : openAccordionId);
        }
    }
</script>

<Accordion class="w-100">
        <AccordionItem active={true}>
            <div slot="header">Maps</div>
            <ListGroup>
                {#each mapData as mapItem}
                    <ListGroupItem 
                        id="{mapItem.RowId}-tab"
                        active={map === mapItem.RowId}
                        tag="button"
                        action
                        on:click={() => openTab(mapItem.RowId, true)}
                    >
                        {ToName(mapItem)}
                    </ListGroupItem>
                {/each}
            </ListGroup>
        </AccordionItem>
</Accordion>
