<script lang="ts">
    import {getLastUpdate} from "$lib/utils";
    import CardLink from "../component/CardLink.svelte";
    import { browser } from '$app/environment';
</script>

<svelte:head>
    <title>Home</title>

    <meta property="og:site_name" content="Home">
    <meta property="og:title" content="Home">
    <meta name="description" content="A collection of probabilities." />
    <meta property="og:description" content="A collection of probabilities." />
</svelte:head>

<div class="col-12">
    <h1 class="text-body-emphasis">Final Fantasy 14 %-Stats</h1>

    <p>
        <strong>Last Update</strong>:
        {#await getLastUpdate(browser) then value}
            {value}
        {/await}
    </p>

    <p>Legend:</p>
    <blockquote>
        <p>
            “Name” - Name of the item<br>
            “Obtained” - Number of times the item was received<br>
            “Total” - Total number of items received<br>
            “Chance” - % in relation to all records
        </p>
    </blockquote>
    <blockquote>
        <p>Keep in mind that every draw is based on randomness, so values can vary slightly for each individual</p>
    </blockquote>

    <h3 id="content">Content</h3>
    <div class="row row-cols-1 row-cols-md-3 g-4 mb-3">
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
    <h3 id="content">Submarines</h3>
    <div class="row row-cols-1 row-cols-md-3 g-4 mb-3">
        <CardLink cardTitle={'Submarine Loot'} cardIcon={63191} cardLink={'submarine/'} />
    </div>
    <h3 id="content">Maps</h3>
    <div class="row row-cols-1 row-cols-md-3 g-4 mb-3">
        <CardLink cardTitle={'Monster Locations'} cardIcon={61837} cardLink={'bnpc/'} />
        <CardLink cardTitle={'Housing Info'} cardIcon={60756} cardLink={'housing/'} />
        <CardLink cardTitle={'Open Plots'} cardIcon={60758} cardLink={'housingOpen/'} />
    </div>
</div>