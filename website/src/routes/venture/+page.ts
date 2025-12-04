import type { PageLoad } from './$types';
import {loadMapping, loadVentures} from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);

    const res = await loadVentures('/data/Ventures.json', fetch)
    await mappingPromise;

    return res;
};