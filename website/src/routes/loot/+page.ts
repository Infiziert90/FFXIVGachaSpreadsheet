import type { PageLoad } from './$types';
import {loadChestDrops, loadMapping} from "$lib/loadHelpers";
import {LoadMapSheet} from "$lib/sheets/simplifiedSheets";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);

    const res = await loadChestDrops('/data/ChestDrops.json', fetch)
    await LoadMapSheet(fetch);
    await mappingPromise;

    return res;
};