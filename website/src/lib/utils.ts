import {error} from "@sveltejs/kit";

const HEADERS = new Headers({
    'Content-Type': 'application/json;charset=UTF-8',
    "User-Agent": "FFXIV Gacha"
});

export async function getLastUpdate(): Promise<string> {
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