import type {Coffer, CofferVariant} from "$lib/interfaces";

interface CofferSelection {
    coffer: Coffer;
    variant: CofferVariant;
}

/**
 * Try to get the specific coffer and variant.
 * @param data - Dictionary to search through
 * @param territoryId - The territory ID to resolve
 * @param cofferId - The coffer variant ID to resolve
 * @returns Venture selection if successful, undefined otherwise.
 */
export function tryGetCoffer(data: Coffer[], territoryId: number, cofferId: number): CofferSelection | undefined {
    // Find the coffer data for the selected territory
    const coffer = data.find((e) => e.TerritoryId === territoryId);
    if (!coffer) return undefined;

    // Find the specific coffer variant
    const variant = coffer.Variants.find((e) => e.Id === cofferId);
    if (!variant) return undefined;

    return { coffer, variant };
}