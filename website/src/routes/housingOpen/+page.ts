import type { PageLoad } from './$types';
import {
    LoadHousingLandSetSheet,
    LoadHousingMapMarkerSheet, LoadMapMarkerSheet,
    LoadMapSheet,
    LoadTerritorySheet, LoadWorldDCGroupSheet, LoadWorldSheet
} from "$lib/sheets/simplifiedSheets";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    let mapPromise = LoadMapSheet(fetch);
    let mapMarkerPromise = LoadMapMarkerSheet(fetch);
    let housingLandPromise = LoadHousingLandSetSheet(fetch);
    let housingMarkerPromise = LoadHousingMapMarkerSheet(fetch);
    let territoryPromise = LoadTerritorySheet(fetch);
    let worldPromise = LoadWorldSheet(fetch);
    let worldDCGroupPromise = LoadWorldDCGroupSheet(fetch);

    await Promise.all([mapPromise, mapMarkerPromise, housingLandPromise, housingMarkerPromise, territoryPromise, worldPromise, worldDCGroupPromise]);
};