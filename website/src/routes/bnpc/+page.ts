import type { PageLoad } from './$types';
import {loadBnpc} from "$lib/loadHelpers";
import {InitializeMapSheets} from "$lib/sheets";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    await InitializeMapSheets(fetch);
    return await loadBnpc('/data/BnpcPairs.json', fetch);
};