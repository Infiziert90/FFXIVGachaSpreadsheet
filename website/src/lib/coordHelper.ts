// From: Dalamuds MapUtil

import {MapSheet} from "$lib/sheets";
import type {Vector3} from "$lib/math/vector3";

export interface SimpleCoords {
    X: number;
    Y: number;
}

export function swapCoords(coords: SimpleCoords): SimpleCoords {
    // noinspection JSSuspiciousNameCombination
    return {X: coords.Y, Y: coords.X};
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
    let mapData = MapSheet[map];

    return worldToMap(worldCoordinates, mapData.OffsetX, mapData.OffsetY, mapData.SizeFactor);
}