import {UpperCaseStr} from "$lib/utils";
import type {SubmarineMap} from "$lib/sheets/submarineMap";

export interface SubmarineExploration {
    RowId: number;

    Destination: string;
    Location: string;
    ExpReward: number;
    SurveyDurationmin: number;
    X: number;
    Y: number;
    Z: number;
    Map: SubmarineMap;
    Stars: number;
    RankReq: number;
    CeruleumTankReq: number;
    SurveyDistance: number;
    StartingPoint: boolean;
}

export function ToName(self: SubmarineExploration): string {
    return UpperCaseStr(self.Destination);
}