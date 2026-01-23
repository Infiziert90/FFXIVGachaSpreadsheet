import {error} from "@sveltejs/kit";


const HEADERS = new Headers({
    'Content-Type': 'application/json;charset=UTF-8',
    "User-Agent": "FFXIV Gacha"
});

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
        return response.json();
    }

    throw new Error(`Response status didn't indicate success. Status: ${response.status} | Message: ${response.statusText}`);
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
 * Converts an icon id to a file path for XIVAPI.
 * @param iconId - The icon id
 * @param hq - Whether to use the high resolution icon
 * @returns The file path
 */
export function getIconPath(iconId: number | string, hq: boolean = false): string {
    const suffix = hq ? '_hr1' : '';
    return `https://v2.xivapi.com/api/asset?path=ui/icon/${iconId}${suffix}.tex&format=png`;
}

export function getFormattedIconId(iconId: number): string {
    let iconGroup = iconId - (iconId % 1000);
    return `${pad(iconGroup, 6)}/${pad(iconId, 6)}`;
}

function pad(num: number, size: number): string {
    let numStr: string = num.toString();
    while (numStr.length < size) numStr = "0" + numStr;
    return numStr;
}