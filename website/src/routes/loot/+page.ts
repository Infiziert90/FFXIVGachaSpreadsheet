import type { PageLoad } from './$types';
import {loadChestDrops, loadCoffer, loadMapping} from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);

    const res = await loadChestDrops('/data/ChestDrops.json', fetch)
    await mappingPromise;

    return res;
};