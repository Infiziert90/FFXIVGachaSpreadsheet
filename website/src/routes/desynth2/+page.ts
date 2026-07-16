import type { PageLoad } from './$types';
import {loadDesynthesisBase, loadMapping} from "$lib/loadHelpers";
import {LoadJobSheet} from "$lib/sheets/simplifiedSheets";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);
    let jobPromise = LoadJobSheet(fetch);

    const res = await loadDesynthesisBase('/data/desynthesis2/base.json', fetch);
    await jobPromise;
    await mappingPromise;

    return res;
};