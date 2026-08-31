import type {Reward} from "$lib/structs/reward";

export interface FateReward {
    Expansions: FateExpansion[];
}

export interface FateExpansion {
    Id: number;
    Records: number;

    Territories: FateTerritory[];
}

export interface FateTerritory {
    Id: number;
    Records: number;

    FateTypes: FateType[];
}

export interface FateType {
    Id: number;
    Records: number;

    Fates: Fate[];
}

export interface Fate {
    Id: number;
    Records: number;

    Rewards: Reward[];
}