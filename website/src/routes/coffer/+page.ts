import type { PageLoad } from './$types';
import {loadCoffer} from "$lib/loadHelpers";
import {loadItemMapping} from "$lib/mappings";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadItemMapping(fetch);

    const res = await loadCoffer('/data/RandomCoffers.json', fetch)
    await mappingPromise;

    return res;
};