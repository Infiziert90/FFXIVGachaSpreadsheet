import {UpperCaseStr} from "$lib/utils";

export interface SubExplorationRow {
    RowId: number;

    Destination: string;
    Location: string;
    ExpReward: number;
    SurveyDurationmin: number;
    X: number;
    Y: number;
    Z: number;
    Map: number;
    Stars: number;
    RankReq: number;
    CeruleumTankReq: number;
    SurveyDistance: number;
    StartingPoint: boolean;
}

export function ToSectorName(self: SubExplorationRow): string {
    return UpperCaseStr(self.Destination);
}