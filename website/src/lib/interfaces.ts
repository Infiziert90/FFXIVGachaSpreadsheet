import type {Reward} from "$lib/structs/reward";

export interface DesynthBase {
    Sources: Record<number, DesynthHistory>;
    Rewards: Record<number, DesynthHistory>;
}

// This must be exposed because of Source and Reward being the same type
export interface DesynthHistory {
    Records: number;
    Rewards: Reward[]
}

export interface DesynthesisBase {
    Sources: number[];
    Rewards: number[];
}

export interface Venture {
    Name: string;
    Category: number;
    Tasks: VentureTask[];
}

export interface VentureTask {
    Name: string;
    Type: number;
    Patches: Record<string, VentureContent>;
}

export interface VentureContent {
    Total: number;
    Primaries: Reward[];
    Secondaries: Reward[];
}

export interface SubLoot {
    Total: number;
    Sectors: Record<number, Sector>;
}

export interface Sector {
    Records: number;
    T3Capable: number;

    Id: number;
    Name: string;
    Letter: string;
    Rank: number;
    Stars: number;
    UnlockedFrom: number;

    Pools: Record<string, LootPool>;
}

export interface LootPool {
    Records: number;
    Rewards: Record<number, PoolReward>;
    Stats: Stats;
}

export interface PoolReward {
    Id: number;
    Amount: number;
    Total: number;
    WasT3: number;
    MinMax: Record<string, [number, number]>;
}

export interface Stats {
    Min: number;
    Mid: number;
    High: number;

    Low: number;
    Normal: number;
    Optimal: number;

    Favor: number;
    DoubleDips: number;
}

// Used internally
export interface UniqueLocation {
    Territory: number;
    Map: number;
}