// From: Dalamuds MapUtil

import type {Vector3} from "$lib/math/vector3";
import {SimpleMapSheet} from "$lib/sheets/simplifiedSheets";
import type {MapMarkerRow} from "$lib/sheets/structure/mapMarker";

export interface SimpleCoords {
    X: number;
    Y: number;
}

export function swapCoords(coords: SimpleCoords): SimpleCoords {
    // noinspection JSSuspiciousNameCombination
    return {X: coords.Y, Y: coords.X};
}

export function convertSheetToMapCoord(mapMarker: MapMarkerRow, sizeFactor: number): SimpleCoords {
    let x = mapMarker.X * 40.96 / 2048 / sizeFactor * 100 + 1;
    let y = mapMarker.Y * 40.96 / 2048 / sizeFactor * 100 + 1;
    return {X: x, Y: y};
}

function convertWorldCoordXZToMapCoord(value: number, scale: number, offset: number): number {
    return (0.02 * offset) + (2048 / scale) + (0.02 * value) + 1.0;
}

function worldToMap(worldCoordinates: Vector3, xOffset: number = 0, yOffset: number = 0, scale: number = 100): SimpleCoords {
    return {
        X: convertWorldCoordXZToMapCoord(worldCoordinates.X, scale, xOffset),
        Y: convertWorldCoordXZToMapCoord(worldCoordinates.Z, scale, yOffset)
    };
}

export function convertToMapCoords(worldCoordinates: Vector3, map: number): SimpleCoords {
    let mapRow = SimpleMapSheet[map];

    return worldToMap(worldCoordinates, mapRow.OffsetX, mapRow.OffsetY, mapRow.SizeFactor);
}

export function convertSizeFactorToMapMaxCoord(map: number): number {
    let mapRow = SimpleMapSheet[map];

    return 1 + 4096 / mapRow.SizeFactor; // 1 + 2048 / 50 / (mapRow.SizeFactor / 100)
}