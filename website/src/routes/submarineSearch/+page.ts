import type { PageLoad } from './$types';
import {loadSubmarines} from "$lib/loadHelpers";
import {InitializeHelpers} from "$lib/sheets/sheetHelper";
import {LoadSubExplorationSheet, LoadSubMapSheet} from "$lib/sheets/simplifiedSheets";
import {loadItemMapping} from "$lib/mappings";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let dataPromise = loadSubmarines('/data/Submarines.json', fetch)
    let mappingPromise = loadItemMapping(fetch);
    let mapPromise = LoadSubMapSheet(fetch);
    let explorationPromise = LoadSubExplorationSheet(fetch);

    let res = await Promise
        .all([dataPromise, mappingPromise, mapPromise, explorationPromise])
        .then((data) => data[0]);

    InitializeHelpers();

    return res;
};