import type { PageLoad } from './$types';
import {errorHandling, responseHandler} from "$lib/utils";
import type {Coffer} from "$lib/interfaces";
import {error} from "@sveltejs/kit";

export const load: PageLoad = async ({ params }) => {
    const res = await fetch(`data/Occult.json`)
        .then(responseHandler)
        .then((data: Coffer[]) => {
            return data;
        })
        .catch(errorHandling);

    if (!res) {
        error(500, {message: 'Failed to load occult data set.'});
    }

    console.log(res);
    return {content: res};
};