export interface HousingLandSetRow {
    RowId: number;

    Sets: LandSetEntry[];
}

export interface LandSetEntry {
    PlacardId: number;
    InitialPrice: number;
    PlotSize: number;
}