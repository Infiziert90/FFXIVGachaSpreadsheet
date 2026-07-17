import type {Reward} from "$lib/structs/reward";

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
    Chests: Record<string, Chest[]>;
    PatchRecords: Record<string, number>;
}

export interface Chest {
    Records: number;
    Id: number;
    Name: string;
    MapId: number;
    TerritoryId: number;
    PlaceNameSub: string;
    Position: { X: number, Y: number, Z: number };
    Rewards: Reward[];
}