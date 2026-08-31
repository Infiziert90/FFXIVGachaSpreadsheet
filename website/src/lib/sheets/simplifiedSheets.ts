import type {MapRow} from "$lib/sheets/structure/map";
import type {TerritoryRow} from "$lib/sheets/structure/territory";
import type {SubExplorationRow} from "$lib/sheets/structure/submarines/subExploration";
import {logAndThrow, responseHandler} from "$lib/utils";
import type {HousingMapMarkerRow} from "$lib/sheets/structure/housingMapMarkerRow";
import type {HousingLandSetRow} from "$lib/sheets/structure/housingLandSet";
import type {WorldRow} from "$lib/sheets/structure/world";
import type {WorldDCGroupRow} from "$lib/sheets/structure/worldDCGroup";
import type {MapMarkerRow} from "$lib/sheets/structure/mapMarker";
import type {ReductionRewardRow} from "$lib/sheets/structure/reductionReward";
import type {JobRow} from "$lib/sheets/structure/job";
import type {SubRankRow} from "$lib/sheets/structure/submarines/subRank";
import type {SubPartRow} from "$lib/sheets/structure/submarines/subPart";
import type {SubMapRow} from "$lib/sheets/structure/submarines/subMap";
import type {ExVersionRow} from "$lib/sheets/structure/exVersion";
import type {FateRow} from "$lib/sheets/structure/fate";
import type {DynamicEventRow} from "$lib/sheets/structure/dynamicEvent";

type Fetch = typeof fetch;

export const SimpleJobSheet: Record<number, JobRow> = {};

export const SimpleMapSheet: Record<number, MapRow> = {};
export const SimpleTerritorySheet: Record<number, TerritoryRow> = {};
export const SimpleExVersion: Record<number, ExVersionRow> = {};

export const SimpleSubMapSheet: Record<number, SubMapRow> = {};
export const SimpleSubRankSheet: Record<number, SubRankRow> = {};
export const SimpleSubPartSheet: Record<number, SubPartRow> = {};
export const SimpleSubExplorationSheet: Record<number, SubExplorationRow> = {};

export const SimpleBNpcNameSheet: Record<number, Record<string, string>> = {};

export const SimpleMapMarker: Record<number, Record<number, MapMarkerRow>> = {};
export const SimpleHousingLandSet: Record<number, HousingLandSetRow> = {};
export const SimpleHousingMapMarker: Record<number, Record<number, HousingMapMarkerRow>> = {};

export const SimpleWorld: Record<number, WorldRow> = {};
export const SimpleWorldDCGroup: Record<number, WorldDCGroupRow> = {};

export const SimpleReductionReward: Record<number, Record<number, ReductionRewardRow>> = {};

export const SimpleFate: Record<number, FateRow> = {};
export const SimpleDynamicEvent: Record<number, DynamicEventRow> = {};

export async function LoadJobSheet(fetch: Fetch) {
    if (Object.keys(SimpleJobSheet).length > 0)
        return;

    return await fetch('/sheets/job.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, JobRow>) => {
            for (const row of Object.values(data))
                SimpleJobSheet[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading job data.', err);
        });
}

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

export async function LoadExVersionSheet(fetch: Fetch) {
    if (Object.keys(SimpleExVersion).length > 0)
        return;

    return await fetch('/sheets/exVersion.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, ExVersionRow>) => {
            for (const row of Object.values(data))
                SimpleExVersion[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading exversion data.', err);
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

export async function LoadSubRankSheet(fetch: Fetch) {
    if (Object.keys(SimpleSubRankSheet).length > 0)
        return;

    return await fetch('/sheets/subRank.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, SubRankRow>) => {
            for (const row of Object.values(data))
                SimpleSubRankSheet[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading sub rank data.', err);
        });
}

export async function LoadSubPartSheet(fetch: Fetch) {
    if (Object.keys(SimpleSubPartSheet).length > 0)
        return;

    return await fetch('/sheets/subPart.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, SubPartRow>) => {
            for (const row of Object.values(data))
                SimpleSubPartSheet[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading sub part data.', err);
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
        .then((data: Record<number, Record<string, string>>) => {
            for (const [id, names] of Object.entries(data))
                SimpleBNpcNameSheet[parseInt(id)] = names;
        })
        .catch((err) => {
            logAndThrow('Error loading bNpc name data.', err);
        });
}

export async function LoadMapMarkerSheet(fetch: Fetch) {
    if (Object.keys(SimpleMapMarker).length > 0)
        return;

    return await fetch('/sheets/mapMarker.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, Record<number, MapMarkerRow>>) => {
            for (const [rowId, subRow] of Object.entries(data))
                SimpleMapMarker[parseInt(rowId)] = subRow;
        })
        .catch((err) => {
            logAndThrow('Error loading map marker data.', err);
        });
}

export async function LoadHousingLandSetSheet(fetch: Fetch) {
    if (Object.keys(SimpleHousingLandSet).length > 0)
        return;

    return await fetch('/sheets/housingLandSet.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, HousingLandSetRow>) => {
            for (const row of Object.values(data))
                SimpleHousingLandSet[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading housing land set data.', err);
        });
}

export async function LoadHousingMapMarkerSheet(fetch: Fetch) {
    if (Object.keys(SimpleHousingMapMarker).length > 0)
        return;

    return await fetch('/sheets/housingMapMarker.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, Record<number, HousingMapMarkerRow>>) => {
            for (const [rowId, subRow] of Object.entries(data))
                SimpleHousingMapMarker[parseInt(rowId)] = subRow;
        })
        .catch((err) => {
            logAndThrow('Error loading housing map marker data.', err);
        });
}

export async function LoadWorldSheet(fetch: Fetch) {
    if (Object.keys(SimpleWorld).length > 0)
        return;

    return await fetch('/sheets/world.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, WorldRow>) => {
            for (const row of Object.values(data))
                SimpleWorld[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading world data.', err);
        });
}

export async function LoadWorldDCGroupSheet(fetch: Fetch) {
    if (Object.keys(SimpleWorldDCGroup).length > 0)
        return;

    return await fetch('/sheets/worldDCGroup.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, WorldDCGroupRow>) => {
            for (const row of Object.values(data))
                SimpleWorldDCGroup[row.RowId] = row;
        })
        .catch((err) => {
            logAndThrow('Error loading world dc group data.', err);
        });
}

export async function LoadReductionRewardSheet(fetch: Fetch) {
    if (Object.keys(SimpleReductionReward).length > 0)
        return;

    return await fetch('/sheets/reductionReward.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, Record<number, ReductionRewardRow>>) => {
            for (const [rowId, subRow] of Object.entries(data))
                SimpleReductionReward[parseInt(rowId)] = subRow;
        })
        .catch((err) => {
            logAndThrow('Error loading reduction reward data.', err);
        });
}

export async function LoadFateSheet(fetch: Fetch) {
    if (Object.keys(SimpleFate).length > 0)
        return;

    return await fetch('/sheets/fate.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, FateRow>) => {
            for (const [rowId, subRow] of Object.entries(data))
                SimpleFate[parseInt(rowId)] = subRow;
        })
        .catch((err) => {
            logAndThrow('Error loading fate data.', err);
        });
}

export async function LoadDynamicEventSheet(fetch: Fetch) {
    if (Object.keys(SimpleDynamicEvent).length > 0)
        return;

    return await fetch('/sheets/dynamicEvent.json', { method: 'GET' })
        .then(responseHandler)
        .then((data: Record<number, DynamicEventRow>) => {
            for (const [rowId, subRow] of Object.entries(data))
                SimpleDynamicEvent[parseInt(rowId)] = subRow;
        })
        .catch((err) => {
            logAndThrow('Error loading dynamic event data.', err);
        });
}
