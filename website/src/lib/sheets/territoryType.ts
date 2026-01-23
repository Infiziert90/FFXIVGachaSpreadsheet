import type {PlaceName} from "$lib/sheets/placeName";

export interface TerritoryType {
    RowId: number;

    Map: number;
    PlaceName: PlaceName;
    PlaceNameRegion: PlaceName;
    PlaceNameZone: PlaceName;
}