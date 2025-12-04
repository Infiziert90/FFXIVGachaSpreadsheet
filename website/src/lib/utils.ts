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

    return Promise.reject(response);
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