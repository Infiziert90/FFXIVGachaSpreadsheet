import type { PageLoad } from './$types';
import {loadDesynth, loadDesynthesisBase, loadMapping} from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);

    const res = await loadDesynthesisBase('/data/desynthesis/base.json', fetch)
    await mappingPromise;

    return res;
};