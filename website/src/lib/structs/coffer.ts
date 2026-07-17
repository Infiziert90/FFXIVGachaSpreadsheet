import type {Reward} from "$lib/structs/reward";

export interface Coffer {
    Name: string;
    TerritoryId: number;

    Variants: CofferVariant[];
}

export interface CofferVariant {
    Id: number;
    Name: string;

    Patches: Record<string, CofferContent>;
}

export interface CofferContent {
    Total: number;
    Items: Reward[];
}