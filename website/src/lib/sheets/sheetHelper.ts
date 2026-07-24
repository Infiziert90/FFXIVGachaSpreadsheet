import {SimpleSubExplorationSheet, SimpleSubMapSheet, SimpleSubRankSheet} from "$lib/sheets/simplifiedSheets";
import type {SubExplorationRow} from "$lib/sheets/structure/submarines/subExploration";
import type {SubRankRow} from "$lib/sheets/structure/submarines/subRank";

export const LastRank: SubRankRow = {RowId: 0, Capacity: 0, ExpToNext: 0, FavorBonus: 0, RangeBonus: 0, RetrievalBonus: 0, SpeedBonus: 0, SurveillanceBonus: 0};
export const MapNames: string[] = [];
export const ReversedMaps: number[] = [];
export const MapToStartSector: Record<number, SubExplorationRow> = [];

export function InitializeHelpers() {
    for (const sector of Object.values(SimpleSubExplorationSheet)) {
        if (!sector.StartingPoint)
            continue;

        ReversedMaps.push(sector.RowId);
        MapToStartSector[sector.Map] = sector;
    }
    ReversedMaps.reverse()

    for (const map of Object.values(SimpleSubMapSheet).filter(m => m.RowId > 0)) {
        MapNames.push(map.Name);
    }

    let last = Object.values(SimpleSubRankSheet).filter(s => s.Capacity !== 0).map(s => s.RowId).toSorted((a, b) => a - b).at(-1);
    if (last === undefined)
        return;

    let lastRank = SimpleSubRankSheet[last];
    LastRank.RowId = lastRank.RowId;
    LastRank.Capacity = lastRank.Capacity;
    LastRank.ExpToNext = lastRank.ExpToNext;
    LastRank.FavorBonus = lastRank.FavorBonus;
    LastRank.RangeBonus = lastRank.RangeBonus;
    LastRank.SpeedBonus = lastRank.SpeedBonus;
    LastRank.SurveillanceBonus = lastRank.SurveillanceBonus;
    LastRank.RetrievalBonus = lastRank.RetrievalBonus;
}