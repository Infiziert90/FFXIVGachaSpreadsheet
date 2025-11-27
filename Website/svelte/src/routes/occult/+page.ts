import type { PageLoad } from './$types';
import {responseHandler} from "$lib/utils";
import type {Coffer} from "$lib/interfaces";
import {error} from "@sveltejs/kit";

// @ts-ignore
export const load: PageLoad = async ({ parent, fetch }) => {
    let mappingPromise = parent();

    try {
        const res = await fetch(`/data/OccultTreasures.json`)
            .then(responseHandler)
            .then((data: Coffer[]) => {
                return data;
            });

        if (!res) {
            throw error(500, {message: 'Failed to load occult data set.'});
        }

        await mappingPromise;
        return {content: res};
    } catch (err) {
        console.error('Error loading occult data:', err);
        throw error(500, {message: 'Failed to load occult data set.'});
    }
};