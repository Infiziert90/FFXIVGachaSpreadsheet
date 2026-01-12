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
        e.stopPropagation?.();
        const isActive = typeof e.detail === 'boolean' ? e.detail : (e.detail as { active?: boolean })?.active;
        if (isActive !== undefined) {
            openAccordionId = isActive ? mapId : (openAccordionId === mapId ? null : openAccordionId);
        }
    }

    function handleItemClick(mapId: number, e: Event) {
        e.stopPropagation?.();
        openTab(mapId, true);
    }
</script>

<Accordion class="w-100">
        <AccordionItem active={true}>
            <div slot="header">Maps</div>
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
                        {ToName(mapItem)}
                    </ListGroupItem>
                {/each}
            </ListGroup>
        </AccordionItem>
</Accordion>
