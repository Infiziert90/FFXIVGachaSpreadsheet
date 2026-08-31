import type { PageLoad } from './$types';
import {loadFate} from "$lib/loadHelpers";
import {
    LoadDynamicEventSheet,
    LoadExVersionSheet,
    LoadFateSheet,
    LoadTerritorySheet
} from "$lib/sheets/simplifiedSheets";
import {loadItemMapping} from "$lib/mappings";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadItemMapping(fetch);

    const res = await loadFate('/data/FateReward.json', fetch)
    await LoadTerritorySheet(fetch);
    await LoadExVersionSheet(fetch);
    await LoadFateSheet(fetch);
    await LoadDynamicEventSheet(fetch);
    await mappingPromise;

    return res;
};