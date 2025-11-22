export interface Reward {
    Name: string;
    Id: number;
    Amount: number;
    Percentage: number;
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