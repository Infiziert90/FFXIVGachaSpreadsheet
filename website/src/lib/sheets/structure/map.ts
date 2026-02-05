import type {PlaceRow} from "$lib/sheets/structure/placeName";

export interface MapRow {
    RowId: number;

    Id: string;
    MapMarkerRange: number;
    OffsetX: number;
    OffsetY: number;
    SizeFactor: number;
    Territory: number;
    PlaceName: PlaceRow;
    PlaceNameSub: PlaceRow;
    PlaceNameRegion: PlaceRow;
}