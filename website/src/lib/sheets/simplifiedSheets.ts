import type {MapRow} from "$lib/sheets/structure/map";
import type {TerritoryRow} from "$lib/sheets/structure/territory";
import type {SubMapRow} from "$lib/sheets/structure/subMap";
import type {SubExplorationRow} from "$lib/sheets/structure/subExploration";
import {logAndThrow, responseHandler} from "$lib/utils";

type Fetch = typeof fetch;

export const SimpleMapSheet: Record<number, MapRow> = {};
export const SimpleTerritorySheet: Record<number, TerritoryRow> = {};

export const SimpleSubMapSheet: Record<number, SubMapRow> = {};
export const SimpleSubExplorationSheet: Record<number, SubExplorationRow> = {};

export const SimpleBNpcNameSheet: Record<number, string> = {};

export async function LoadMapSheet(fetch: Fetch) {
    if (Object.keys(SimpleMapSheet).length > 0)
        return;

    return await fetch('/sheets/map.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, MapRow>) => {
            for (const row of Object.values(data))
                SimpleMapSheet[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading map data.', err);
        });
}

export async function LoadTerritorySheet(fetch: Fetch) {
    if (Object.keys(SimpleTerritorySheet).length > 0)
        return;

    return await fetch('/sheets/territory.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, TerritoryRow>) => {
            for (const row of Object.values(data))
                SimpleTerritorySheet[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading territory data.', err);
        });
}

export async function LoadSubMapSheet(fetch: Fetch) {
    if (Object.keys(SimpleSubMapSheet).length > 0)
        return;

    return await fetch('/sheets/subMap.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, SubMapRow>) => {
            for (const row of Object.values(data))
                SimpleSubMapSheet[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading sub map data.', err);
        });
}

export async function LoadSubExplorationSheet(fetch: Fetch) {
    if (Object.keys(SimpleSubExplorationSheet).length > 0)
        return;

    return await fetch('/sheets/subExploration.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, SubExplorationRow>) => {
            for (const row of Object.values(data))
                SimpleSubExplorationSheet[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading sub exploration data.', err);
        });
}

export async function LoadBNpcNameSheet(fetch: Fetch) {
    if (Object.keys(SimpleBNpcNameSheet).length > 0)
        return;

    return await fetch('/sheets/bNpcNames.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, string>) => {
            for (const [id, name] of Object.entries(data))
                SimpleBNpcNameSheet[parseInt(id)] = name;
        })
        .catch((err) => {
            logAndThrow('Error loading bNpc name data.', err);
        });
}

