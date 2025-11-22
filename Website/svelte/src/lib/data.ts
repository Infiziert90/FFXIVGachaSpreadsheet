import {errorHandling, responseHandler} from "$lib/utils";

export const IconPaths: { [id: string ]: string } = {};

export async function loadIcons() {
    await fetch('data/IconPaths.json')
        .then(responseHandler)
        .then((data: { [id: string ]: string }) =>{
            for (const [key, value] of Object.entries(data)) {
                IconPaths[key] = value;
            }
        })
        .catch(errorHandling);
}