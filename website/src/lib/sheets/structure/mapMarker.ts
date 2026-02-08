import type {PlaceRow} from "$lib/sheets/structure/placeName";

export interface MapMarkerRow {
    RowId: number;

    PlaceNameSubtext: PlaceRow;
    X: number;
    Y: number;
    SubtextOrientation: number;
    Icon: number;
    DataType: number;
    Unknown0: number;
}