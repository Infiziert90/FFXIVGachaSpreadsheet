import type { PageLoad } from './$types';
import { loadBnpc } from "$lib/loadHelpers";
import { LoadBNpcNameSheet, LoadMapSheet, LoadTerritorySheet } from "$lib/sheets/simplifiedSheets";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let dataPromise = loadBnpc('/data/BnpcPairs.json', fetch);
    let mapPromise = LoadMapSheet(fetch);
    let bNpcPromise = LoadBNpcNameSheet(fetch);
    let territoryPromise = LoadTerritorySheet(fetch);

    return await Promise
        .all([dataPromise, mapPromise, territoryPromise, bNpcPromise])
        .then((data) => data[0]);
};