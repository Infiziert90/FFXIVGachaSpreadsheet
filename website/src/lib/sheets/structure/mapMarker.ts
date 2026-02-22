import type {PlaceRow} from "$lib/sheets/structure/placeName";

export interface MapMarkerRow {
    RowId: number;

    PlaceNameSubtext: PlaceRow;
    X: number;
    Y: number;
    Icon: number;
    SubtextOrientation: number;
    DataType: number;
}