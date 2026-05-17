<script lang="ts">
    import AccordionItem from "./AccordionItem.svelte";
    import { ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import type { Venture } from "$lib/interfaces";

    interface Props {
        ventureData: Venture[];
        category: number;
        taskType: number;
        openTab: (categoryId: number, taskTypeId: number, addQuery: boolean) => void;
    }

    let { ventureData, category, taskType, openTab }: Props = $props();
    
    // Open accordion when territory changes - sync with territory prop
    let openAccordionId = $state<number | null>(
        ventureData.find(v => v.Category === category)?.Category ?? null
    );
    
    // Update openAccordionId when territory prop changes
    $effect(() => {
        const newId = ventureData.find(c => c.Category === category)?.Category ?? null;
        if (newId !== null) {
            openAccordionId = newId;
        }
    });

    function toggleNode(categoryId: number) {
        openAccordionId = openAccordionId === categoryId ? null : categoryId;
    }

    function handleItemClick(categoryId: number, taskTypeId: number, e: Event) {
        e.stopPropagation?.();
        openTab(categoryId, taskTypeId, true);
    }
</script>

<div class="accordion w-100">
    <!-- /**
     * Iterates through ventureData array and uses ventureItem.Category as the unique key
     * for each item in the each-block.
     */ -->
    {#each ventureData as ventureItem (ventureItem.Category)}
        <AccordionItem
            open={openAccordionId === ventureItem.Category}
            ontoggle={() => toggleNode(ventureItem.Category)}
        >
            {#snippet header()}
                {ventureItem.Name}
            {/snippet}
            <ListGroup>
                <!-- /**
                 * Iterates through ventureItem.Tasks array and uses taskVariant.Type
                 * as the unique key for each item in the each-block.
                 */ -->
                {#each ventureItem.Tasks as taskVariant (taskVariant.Type)}
                    <ListGroupItem 
                        id="{ventureItem.Category}{taskVariant.Type}-tab"
                        active={category === ventureItem.Category && taskType === taskVariant.Type}
                        tag="button"
                        action
                        onclick={(e) => handleItemClick(ventureItem.Category, taskVariant.Type, e)}
                    >
                        {taskVariant.Name}
                    </ListGroupItem>
                {/each}
            </ListGroup>
        </AccordionItem>
    {/each}
</div>
