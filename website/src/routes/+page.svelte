<script lang="ts">
    import CardLink from "../component/CardLink.svelte";
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

    <h3 id="content" class="text-center mt-4">Content</h3>
    <div class="row row-cols-1 row-cols-md-3 g-4 mb-5 justify-content-center">
        <CardLink cardTitle={'Random Coffers'} cardIcon={61812} cardLink={'coffer/'} />
        <CardLink cardTitle={'Lockboxes'} cardIcon={61808} cardLink={'lockbox/'} />
        <CardLink cardTitle={'Card Packs'} cardIcon={61820} cardLink={'card/'} />
        <CardLink cardTitle={'Ventures'} cardIcon={61818} cardLink={'venture/'} />
        <CardLink cardTitle={'Eureka Bunnies'} cardIcon={61833} cardLink={'bunny/'} />
        <CardLink cardTitle={'Deep Dungeons'} cardIcon={61824} cardLink={'deep/'} />
        <CardLink cardTitle={'Desynthesis'} cardIcon={120} cardLink={'desynth/'} />
        <CardLink cardTitle={'Loot'} cardIcon={61801} cardLink={'loot/'} />
        <CardLink cardTitle={'Occult'} cardIcon={61851} cardLink={'occult/'} />
        <CardLink cardTitle={'Logograms and Fragments'} cardIcon={61837} cardLink={'logofrag/'} />
    </div>
    <h3 id="content" class="text-center mt-4">Submarines</h3>
    <div class="row row-cols-1 row-cols-md-3 g-4 mb-5 justify-content-center">
        <CardLink cardTitle={'Submarine Loot'} cardIcon={63191} cardLink={'submarine/'} />
    </div>
    <h3 id="content" class="text-center mt-4">Maps</h3>
    <div class="row row-cols-1 row-cols-md-3 g-4 mb-5 justify-content-center">
        <CardLink cardTitle={'Monster Locations'} cardIcon={61837} cardLink={'bnpc/'} />
        <CardLink cardTitle={'Housing Info'} cardIcon={60756} cardLink={'housing/'} />
        <CardLink cardTitle={'Open Plots'} cardIcon={60758} cardLink={'housingOpen/'} />
    </div>
</div>