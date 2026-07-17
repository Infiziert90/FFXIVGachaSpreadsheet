// From: Dalamuds MapUtil

import type {Vector3} from "$lib/math/vector3";
import {SimpleMapSheet} from "$lib/sheets/simplifiedSheets";
import type {MapMarkerRow} from "$lib/sheets/structure/mapMarker";

export interface SimpleCoords {
    X: number;
    Y: number;
}

/**
 * A simple swap going from leaflet lat, lot to FFXIV map X,Y
 * @param coords - Coordinates from leaflet
 * @return The swapped coordinates
 */
export function swapCoords(coords: SimpleCoords): SimpleCoords {
    // noinspection JSSuspiciousNameCombination
    return {X: coords.Y, Y: coords.X};
}

export function convertSheetToMapCoord(mapMarker: MapMarkerRow, sizeFactor: number): SimpleCoords {
    let x = mapMarker.X * 40.96 / 2048 / sizeFactor * 100 + 1;
    let y = mapMarker.Y * 40.96 / 2048 / sizeFactor * 100 + 1;
    return {X: x, Y: y};
}

/**
 * Converts FFXIV world position to user facing map coordinates.
 * @param value - X or Z world position
 * @param scale - The maps scale value
 * @param offset - The maps offset value
 * @return X or Y map coordinate
 */
function convertWorldCoordXZToMapCoord(value: number, scale: number, offset: number): number {
    return (0.02 * offset) + (2048 / scale) + (0.02 * value) + 1.0;
}

/**
 * Converts FFXIV world position to user facing map coordinates.
 * @param worldCoordinates - A vector3 with XYZ world position, Y will always be ignored
 * @param xOffset - The maps X offset value
 * @param yOffset - The maps Y offset value
 * @param scale - The maps scale
 * @return XY map coordinates, this should be formatted to only use 2 decimal places later
 */
function worldToMap(worldCoordinates: Vector3, xOffset: number = 0, yOffset: number = 0, scale: number = 100): SimpleCoords {
    return {
        X: convertWorldCoordXZToMapCoord(worldCoordinates.X, scale, xOffset),
        Y: convertWorldCoordXZToMapCoord(worldCoordinates.Z, scale, yOffset)
    };
}

/**
 * A helper for `worldToMap`, picks the correct map values from the Map sheet.
 * @param worldCoordinates - A vector3 with XYZ world position, Y will always be ignored
 * @param map - The map id
 */
export function convertToMapCoords(worldCoordinates: Vector3, map: number): SimpleCoords {
    let mapRow = SimpleMapSheet[map];

    return worldToMap(worldCoordinates, mapRow.OffsetX, mapRow.OffsetY, mapRow.SizeFactor);
}

/**
 * Converts the map size factor to its correct bounds.
 * @param map - The map id
 * @return The bounds for this map
 */
export function convertSizeFactorToMapMaxCoord(map: number): number {
    let mapRow = SimpleMapSheet[map];

    return 1 + 4096 / mapRow.SizeFactor; // 1 + 2048 / 50 / (mapRow.SizeFactor / 100)
}