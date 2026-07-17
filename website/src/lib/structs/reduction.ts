import type {Reward} from "$lib/structs/reward";

export interface Reduction {
    Records: number;

    Jobs: ReductionJob[];
}

export interface ReductionJob {
    Records: number;

    Id: number;
    Name: string;

    Sources: ReductionSource[];
}

export interface ReductionSource {
    Records: number;

    Id: number;

    MainTier: number;
    Maximum: number;

    LowestSand: number;
    LowestBonus: number;

    Tiers: ReductionTier[];
}

export interface ReductionTier {
    Records: number;

    SubTier: number;

    Patches: Record<string, ReductionPatch>;
}

interface ReductionPatch {
    NormalCount: number;
    BonusCount: number;

    Normal: Reward[];
    Bonus: Reward[];
}