<script lang="ts">
    import type {ItemDetail, PoolReward, Sector, SubLoot} from "$lib/interfaces";
    import PageSidebar from "../../component/PageSidebar.svelte";
    import {onMount} from "svelte";
    import ItemSearchbar from "../../component/ItemSearchbar.svelte";
    import SubItemTable from "../../component/SubItemTable.svelte";
    import {SubColumns} from "$lib/table";
    import {Mappings} from "$lib/mappings";
    import SimpleItemCard from "../../component/SimpleItemCard.svelte";
    import {getTierAsNumber} from "$lib/submarines/utils";
    import {tryGetSubmarineSearchSearchParams} from "$lib/searchParamHelper";
    import {page} from "$app/state";
    import {replaceState} from "$app/navigation";

    interface Props {
        data: { content: SubLoot };
    }

    // html elements
    let tabContentElement: HTMLDivElement = $state() as HTMLDivElement;
    let tabElements: { [key: string]: HTMLButtonElement } = $state({});

    let { data }: Props = $props();

    let subData: SubLoot = data.content;

    let selectedId = $state(0);

    // Stats
    let titleStats = $state('');
    let totalStats = $state('');
    let selectedStats = $state('');

    // Table data
    let tableItemId: number = $state(0);
    let tableItems: ItemDetail[] | undefined = $state(undefined);

    let allItems: Record<number, ItemDetail[]> = {};

    let searchItems: Set<number> = new Set();
    for (const sectorData of Object.values(subData.Sectors)) {
        for (const [tier, pool] of Object.entries(sectorData.Pools)) {
            for (const [itemId, reward] of Object.entries(pool.Rewards)) {
                searchItems.add(parseInt(itemId));

                if (!allItems.hasOwnProperty(itemId)) {
                    allItems[itemId] = [];
                }

                allItems[itemId].push({
                    Id: parseInt(itemId),
                    Sector: sectorData.Id,
                    Tier: getTierAsNumber(tier),
                    Poor: `${reward.MinMax['Poor'][0]} - ${reward.MinMax['Poor'][1]}`,
                    Normal: `${reward.MinMax['Normal'][0]} - ${reward.MinMax['Normal'][1]}`,
                    Optimal: `${reward.MinMax['Optimal'][0]} - ${reward.MinMax['Optimal'][1]}`,
                    T3Rate: getT3HitRate(reward, pool.Records, sectorData)
                });
            }
        }
    }

    // Set default meta data
    let title = $state('Submarine Item Search');
    let description = $state('An overview of items and the sectors that drop them.');

    // Override defaults with URL parameters if they exist
    let submarineSearchParams = tryGetSubmarineSearchSearchParams(page.url.searchParams);
    if (submarineSearchParams !== undefined) {
        tableItemId = submarineSearchParams.itemId;

        // svelte-ignore state_referenced_locally
        if (tableItemId in Mappings) {
            title = `Submarine Item Search - ${Mappings[tableItemId].Name}`;
            description = `All known sectors with drops for ${Mappings[tableItemId].Name}.`
        }
    }

    // When page loads, open the tab for the current map
    onMount(async () => {
        if (tableItemId > 0) {
            await onButtonClick(tableItemId, false);
        }
    })

    async function onButtonClick(itemId: number, addQuery: boolean = false) {
        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('item', itemId.toString());
            replaceState(page.url, page.state);
        }

        if (!allItems.hasOwnProperty(itemId)) {
            tableItemId = 0;
            tableItems = undefined;
            return;
        }

        tableItemId = itemId;
        tableItems = allItems[itemId];

        // Scroll to the top of the page
        window.scrollTo(0, 0);

        // Set the new title
        document.title = `Submarine Item Search - ${Mappings[itemId].Name}`;
    }

    function getT3HitRate(reward: PoolReward, poolRecords: number, sector: Sector): string {
        if (poolRecords === 0 || sector.T3Capable === 0)
            return '0.00';

        let capableHit = reward.WasT3 / sector.T3Capable * 100;
        return capableHit.toFixed(2);
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>
{#if tableItems !== undefined}
    <PageSidebar>
        <ItemSearchbar
                items={Array.from(searchItems)}
                {selectedId}
                {onButtonClick}
                {tabElements}
        />
    </PageSidebar>
    <div class="col-12 col-lg-2 order-0 order-lg-3">
    </div>
    <div class="col-12 col-lg-7 order-0 order-lg-2">
        <div id="tabcontent" class="table-responsive" bind:this={tabContentElement}>
            <div class="container mb-5 p-2 rounded border tier-anchor" style="background-color: var(--bs-tertiary-bg);">
                <SimpleItemCard itemId={tableItemId}/>
                {#if tableItems.length > 0}
                    <SubItemTable items={tableItems} columns={SubColumns} />
                {:else}
                    <p>No data for the selected item.</p>
                {/if}
            </div>
        </div>
    </div>
{:else}
    <div class="col-12">
        <ItemSearchbar
                items={Array.from(searchItems)}
                {selectedId}
                {onButtonClick}
                {tabElements}
        />
    </div>
{/if}