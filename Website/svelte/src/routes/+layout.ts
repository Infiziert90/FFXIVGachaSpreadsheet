import type {PageLoad} from "../../.svelte-kit/types/src/routes/occult/$types";
import {loadMapping} from "$lib/loadHelpers";

export const ssr = false

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    await loadMapping(fetch)
}