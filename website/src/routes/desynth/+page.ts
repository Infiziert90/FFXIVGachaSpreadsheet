import type { PageLoad } from './$types';
import {loadDesynth} from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ parent, fetch }) => {
    let mappingPromise = parent();

    const res = await loadDesynth('/data/Desynthesis.json', fetch)
    await mappingPromise;

    return res;
};