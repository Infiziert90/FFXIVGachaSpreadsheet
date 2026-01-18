export interface Reward {
    Id: number;
    Amount: number;
    Pct: number;
    Total: number;
    Min: number;
    Max: number;
}

export interface Coffer {
    Name: string;
    TerritoryId: number;

    Variants: CofferVariant[];
}

export interface CofferVariant {
    Id: number;
    Name: string;

    Patches: { [key: string]: CofferContent };
}

export interface CofferContent {
    Total: number;
    Items: Reward[];
}

export interface DesynthBase {
    Sources: Record<number, DesynthHistory>;
    Rewards: Record<number, DesynthHistory>;
}

// This must be exposed because of Source and Reward being the same type
export interface DesynthHistory {
    Records: number;
    Rewards: Reward[]
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

interface VentureContent {
    Total: number;
    Primaries: Reward[];
    Secondaries: Reward[];
}

export interface ChestDrop {
    Id: number;
    Name: string;
    Expansions: Expansion[];
}

export interface Expansion {
    Id: number;
    Name: string;
    Headers: Header[];
}

export interface Header {
    Id: number;
    Name: string;
    Duties: Duty[];
}

export interface Duty {
    Records: number;
    Id: number;
    Name: string;
    SortKey: number;
    Chests: Chest[];
}

export interface Chest {
    Records: number;
    Id: number;
    Name: string;
    MapId: number;
    TerritoryId: number;
    PlaceNameSub: string;
    Rewards: Reward[];
}

export interface SubLoot {
    Total: number;
    Sectors: Record<number, Sector>;
}

export interface Sector {
    Records: number;
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