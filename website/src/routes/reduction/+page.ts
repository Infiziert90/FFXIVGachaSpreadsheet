import type { PageLoad } from './$types';
import {loadMapping, loadReduction} from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);

    const res = await loadReduction('/data/Reduction.json', fetch)
    await mappingPromise;

    return res;
};