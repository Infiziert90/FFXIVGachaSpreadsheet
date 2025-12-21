import {SubmarineMapSheet} from "$lib/sheets";
import {UpperCaseStr} from "$lib/utils";
import type {SubmarineExploration} from "$lib/sheets/submarineExploration";

export interface SubmarineMap {
    RowId: number;

    Name: string;
}

interface MapSheet {
    schema: string;
    rows: MapRow[];
}

interface MapRow {
    row_id: number;
    fields: MapFields;
}

interface MapFields {
    Name: string;
}

export function ToName(self: SubmarineMap): string {
    return UpperCaseStr(self.Name);
}

export function ReadMapSheet(data: MapSheet) {
    for (const row of data.rows) {
        SubmarineMapSheet[row.row_id] = {
            RowId: row.row_id,
            Name: row.fields.Name,
        }
    }
}