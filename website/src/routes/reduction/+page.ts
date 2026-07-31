import type { PageLoad } from './$types';
import {loadReduction} from "$lib/loadHelpers";
import {LoadReductionRewardSheet} from "$lib/sheets/simplifiedSheets";
import {loadItemMapping} from "$lib/mappings";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadItemMapping(fetch);

    const res = await loadReduction('/data/Reduction.json', fetch)
    await LoadReductionRewardSheet(fetch);
    await mappingPromise;

    return res;
};