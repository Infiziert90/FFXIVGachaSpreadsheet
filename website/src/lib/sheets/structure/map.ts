import type {PlaceRow} from "$lib/sheets/structure/placeName";

export interface MapRow {
    RowId: number;

    Id: string;
    OffsetX: number;
    OffsetY: number;
    SizeFactor: number;
    PlaceName: PlaceRow;
    PlaceNameSub: PlaceRow;
    PlaceNameRegion: PlaceRow;
}