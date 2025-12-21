import type {SubmarineExploration} from "$lib/sheets/submarineExploration";
import {ReadMapSheet, type SubmarineMap} from "$lib/sheets/submarineMap";
import {logAndThrow, responseHandler} from "$lib/utils";

const XIVAPIURL: string = 'https://v2.xivapi.com/api/sheet/';
const HEADERS = new Headers({
    'Content-Type': 'application/json;charset=UTF-8',
    "User-Agent": "FFXIV Gacha"
});

interface SheetResponse {
    schema: string;
    rows: SheetRow[];
}

interface SheetRow {
    row_id: number;
    fields: object;
}

export const SubmarineExplorationSheet: Record<number, SubmarineExploration> = {};
export const SubmarineMapSheet: Record<number, SubmarineMap> = {};

export const MapNames: string[] = [];
export const ReversedMaps: number[] = [];
export const MapToStartSector: { [id: number]: SubmarineExploration } = [];

export async function InitializeSheets() {
    try {
        if (Object.keys(SubmarineExplorationSheet).length > 0) return;

        console.log("InitializeSheets");

        await fetch(XIVAPIURL + 'SubmarineExploration?fields=*&limit=1000', {
            method: 'GET',
            headers: HEADERS
        })
        .then(responseHandler)
        .then((data: SheetResponse) => {
            for (const row of data.rows) {
                SubmarineExplorationSheet[row.row_id] = {
                    RowId: row.row_id,

                    Destination: row.fields.Destination,
                    Location: row.fields.Location,
                    ExpReward: row.fields.ExpReward,
                    SurveyDurationmin: row.fields.SurveyDurationmin,
                    X: row.fields.Y,
                    Y: row.fields.Y,
                    Z: row.fields.Z,
                    Map: { RowId: row.fields.Map.row_id, Name: row.fields.Map.fields.Name },
                    Stars: row.fields.Stars,
                    RankReq: row.fields.RankReq,
                    CeruleumTankReq: row.fields.CeruleumTankReq,
                    SurveyDistance: row.fields.SurveyDistance,
                    StartingPoint: row.fields.StartingPoint,
                }
            }
        })
        .catch((err) => {
            logAndThrow('Error loading exploration data.', err);
        });

        await fetch(XIVAPIURL + 'SubmarineMap?fields=*', {
            method: 'GET',
            headers: HEADERS
        })
        .then(responseHandler)
        .then(ReadMapSheet)
        .catch((err) => {
            logAndThrow('Error loading map data.', err);
        });

        for (const sector of Object.values(SubmarineExplorationSheet)) {
            if (!sector.StartingPoint)
                continue;

            ReversedMaps.push(sector.RowId);
            MapToStartSector[sector.Map.RowId] = sector;
        }
        ReversedMaps.reverse()

        for (const map of Object.values(SubmarineMapSheet).filter(m => m.RowId > 0)) {
            MapNames.push(map.Name);
        }
    } catch (err) {
        logAndThrow('Error loading sheet data.', err)
    }
}