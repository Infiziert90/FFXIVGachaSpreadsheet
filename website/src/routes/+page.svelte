<script lang="ts">
    import CardLink from '../component/CardLink.svelte';
    import { navCategories } from '$lib/navConfig';
</script>

<svelte:head>
    <title>Home</title>

    <meta property="og:site_name" content="XIVStats">
    <meta property="og:title" content="Home">
    <meta name="description" content="A collection of stats and tools." />
    <meta property="og:description" content="A collection of stats and tools." />
</svelte:head>

<div class="col-12">
    <h1 class="text-body-emphasis text-center mb-5">Stats & Tools</h1>

    {#each navCategories as category}
        <h3 class="text-center mt-4">{category.label}</h3>
        <div class="row row-cols-1 row-cols-md-3 g-4 mb-5 justify-content-center">
            {#each category.items as item}
                <CardLink cardTitle={item.label} cardIcon={item.icon} cardLink={item.href} />
            {/each}
        </div>
    {/each}
</div>
