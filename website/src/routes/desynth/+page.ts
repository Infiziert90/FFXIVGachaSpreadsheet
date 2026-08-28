import type { PageLoad } from './$types';
import {loadDesynthesisBase} from "$lib/loadHelpers";
import {LoadJobSheet} from "$lib/sheets/simplifiedSheets";
import {loadItemMapping} from "$lib/mappings";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadItemMapping(fetch);
    let jobPromise = LoadJobSheet(fetch);

    const res = await loadDesynthesisBase('/data/desynthesis2/base.json', fetch);
    await jobPromise;
    await mappingPromise;

    return res;
};