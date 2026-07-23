import {SimpleSubExplorationSheet} from "$lib/sheets/simplifiedSheets";
import {ToSectorName} from "$lib/sheets/structure/subExploration";
import {ReversedMaps} from "$lib/sheets/sheetHelper";

/**
 * Gets the full sector name from a sector number.
 * Format: SectorName (Letter - Map Short)
 * @param sector - A valid sector
 */
export function getFullSectorName(sector: number): string {
    if (!SimpleSubExplorationSheet.hasOwnProperty(sector)) {
        return `Unknown sector: ${sector}`;
    }

    let subRow = SimpleSubExplorationSheet[sector];
    return `${ToSectorName(subRow).split('(')[0]} (${numToLetter(subRow.RowId, true)} - ${mapToThreeLetter(subRow.RowId, true)})`
}

const Letters: string = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
/**
 * Convert a sector to its letter.
 * @param num - A valid sector number
 * @param findStart - True if the number has not been adjusted for its start.
 */
export function numToLetter(num: number, findStart: boolean): string {
    if (findStart) {
        let start = findVoyageStart(num);
        if (start === undefined)
            return 'Unknown';

        num -= start;
    }

    let index = num - 1;  // 0 indexed

    let value = '';
    if (index >= Letters.length)
        value += Letters[(index / Letters.length) - 1];

    value += Letters[index % Letters.length];

    return value;
}

/**
 * Convert map or sector to the map letters.
 * @param key - A valid map or sector number
 * @param sectorToMap - Convert sector to the map number
 */
export function mapToThreeLetter(key: number, sectorToMap: boolean = false): string
{
    if (sectorToMap)
        key = SimpleSubExplorationSheet[key].Map;

    switch (key) {
        case 1: return 'DSS';
        case 2: return 'SOA';
        case 3: return 'SOJ';
        case 4: return 'SSS';
        case 5: return 'TLS';
        case 6: return 'SID';
        case 7: return 'TNE';
        default: return '';
    }
}

/**
 * Find the map number from any sector.
 * @param sector - A valid sector number
 */
export function findMapFromSector(sector: number): number {
    return SimpleSubExplorationSheet[sector].Map;
}

/**
 * Find the voyage start sector from any sector number in between
 * @param sector - A valid sector number
 * @return Undefined if invalid.
 */
export function findVoyageStart(sector: number): number | undefined {
    // This works because we reversed the list of start points
    return ReversedMaps.find(m => sector >= m);
}

/**
 * Convert the tier string into its number component.
 * @param tier - Tier string from the submarine data json
 */
export function getTierAsNumber(tier: string): number {
    switch (tier) {
        case 'Tier1': return 1;
        case 'Tier2': return 2;
        case 'Tier3': return 3;
        default: return 0;
    }
}