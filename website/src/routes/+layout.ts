import type { PageLoad } from "../../.svelte-kit/types/src/routes/occult/$types";
import {type ItemInfo, Mappings} from "$lib/mappings";
import {errorHandling, logAndThrow, responseHandler} from "$lib/utils";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    try {
        if (Object.keys(Mappings).length > 0) return;

        await fetch('/data/Mappings.json')
            .then(responseHandler)
            .then((data: Record<number, ItemInfo>) =>{
                for (const [key, value] of Object.entries(data)) {
                    Mappings[parseInt(key)] = value;
                }
            })
            .catch(errorHandling);
    } catch (err) {
        logAndThrow('Error loading mapping data.', err)
    }
}