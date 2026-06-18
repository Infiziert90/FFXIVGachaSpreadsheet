export interface Reduction {
    Records: number;

    Sources: Record<number, ReductionSource>;
}

export interface ReductionSource {
    Records: number;
    LowestSand: number;
    LowestBonus: number;

    Tiers: Record<number, ReductionTier>;
}

interface ReductionTier {
    Records: number;
    Minimum: number;

    Patches: Record<string, ReductionPatch>;
}

interface ReductionPatch {
    NormalCount: number;
    BonusCount: number;

    Normal: Record<number, ReductionReward>;
    Bonus: Record<number, ReductionReward>;
}

export interface ReductionReward {
    Amount: number;
    Total: number;
    Min: number;
    Max: number;
}