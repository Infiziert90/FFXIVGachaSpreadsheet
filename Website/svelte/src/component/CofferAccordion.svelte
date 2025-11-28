<script lang="ts">
    import { Accordion, AccordionItem } from '@sveltestrap/sveltestrap';
    import type { Coffer } from "$lib/interfaces";
    import { tick } from 'svelte';

    interface Props {
        cofferData: Coffer[];
        territory: number;
        coffer: number;
        openTab: (territory: number, coffer: number, addQuery: boolean) => void;
        tabElements: { [key: string]: HTMLButtonElement };
    }

    let { cofferData, territory, coffer, openTab, tabElements }: Props = $props();
    
    // Open accordion when territory changes
    let openAccordionId = $state<number | null>(
        cofferData.find(c => c.TerritoryId === territory)?.TerritoryId ?? null
    );

    // Mark button as active when territory/coffer changes
    $effect(() => {
        if (!territory || !coffer || openAccordionId !== territory) return;
        
        const markActive = () => {
            Object.values(tabElements).forEach(el => el?.classList.remove('active'));
            const button = tabElements[`${territory}${coffer}`];
            button?.classList.add('active');
        };

        const button = tabElements[`${territory}${coffer}`];
        if (button) {
            markActive();
        } else {
            tick().then(markActive);
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
            <div class="accordion-body">
                {#each cofferItem.Variants as cofferVariant}
                    <div class="tab">
                        <button 
                            id="{cofferItem.TerritoryId}{cofferVariant.Id}-tab"
                            class="tablinks btn accordion-body-btn"
                            onclick={() => openTab(cofferItem.TerritoryId, cofferVariant.Id, true)}
                            bind:this={tabElements[`${cofferItem.TerritoryId}${cofferVariant.Id}`]}
                        >
                            {cofferVariant.Name}
                        </button>
                    </div>
                {/each}
            </div>
        </AccordionItem>
    {/each}
</Accordion>
