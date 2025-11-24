<script lang="ts">
    import { Accordion, AccordionItem } from '@sveltestrap/sveltestrap';
    import type { Coffer } from "$lib/interfaces";

    interface Props {
        cofferData: Coffer[];
        territory: number;
        openTab: (territory: number, coffer: number, addQuery: boolean) => void;
        tabElements: { [key: string]: HTMLButtonElement };
    }

    let { cofferData, territory, openTab, tabElements }: Props = $props();
</script>

<Accordion theme="dark" class="w-100">
    {#each cofferData as coffer}
        <AccordionItem active={coffer.Territory === territory}>
            <div slot="header">{coffer.Name}</div>
            <div class="accordion-body">
                {#each coffer.Coffers as cofferVariant}
                    <div class="tab">
                        <button 
                            id="{coffer.Territory}{cofferVariant.CofferId}-tab"
                            class="tablinks btn accordion-body-btn"
                            onclick={() => openTab(coffer.Territory, cofferVariant.CofferId, true)}
                            bind:this={tabElements[`${coffer.Territory}${cofferVariant.CofferId}`]}
                        >
                            {cofferVariant.CofferName}
                        </button>
                    </div>
                {/each}
            </div>
        </AccordionItem>
    {/each}
</Accordion>

