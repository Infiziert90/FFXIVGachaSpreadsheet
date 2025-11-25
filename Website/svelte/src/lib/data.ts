import {errorHandling, responseHandler} from "$lib/utils";

export interface ItemInfo {
    Name: string;
    Icon: string;
}

export const Mappings: Record<number, ItemInfo> = {};

export async function loadMappings(fetch: any) {
    if (Object.keys(Mappings).length > 0) return;

    await fetch('data/Mappings.json')
        .then(responseHandler)
        .then((data: Record<number, ItemInfo>) =>{
            for (const [key, value] of Object.entries(data)) {
                Mappings[parseInt(key)] = value;
            }
        })
        .catch(errorHandling);

    console.log(Mappings);
}