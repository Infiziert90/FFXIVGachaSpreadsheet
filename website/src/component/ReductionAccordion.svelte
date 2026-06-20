<script lang="ts">
    import AccordionItem from "./AccordionItem.svelte";
    import { ListGroup, ListGroupItem } from '@sveltestrap/sveltestrap';
    import type {Reduction} from "$lib/structs/reduction";
    import { Mappings } from "$lib/mappings";

    interface Props {
        reductionData: Reduction;
        job: number;
        source: number;
        openTab: (jobId: number, sourceId: number, addQuery: boolean) => void;
    }

    let { reductionData, job, source, openTab }: Props = $props();

    // Open accordion when job changes - sync with job prop
    let openAccordionId = $state<number | null>(
        reductionData.Jobs.find(c => c.Id === job)?.Id ?? null
    );

    // Update openAccordionId when job prop changes
    $effect(() => {
        const newId = reductionData.Jobs.find(c => c.Id === job)?.Id ?? null;
        if (newId !== null) {
            openAccordionId = newId;
        }
    });

    function toggleNode(sourceId: number) {
        openAccordionId = openAccordionId === sourceId ? null : sourceId;
    }

    function handleItemClick(jobId: number, sourceId: number, e: Event) {
        e.stopPropagation?.();
        openTab(jobId, sourceId, true);
    }
</script>

<div class="accordion w-100">
    <!-- /**
     * Iterates through reduction data array and uses job id
     * as the unique key for each item in the each-block.
     */ -->
    {#each reductionData.Jobs as jobRow (jobRow.Id)}
        <AccordionItem
                open={openAccordionId === jobRow.Id}
                ontoggle={() => toggleNode(jobRow.Id)}
        >
            {#snippet header()}
                {jobRow.Name}
            {/snippet}
            <ListGroup>
                <!-- /**
                 * Iterates through jobRow.Sources array and uses source id
                 * as the unique key for each item in the each-block.
                 */ -->
                {#each jobRow.Sources as sourceRow (sourceRow.Id)}
                    <ListGroupItem
                            id="{jobRow.Id}{sourceRow.Id}-tab"
                            active={job === jobRow.Id && source === sourceRow.Id}
                            tag="button"
                            action
                            onclick={(e) => handleItemClick(jobRow.Id, sourceRow.Id, e)}
                    >
                        {Mappings[sourceRow.Id].Name}
                    </ListGroupItem>
                {/each}
            </ListGroup>
        </AccordionItem>
    {/each}
</div>
