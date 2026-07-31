import {logAndThrow, responseCompressedHandler} from "$lib/utils";

type Fetch = typeof fetch;

export interface Localized {
    En: string;
    Fr: string;
    De: string;
    Ja: string;
}

export interface ItemEntry extends Localized {
    Icon: string;
}

export const ItemMappings: Record<number, ItemEntry> = {};

export async function loadItemMapping(fetch: Fetch) {
    try {
        if (Object.keys(ItemMappings).length > 0) return;

        const res: Record<number, ItemEntry> = await fetch('/website/mappings/Items.json.gz')
            .then(responseCompressedHandler)
            .then((data: Record<number, ItemEntry>) =>{
                return data;
            });

        if (!res) {
            throw new Error(`item mapping resource is invalid.`);
        }

        for (const [key, value] of Object.entries(res)) {
            ItemMappings[parseInt(key)] = value;
        }
    } catch (err) {
        logAndThrow('Error loading item mapping data.', err)
    }
}