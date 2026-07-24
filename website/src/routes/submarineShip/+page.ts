import type { PageLoad } from './$types';
import {loadMapping} from "$lib/loadHelpers";
import {InitializeHelpers} from "$lib/sheets/sheetHelper";
import {
    LoadSubExplorationSheet,
    LoadSubMapSheet,
    LoadSubPartSheet,
    LoadSubRankSheet
} from "$lib/sheets/simplifiedSheets";
import {importCalculatedData} from "$lib/submarines/calculated";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    console.log(`Loading all required data`)
    let mappingPromise = loadMapping(fetch);
    let mapPromise = LoadSubMapSheet(fetch);
    let rankPromise = LoadSubRankSheet(fetch);
    let partPromise = LoadSubPartSheet(fetch);
    let explorationPromise = LoadSubExplorationSheet(fetch);
    let calculatedDataPromise = importCalculatedData(fetch);

    await Promise.all([mappingPromise, mapPromise, rankPromise, partPromise, explorationPromise, calculatedDataPromise]);
    InitializeHelpers();
};