import type {SubExplorationRow} from "$lib/sheets/structure/subExploration";
import {SimpleSubExplorationSheet, SimpleSubMapSheet} from "$lib/sheets/simplifiedSheets";

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
}