import {UpperCaseStr} from "$lib/utils";

export interface SubMapRow {
    RowId: number;

    Name: string;
}

export function ToMapName(self: SubMapRow): string {
    return UpperCaseStr(self.Name);
}