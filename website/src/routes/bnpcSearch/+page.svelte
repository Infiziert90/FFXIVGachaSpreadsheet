<script lang="ts">
    import PageSidebar from "../../component/PageSidebar.svelte";
    import BnpcSearchbar from "../../component/BnpcSearchbar.svelte";
    import {onMount} from "svelte";
    import {browser} from "$app/environment";
    import {page} from "$app/state";
    import {replaceState} from "$app/navigation";
    import type {BnpcPairing, Pairing} from "$lib/structs/bnpc";
    import {SimpleBNpcNameSheet, SimpleMapSheet, SimpleTerritorySheet} from "$lib/sheets/simplifiedSheets";
    import {getWikiUrl} from "$lib/utils";
    import {tryGetBnpcSearchParams} from "$lib/searchParamHelper";
    import {localizedBNpcName} from "$lib/localization";
    import {currentLanguage} from "$lib/stores/language";

    interface Props {
        data: { content: BnpcPairing };
    }

    interface MonsterGroup {
        id: number;
        nameIds: Set<number>;
        baseIds: Set<number>;
        pairings: Pairing[];
        zones: Set<string>;
    }

    interface ZoneResult {
        territory: number;
        map: number;
        name: string;
        instanced: boolean;
        questBattle: boolean;
        levels: Set<number>;
        forayLevels: Set<number>;
    }

    let { data }: Props = $props();
    let pairingData: BnpcPairing = data.content;

    // Multiple name ids can share the same name, so monsters are grouped by their english name
    const groups: Record<string, MonsterGroup> = {};
    const nameIdToGroup: Record<number, MonsterGroup> = {};
    if (browser) {
        for (const pairing of Object.values(pairingData.BPairs)) {
            if (pairing.B === 0 || pairing.N === 0)
                continue;

            const englishName = SimpleBNpcNameSheet[pairing.N]?.['En'];
            if (!englishName)
                continue;

            if (!groups.hasOwnProperty(englishName)) {
                groups[englishName] = {id: pairing.N, nameIds: new Set(), baseIds: new Set(), pairings: [], zones: new Set()};
            }

            const group = groups[englishName];
            group.nameIds.add(pairing.N);
            group.baseIds.add(pairing.B);
            group.pairings.push(pairing);
            for (const location of Object.values(pairing.L))
                group.zones.add(`${location.T}-${location.M}`);

            nameIdToGroup[pairing.N] = group;
        }
    }

    const searchMonsters = Object.values(groups).map(group => ({id: group.id, zones: group.zones.size}));

    let selectedGroup: MonsterGroup | undefined = $state(undefined);
    let zoneResults: ZoneResult[] = $state([]);

    // Set default meta data
    let title = $state('Monster Search');
    let description = $state('Search a monster by name and find every location it has been spotted at.');

    // Override defaults with URL parameters if they exist
    let bnpcSearchParams = tryGetBnpcSearchParams(page.url.searchParams);
    if (bnpcSearchParams !== undefined) {
        // svelte-ignore state_referenced_locally
        title = `Monster Search - ${localizedBNpcName(bnpcSearchParams.nameId, $currentLanguage)}`;
        description = `All known locations of ${localizedBNpcName(bnpcSearchParams.nameId, 'En')}.`;
    }

    onMount(async () => {
        if (bnpcSearchParams !== undefined) {
            await onButtonClick(bnpcSearchParams.nameId, false);
        }
    })

    async function onButtonClick(nameId: number, addQuery: boolean = false) {
        const group = nameIdToGroup[nameId];
        if (group === undefined) {
            selectedGroup = undefined;
            zoneResults = [];
            return;
        }

        // Update URL if requested (when user clicks a button)
        if (addQuery) {
            page.url.searchParams.set('name', group.id.toString());
            replaceState(page.url, page.state);
        }

        selectedGroup = group;
        zoneResults = buildZoneResults(group);

        window.scrollTo(0, 0);
        document.title = `Monster Search - ${localizedBNpcName(group.id, $currentLanguage)}`;
    }

    function buildZoneResults(group: MonsterGroup): ZoneResult[] {
        const zones: Record<string, ZoneResult> = {};
        for (const pairing of group.pairings) {
            for (const location of Object.values(pairing.L)) {
                const key = `${location.T}-${location.M}`;
                if (!zones.hasOwnProperty(key)) {
                    const territory = SimpleTerritorySheet[location.T];
                    zones[key] = {
                        territory: location.T,
                        map: location.M,
                        name: getMapName(location.M),
                        instanced: territory.ContentFinderCondition > 0,
                        questBattle: territory.QuestBattle > 0,
                        levels: new Set(),
                        forayLevels: new Set()
                    };
                }

                if (location.FL === 0)
                    zones[key].levels.add(location.L);
                else
                    zones[key].forayLevels.add(location.FL);
            }
        }

        // Open world zones first, then alphabetically
        return Object.values(zones).sort((a, b) => {
            const aInstanced = a.instanced || a.questBattle;
            const bInstanced = b.instanced || b.questBattle;
            if (aInstanced !== bInstanced)
                return aInstanced ? 1 : -1;

            return a.name.localeCompare(b.name);
        });
    }

    function getMapName(mapId: number): string {
        const map = SimpleMapSheet[mapId];
        return `${map.PlaceName.Name}${map.PlaceNameSub.Name.length > 1 ? ` - ${map.PlaceNameSub.Name}` : ''}`;
    }

    function formatLevels(levels: Set<number>): string {
        const sorted = Array.from(levels).sort((a, b) => a - b);
        if (sorted.length === 0)
            return '';

        const min = sorted[0];
        const max = sorted[sorted.length - 1];
        return min === max ? `${min}` : `${min} - ${max}`;
    }

    function getMapLink(zone: ZoneResult): string {
        return `/bnpc/?territory=${zone.territory}&map=${zone.map}&name=${selectedGroup!.id}`;
    }
</script>

<svelte:head>
    <title>{title}</title>

    <meta property="og:title" content={title}>
    <meta name="description" content={description} />
    <meta property="og:description" content={description} />
</svelte:head>
{#if selectedGroup !== undefined}
    <PageSidebar title="Monster search">
        <BnpcSearchbar
                monsters={searchMonsters}
                selectedId={selectedGroup.id}
                {onButtonClick}
        />
    </PageSidebar>
    <div class="col-12 col-lg-2 order-0 order-lg-3">
    </div>
    <div class="col-12 col-lg-7 order-0 order-lg-2">
        <div class="container mb-5 p-2 rounded border" style="background-color: var(--bs-tertiary-bg);">
            <div class="card mb-3">
                <div class="card-body">
                    <h5 class="card-title">
                        <a href={getWikiUrl(localizedBNpcName(selectedGroup.id, 'En'))} class="link-body-emphasis link-offset-2 link-underline link-underline-opacity-0" target="_blank">{localizedBNpcName(selectedGroup.id, $currentLanguage)}</a>
                    </h5>
                    <p class="card-text text-muted small mb-0">
                        Name IDs: {Array.from(selectedGroup.nameIds).join(', ')} · Base IDs: {Array.from(selectedGroup.baseIds).join(', ')}
                    </p>
                </div>
            </div>

            {#if zoneResults.length > 0}
                <div class="table-responsive">
                    <table class="table table-striped table-hover align-middle mb-0">
                        <thead>
                        <tr>
                            <th>Zone</th>
                            <th>Level</th>
                            <th></th>
                        </tr>
                        </thead>
                        <tbody>
                        {#each zoneResults as zone}
                            <tr>
                                <td>
                                    {zone.name}
                                    {#if zone.instanced}
                                        <span class="badge text-bg-secondary ms-1">Instanced</span>
                                    {/if}
                                    {#if zone.questBattle}
                                        <span class="badge text-bg-secondary ms-1">Quest</span>
                                    {/if}
                                </td>
                                <td class="text-nowrap">
                                    {#if zone.levels.size > 0}
                                        Lv. {formatLevels(zone.levels)}
                                    {/if}
                                    {#if zone.forayLevels.size > 0}
                                        <span class="d-block">Foray Lv. {formatLevels(zone.forayLevels)}</span>
                                    {/if}
                                </td>
                                <td class="text-end">
                                    {#if !zone.questBattle}
                                        <a href={getMapLink(zone)} class="btn btn-sm btn-outline-secondary text-nowrap">
                                            <i class="bi bi-map"></i> Map
                                        </a>
                                    {/if}
                                </td>
                            </tr>
                        {/each}
                        </tbody>
                    </table>
                </div>
            {:else}
                <p>No locations recorded for this monster.</p>
            {/if}
        </div>
    </div>
{:else}
    <div class="col-lg-3"></div>
    <div class="col-lg-6 col-12">
        <BnpcSearchbar
                monsters={searchMonsters}
                selectedId={0}
                {onButtonClick}
        />
    </div>
    <div class="col-lg-3"></div>
{/if}
