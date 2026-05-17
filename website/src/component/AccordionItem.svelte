<script lang="ts">
    import Collapse from "./Collapse.svelte";

    interface Props {
        open: boolean;
        ontoggle?: () => void;
        animate?: boolean;
        children?: import('svelte').Snippet;
        header?: import('svelte').Snippet;
    }

    let { open, ontoggle = () => {}, animate = true, children, header }: Props = $props();
</script>

<div class="accordion-item">
    <h2 class="accordion-header">
        <button
            class="accordion-button"
            class:collapsed={!open}
            type="button"
            onclick={ontoggle}
        >
            {#if header}
                {@render header()}
            {/if}
        </button>
    </h2>
    <Collapse isOpen={open} {animate} class="accordion-collapse">
        <div class="accordion-body">
            {#if children}
                {@render children()}
            {/if}
        </div>
    </Collapse>
</div>