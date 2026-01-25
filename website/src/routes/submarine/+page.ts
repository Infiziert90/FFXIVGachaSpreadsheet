import type { PageLoad } from './$types';
import {loadMapping, loadSubmarines} from "$lib/loadHelpers";
import {InitializeHelpers} from "$lib/sheets/sheetHelper";
import {LoadSubExplorationSheet, LoadSubMapSheet} from "$lib/sheets/simplifiedSheets";

export const load: PageLoad = async ({ fetch }) => {
    let dataPromise = loadSubmarines('/data/Submarines.json', fetch)
    let mappingPromise = loadMapping(fetch);
    let mapPromise = LoadSubMapSheet(fetch);
    let explorationPromise = LoadSubExplorationSheet(fetch);

    let res = await Promise
        .all([dataPromise, mappingPromise, mapPromise, explorationPromise])
        .then((data) => data[0]);

    InitializeHelpers();

    return res;
};