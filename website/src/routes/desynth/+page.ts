import type { PageLoad } from './$types';
import {loadDesynth, loadMapping} from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);

    const res = await loadDesynth('/data/Desynthesis.json', fetch)
    await mappingPromise;

    return res;
};