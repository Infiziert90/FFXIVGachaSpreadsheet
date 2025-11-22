import {errorHandling, responseHandler} from "$lib/utils";
import {error} from "@sveltejs/kit";

export const IconPaths: Record<string, string> = {};

export async function loadIcons(fetch: any) {
    if (IconPaths.length > 0) return;

    await fetch('data/IconPaths.json')
        .then(responseHandler)
        .then((data: { [id: string ]: string }) =>{
            for (const [key, value] of Object.entries(data)) {
                IconPaths[key] = value;
            }
        })
        .catch(errorHandling);

    console.log(IconPaths);
}