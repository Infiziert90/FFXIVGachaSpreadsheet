<script lang="ts">
    import { slide } from 'svelte/transition';

    interface Props {
        isOpen: boolean;
        animate?: boolean;
        class?: string;
        style?: string;
        children?: import('svelte').Snippet;
    }

    let { isOpen, animate = true, class: className = '', style = '', children }: Props = $props();
</script>

{#if isOpen}
    <div
        transition:slide={{ duration: animate ? 200 : 0 }}
        style="overflow: hidden;{style ? ` ${style}` : ''}"
        class={className}
    >
        {#if children}
            {@render children()}
        {/if}
    </div>
{/if}