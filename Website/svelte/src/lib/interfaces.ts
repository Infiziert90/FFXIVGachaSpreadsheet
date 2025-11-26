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
    Territory: number;

    Coffers: CofferVariant[];
}

interface CofferVariant {
    CofferId: number;
    CofferName: string;

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

export interface DesynthHistory {
    Records: number;
    Rewards: Reward[]
}