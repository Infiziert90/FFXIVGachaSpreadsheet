import type { PageLoad } from './$types';
import {loadMapping, loadSubmarines, loadVentures} from "$lib/loadHelpers";
import {InitializeSheets} from "$lib/sheets";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mappingPromise = loadMapping(fetch);

    const res = await loadSubmarines('/data/Submarines.json', fetch)
    await mappingPromise;
    await InitializeSheets();

    return res;
};