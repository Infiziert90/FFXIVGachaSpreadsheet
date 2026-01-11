<script lang="ts">
    import { Accordion, AccordionItem, ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import type { Coffer } from "$lib/interfaces";

    interface Props {
        cofferData: Coffer[];
        territory: number;
        coffer: number;
        openTab: (territory: number, coffer: number, addQuery: boolean) => void;
    }

    let { cofferData, territory, coffer, openTab }: Props = $props();
    
    // Open accordion when territory changes - sync with territory prop
    let openAccordionId = $state<number | null>(
        cofferData.find(c => c.TerritoryId === territory)?.TerritoryId ?? null
    );
    
    // Update openAccordionId when territory prop changes
    $effect(() => {
        const newId = cofferData.find(c => c.TerritoryId === territory)?.TerritoryId ?? null;
        if (newId !== null) {
            openAccordionId = newId;
        }
    });

    function handleToggle(cofferId: number, e: CustomEvent) {
        e.stopPropagation?.();
        const isActive = typeof e.detail === 'boolean' ? e.detail : (e.detail as { active?: boolean })?.active;
        if (isActive !== undefined) {
            openAccordionId = isActive ? cofferId : (openAccordionId === cofferId ? null : openAccordionId);
        }
    }

    function handleItemClick(territory: number, coffer: number, e: Event) {
        e.stopPropagation?.();
        openTab(territory, coffer, true);
    }
</script>

<Accordion class="w-100">
    /**
     * Iterates through cofferData array and uses cofferItem.TerritoryId
     * as the unique key for each item in the each-block.
     */
    {#each cofferData as cofferItem (cofferItem.TerritoryId)}
        <AccordionItem 
            active={openAccordionId === cofferItem.TerritoryId}
            ontoggle={(e) => handleToggle(cofferItem.TerritoryId, e)}
        >
            <div slot="header">{cofferItem.Name}</div>
            <ListGroup>
                /**
                 * Iterates through cofferItem.Variants array and uses cofferVariant.Id
                 * as the unique key for each item in the each-block.
                 */
                {#each cofferItem.Variants as cofferVariant (cofferVariant.Id)}
                    <ListGroupItem 
                        id="{cofferItem.TerritoryId}{cofferVariant.Id}-tab"
                        active={territory === cofferItem.TerritoryId && coffer === cofferVariant.Id}
                        tag="button"
                        action
                        onclick={(e) => handleItemClick(cofferItem.TerritoryId, cofferVariant.Id, e)}
                    >
                        {cofferVariant.Name}
                    </ListGroupItem>
                {/each}
            </ListGroup>
        </AccordionItem>
    {/each}
</Accordion>
