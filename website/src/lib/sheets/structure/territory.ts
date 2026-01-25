import type {PlaceRow} from "$lib/sheets/structure/placeName";

export interface TerritoryRow {
    RowId: number;

    Map: number;
    QuestBattle: number;
    ContentFinderCondition: number;
    PlaceName: PlaceRow;
    PlaceNameRegion: PlaceRow;
    PlaceNameZone: PlaceRow;
}