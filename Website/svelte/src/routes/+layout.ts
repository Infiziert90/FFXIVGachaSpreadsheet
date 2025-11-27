import {loadMappings} from "$lib/data";
import type {PageLoad} from "../../.svelte-kit/types/src/routes/occult/$types";

export const ssr = false

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    // Load mapping data
    await loadMappings(fetch)
}