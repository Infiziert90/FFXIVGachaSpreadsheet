import type { PageLoad } from './$types';
import {loadMapping, loadReduction} from "$lib/loadHelpers";
import {LoadReductionRewardSheet} from "$lib/sheets/simplifiedSheets";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);

    const res = await loadReduction('/data/Reduction.json', fetch)
    await LoadReductionRewardSheet(fetch);
    await mappingPromise;

    return res;
};