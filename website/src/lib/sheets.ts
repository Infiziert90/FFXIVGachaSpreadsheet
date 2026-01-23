import type {SubmarineExploration} from "$lib/sheets/submarineExploration";
import {ReadMapSheet, type SubmarineMap} from "$lib/sheets/submarineMap";
import {logAndThrow, responseHandler} from "$lib/utils";
import type {TerritoryType} from "$lib/sheets/territoryType";
import type {ExcelMap} from "$lib/sheets/map";

export const XIVAPIURL: string = 'https://v2.xivapi.com/api/sheet/';
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
export const TerritoryTypeSheet: Record<number, TerritoryType> = {};
export const MapSheet: Record<number, ExcelMap> = {};
export const BNpcNameSheet: Record<number, string> = {};

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

export async function InitializeMapSheets(fetch: any) {
    try {
        if (Object.keys(TerritoryTypeSheet).length > 0) return;

        console.log("Initialize map related sheets");

        // await fetch(XIVAPIURL + 'Map?fields=Id,SizeFactor,OffsetX,OffsetY&limit=1500', {
        // await fetch(XIVAPIURL + 'TerritoryType?fields=Map@as(raw),PlaceName.Name,PlaceNameRegion.Name,PlaceNameZone.Name&limit=1500', {
        await fetch('/dev/TerritoryType.json', {
            method: 'GET',
            headers: HEADERS
        })
        .then(responseHandler)
        .then((data: SheetResponse) => {
            for (const row of data.rows) {
                TerritoryTypeSheet[row.row_id] = {
                    RowId: row.row_id,

                    Map: row.fields['Map@as(raw)'],
                    PlaceName: row.fields.PlaceName.fields.Name,
                    PlaceNameZone: row.fields.PlaceNameZone.fields.Name,
                    PlaceNameRegion: row.fields.PlaceNameRegion.fields.Name,
                }
            }
        })
        .catch((err) => {
            logAndThrow('Error loading territory type data.', err);
        });

        await fetch('/dev/Map.json', {
            method: 'GET',
            headers: HEADERS
        })
        .then(responseHandler)
        .then((data: SheetResponse) => {
            for (const row of data.rows) {
                MapSheet[row.row_id] = {
                    RowId: row.row_id,

                    Id: row.fields.Id,
                    OffsetX: row.fields.OffsetX,
                    OffsetY: row.fields.OffsetY,
                    SizeFactor: row.fields.SizeFactor,
                }
            }
        })
        .catch((err) => {
            logAndThrow('Error loading map data.', err);
        });

        await fetch('/dev/Names.json', {
            method: 'GET',
            headers: HEADERS
        })
        .then(responseHandler)
        .then((data: Record<number, string>) => {
            for (const [id, name] of Object.entries(data)) {
                BNpcNameSheet[parseInt(id)] = name;
            }
        })
        .catch((err) => {
            logAndThrow('Error loading map data.', err);
        });
    } catch (err) {
        logAndThrow('Error loading sheet data.', err)
    }
}