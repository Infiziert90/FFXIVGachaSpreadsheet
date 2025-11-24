import type { PageLoad } from './$types';
import { responseHandler } from "$lib/utils";
import type { DesynthBase } from "$lib/interfaces";
import { error } from "@sveltejs/kit";

// @ts-ignore
export const load: PageLoad = async ({ parent, fetch }) => {
    await parent();

    try {
        const res = await fetch(`/data/DesynthesisData.json`)
            .then(responseHandler)
            .then((data: DesynthBase) => {
                return data;
            });

        if (!res) {
            throw error(500, { message: 'Failed to load desynthesis data set.' });
        }

        return { content: res };
    } catch (err) {
        console.error('Error loading desynthesis data:', err);
        throw error(500, { message: 'Failed to load desynthesis data set.' });
    }
};