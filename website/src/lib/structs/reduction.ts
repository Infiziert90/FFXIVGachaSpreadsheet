import type {Reward} from "$lib/interfaces";

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

    LowestSand: number;
    LowestBonus: number;

    Tiers: ReductionTier[];
}

interface ReductionTier {
    Records: number;

    Tier: number;
    Minimum: number;

    Patches: Record<string, ReductionPatch>;
}

interface ReductionPatch {
    NormalCount: number;
    BonusCount: number;

    Normal: Reward[];
    Bonus: Reward[];
}