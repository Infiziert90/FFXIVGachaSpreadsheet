import type { PageLoad } from './$types';
import {loadChestDrops, loadCoffer} from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ parent, fetch }) => {
    let mappingPromise = parent();

    const res = await loadChestDrops('/data/ChestDrops.json', fetch)
    await mappingPromise;

    return res;
};