import type { PageLoad } from './$types';
import {loadChestDrops} from "$lib/loadHelpers";
import {LoadMapSheet} from "$lib/sheets/simplifiedSheets";
import {loadItemMapping} from "$lib/mappings";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadItemMapping(fetch);

    const res = await loadChestDrops('/website/ChestDropsWeb.json.gz', fetch)
    await LoadMapSheet(fetch);
    await mappingPromise;

    return res;
};