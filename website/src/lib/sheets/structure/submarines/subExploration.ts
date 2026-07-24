import {UpperCaseStr} from "$lib/utils";
import {Vector3} from "$lib/math/vector3";
import {numToLetter} from "$lib/submarines/utils";

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

export function ToLetterName(sector: SubExplorationRow): string {
    return `${numToLetter(sector.RowId, true)}. ${ToSectorName(sector)}`
}

export function GetDistance(self: SubExplorationRow, other: SubExplorationRow): number {
    return Math.floor((new Vector3(self.X, self.Y, self.Z).Distance(new Vector3(other.X, other.Y, other.Z))) * 0.035)
}

export function CalcTime(self: SubExplorationRow, other: SubExplorationRow, speed: number): number {
    return GetVoyageTime(self, other, speed) + GetSurveyTime(other, speed);
}

function GetSurveyTime(self: SubExplorationRow, speed: number): number {
    if (speed < 1)
        speed = 1;

    return Math.floor(self.SurveyDurationmin * 7000 / (speed * 100) * 60);
}

function GetVoyageTime(self: SubExplorationRow, other: SubExplorationRow, speed: number): number
{
    if (speed < 1)
        speed = 1;

    return Math.floor((new Vector3(self.X, self.Y, self.Z).Distance(new Vector3(other.X, other.Y, other.Z))) * 3990 / (speed * 100) * 60);
}