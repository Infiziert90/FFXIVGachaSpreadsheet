import {error} from "@sveltejs/kit";

const HEADERS = new Headers({
    'Content-Type': 'application/json;charset=UTF-8',
    "User-Agent": "XIVStats"
});

export const HousingMaps: Record<number, boolean> = {
    72: true,
    192: false,

    82: true,
    193: false,

    83: true,
    194: false,

    364: true,
    365: false,

    679: true,
    680: false,
}

const GameTora: string = 'https://gametora.com/ffxiv/housing-plot-viewer/'
const GameToraMaps: Record<number, string> = {
    72: 'mist',
    192: 'mist',
    82: 'lavender-beds',
    193: 'lavender-beds',
    83: 'goblet',
    194: 'goblet',
    364: 'shirogane',
    365: 'shirogane',
    679: 'empyreum',
    680: 'empyreum'
}

export async function getLastUpdate(browser: boolean): Promise<string> {
    if (!browser) return 'Unknown';

    let data = await fetch('/data/LastUpdate.json', {
        method: 'GET',
        headers: HEADERS
    })
    .then(responseHandler)
    .then((data: string) => {
        return data;
    })
    .catch(errorHandling);

    return data ?? 'Unknown';
}

export function responseHandler(response: Response) {
    if (response.ok) {
        response.clone().bytes().then(t => console.log(toHexString(t.slice(0, 500))));
        return response.json();
    }

    throw new Error(`Response status didn't indicate success. Status: ${response.status} | Message: ${response.statusText}`);
}

function toHexString(byteArray) {
    return Array.from(byteArray, function(byte) {
        return ('0' + (byte & 0xFF).toString(16)).slice(-2);
    }).join('')
}

export function errorHandling(response: any): never {
    logAndThrow(`Unable to fetch resource.`, response);
}

/**
 * Logs the error to the console and throws a 500 error.
 * @param message - A general info message to display
 * @param err - The exception
 */
export function logAndThrow(message: string, err: unknown): never {
    console.error(message, err);
    error(500, {message: `${message} | Error: ${JSON.stringify(err)}`});
}

/**
 * String formatter that changes the first letter of each word to uppercase.
 * @param s - String to format
 * @param article - if an article is used
 */
export function UpperCaseStr(s: string, article: number = 0)
{
    if (article === 1)
        return s;

    const chars = s.split('');
    let lastSpace = true;

    for (let i = 0; i < chars.length; ++i) {
        if (chars[i] === ' ') {
            lastSpace = true;
        } else if (lastSpace) {
            lastSpace = false;
            chars[i] = chars[i].toUpperCase();
        }
    }

    return chars.join('');
}

/**
 * Converts an item name to a wiki url.
 * @param itemName - The name of the item in english 
 * @returns The wiki url
 */
export function getWikiUrl(itemName: string): string {
    return `https://ffxiv.consolegameswiki.com/wiki/${itemName.replace(/\s+/g, '_')}`;
}

/**
 * Generate a valid GameTora url for the house review and pictures
 * @param map - The map id
 * @param plot - The plot number
 */
export function getReviewUrl(map: number, plot: number): string {
    // sub division plots
    if (plot > 29)
        plot -= 30;

    return `${GameTora}${GameToraMaps[map]}?plot=${plot+1}`
}

/**
 * Converts an icon id to a file path for XIVAPI.
 * @param iconId - The icon id
 * @param hq - Whether to use the high resolution icon
 * @returns The file path
 */
export function getIconPath(iconId: number | string, hq: boolean = true): string {
    const suffix = hq ? '_hr1' : '';
    return `https://v2.xivapi.com/api/asset?path=ui/icon/${iconId}${suffix}.tex&format=png`;
}

/**
 * Returns a valid formatting for icon Ids.
 * @param iconId - The icon id to format
 * @returns The formatted number
 */
export function getFormattedIconId(iconId: number): string {
    let iconGroup = iconId - (iconId % 1000);
    return `${pad(iconGroup, 6)}/${pad(iconId, 6)}`;
}

/**
 * Pads a number with leading zeros to a specific size.
 * @param num - The number to pad
 * @param size - The size to pad to
 * @returns The padded number as a string
 */
export function pad(num: number, size: number): string {
    let numStr: string = num.toString().padStart(size, '0');
    // while (numStr.length < size)
    //     numStr = "0" + numStr;

    return numStr;
}

export function getUniqueHash(obj: number[]): number
{
    let hash = 19;
    for (const element of obj.toSorted((a, b) => a - b)) {
        hash = (hash * 31) + element;
    }

    return hash;
}

export function getDuration(totalSeconds: number): string {
    let hours = Math.floor(totalSeconds / 3600);
    totalSeconds %= 3600;
    let minutes = Math.floor(totalSeconds / 60);
    let seconds = totalSeconds % 60;

    return `${pad(hours, 2)}:${pad(minutes, 2)}:${pad(seconds, 2)}`;
}
