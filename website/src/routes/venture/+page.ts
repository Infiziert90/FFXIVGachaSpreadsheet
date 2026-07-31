import type { PageLoad } from './$types';
import {loadVentures} from "$lib/loadHelpers";
import {loadItemMapping} from "$lib/mappings";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadItemMapping(fetch);

    const res = await loadVentures('/data/Ventures.json', fetch)
    await mappingPromise;

    return res;
};