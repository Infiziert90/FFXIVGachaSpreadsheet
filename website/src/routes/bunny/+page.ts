import type { PageLoad } from './$types';
import {loadCoffer} from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ parent, fetch }) => {
    let mappingPromise = parent();

    const res = await loadCoffer('/data/EurekaBunnies.json', fetch)
    await mappingPromise;

    return res;
};