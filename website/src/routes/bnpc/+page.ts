import type { PageLoad } from './$types';
import { loadBnpc } from "$lib/loadHelpers";
import {LoadBNpcNameSheet, LoadMapMarkerSheet, LoadMapSheet, LoadTerritorySheet} from "$lib/sheets/simplifiedSheets";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let dataPromise = loadBnpc('/data/BnpcPairsWeb.json', fetch);
    let mapPromise = LoadMapSheet(fetch);
    let mapMarkerPromise = LoadMapMarkerSheet(fetch);
    let bNpcPromise = LoadBNpcNameSheet(fetch);
    let territoryPromise = LoadTerritorySheet(fetch);

    return await Promise
        .all([dataPromise, mapPromise, mapMarkerPromise, territoryPromise, bNpcPromise])
        .then((data) => data[0]);
};