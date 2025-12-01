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

interface CofferVariant {
    Id: number;
    Name: string;

    Patches: { [key: string]: CofferContent };
}

interface CofferContent {
    Total: number;
    Items: Reward[];
}

export interface DesynthBase {
    Sources: Record<number, DesynthHistory>;
    Rewards: Record<number, DesynthHistory>;
}

interface DesynthHistory {
    Records: number;
    Rewards: Reward[]
}

export interface Venture {
    Name: string;
    Category: number;
    Tasks: VentureTask[];
}

interface VentureTask {
    Name: string;
    Type: number;
    Patches: Record<string, VentureContent>;
}

interface VentureContent {
    Total: number;
    Primaries: Reward[];
    Secondaries: Reward[];
}