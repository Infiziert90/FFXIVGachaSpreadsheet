<script lang="ts">
    interface Props {
        value: string;
        placeholder: string;
        ariaLabel: string;
    }

    let { value = $bindable(''), placeholder, ariaLabel }: Props = $props();
</script>

<div class="input-group">
    <input
            type="text"
            class="form-control"
            {placeholder}
            aria-label={ariaLabel}
            bind:value
    >
    {#if value}
        <button
                class="btn btn-outline-secondary"
                type="button"
                aria-label="Clear search"
                onclick={() => value = ''}
        >
            &times;
        </button>
    {/if}
</div>
