import type { PageLoad } from "../../.svelte-kit/types/src/routes/occult/$types";
import { loadMapping } from "$lib/loadHelpers";

// @ts-ignore
export const load: PageLoad = async ({ fetch }) => {
    await loadMapping(fetch)
}