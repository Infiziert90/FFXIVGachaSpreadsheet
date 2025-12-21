import type { PageLoad } from './$types';
import {loadCoffer, loadMapping} from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);

    const res = await loadCoffer('/data/DeepDungeonSacks.json', fetch)
    await mappingPromise;

    return res;
};