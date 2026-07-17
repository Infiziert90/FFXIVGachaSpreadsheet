import type {Reward} from "$lib/structs/reward";

export interface DesynthBase2 {
    Patches: Record<string, DesynthPatch>;
}

export interface DesynthPatch {
    Records: number;

    Sources: Record<number, SourceHistory>;
    Rewards: Record<number, RewardHistory>;
}

export interface SourceHistory {
    ILvl: number;
    Job: number;

    A: number;
    Above: Reward[];

    B: number;
    Below: Reward[];
}

export interface RewardHistory {
    Records: number;
    Rewards: Reward[];
}

export interface DesynthesisBase2 {
    Sources: number[];
    Rewards: number[];
}

export function isSource(data: SourceHistory | RewardHistory) {
    return 'Above' in data;
}