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
        <AccordionItem active={coffer.TerritoryId === territory}>
            <div slot="header">{coffer.Name}</div>
            <div class="accordion-body">
                {#each coffer.Variants as cofferVariant}
                    <div class="tab">
                        <button 
                            id="{coffer.TerritoryId}{cofferVariant.Id}-tab"
                            class="tablinks btn accordion-body-btn"
                            onclick={() => openTab(coffer.TerritoryId, cofferVariant.Id, true)}
                            bind:this={tabElements[`${coffer.TerritoryId}${cofferVariant.Id}`]}
                        >
                            {cofferVariant.Name}
                        </button>
                    </div>
                {/each}
            </div>
        </AccordionItem>
    {/each}
</Accordion>

