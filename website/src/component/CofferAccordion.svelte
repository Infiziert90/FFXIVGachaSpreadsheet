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
        const isActive = typeof e.detail === 'boolean' ? e.detail : (e.detail as { active?: boolean })?.active;
        if (isActive !== undefined) {
            openAccordionId = isActive ? cofferId : (openAccordionId === cofferId ? null : openAccordionId);
        }
    }
</script>

<Accordion class="w-100">
    {#each cofferData as cofferItem}
        <AccordionItem 
            active={openAccordionId === cofferItem.TerritoryId}
            on:toggle={(e) => handleToggle(cofferItem.TerritoryId, e)}
        >
            <div slot="header">{cofferItem.Name}</div>
            <ListGroup>
                {#each cofferItem.Variants as cofferVariant}
                    <ListGroupItem 
                        id="{cofferItem.TerritoryId}{cofferVariant.Id}-tab"
                        active={territory === cofferItem.TerritoryId && coffer === cofferVariant.Id}
                        tag="button"
                        action
                        on:click={() => openTab(cofferItem.TerritoryId, cofferVariant.Id, true)}
                    >
                        {cofferVariant.Name}
                    </ListGroupItem>
                {/each}
            </ListGroup>
        </AccordionItem>
    {/each}
</Accordion>
