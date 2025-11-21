<script lang="ts">
    import {getLastUpdate} from "$lib/utils";
    import CardLink from "../component/CardLink.svelte";
    import {description, title} from "$lib/title.svelte";

    title.set('Home')
    description.set('A collection of probabilities.')
</script>

<div class="col-12">
    <h1 class="text-body-emphasis">Final Fantasy 14 %-Stats</h1>

    <p>
        <strong>Last Update</strong>:
        {#await getLastUpdate() then value}
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
        <CardLink cardTitle={'Random Coffers'} cardIcon={'061000/061812'} cardLink={'coffer/'} />
        <CardLink cardTitle={'Lockboxes'} cardIcon={'061000/061808'} cardLink={'lockbox/'} />
        <CardLink cardTitle={'Card Packs'} cardIcon={'061000/061820'} cardLink={'card/'} />
        <CardLink cardTitle={'Ventures'} cardIcon={'061000/061818'} cardLink={'venture/'} />
        <CardLink cardTitle={'Eureka Bunnies'} cardIcon={'061000/061833'} cardLink={'bunny/'} />
        <CardLink cardTitle={'Deep Dungeons'} cardIcon={'061000/061824'} cardLink={'deep/'} />
        <CardLink cardTitle={'Desynthesis'} cardIcon={'000000/000120'} cardLink={'desynth/'} />
        <CardLink cardTitle={'Loot'} cardIcon={'061000/061801'} cardLink={'loot/'} />
        <CardLink cardTitle={'Occult'} cardIcon={'061000/061851'} cardLink={'occult/'} />
        <CardLink cardTitle={'Logograms and Fragments'} cardIcon={'061000/061837'} cardLink={'logofrag/'} />
    </div>
</div>