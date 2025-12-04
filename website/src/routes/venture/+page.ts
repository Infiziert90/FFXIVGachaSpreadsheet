import type { PageLoad } from './$types';
import { loadVentures } from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ parent, fetch }) => {
    let mappingPromise = parent();

    const res = await loadVentures('/data/Ventures.json', fetch)
    await mappingPromise;

    return res;
};