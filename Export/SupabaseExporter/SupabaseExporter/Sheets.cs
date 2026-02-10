using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace SupabaseExporter;

public static class Sheets
{
    private static readonly GameData Lumina;

    public static readonly ExcelSheet<Map> MapSheet;
    public static readonly ExcelSheet<Item> ItemSheet;
    public static readonly ExcelSheet<Mount> MountSheet;
    public static readonly ExcelSheet<Treasure> TreasureSheet;
    public static readonly ExcelSheet<ContentType> ContentTypeSheet;
    public static readonly ExcelSheet<RetainerTask> RetainerTaskSheet;
    public static readonly ExcelSheet<GCSupplyDutyReward> GCSupplySheet;
    public static readonly ExcelSheet<TerritoryType> TerritoryTypeSheet;
    public static readonly ExcelSheet<SubmarineExploration> ExplorationSheet;
    public static readonly ExcelSheet<BNpcName> BNPCNameSheet;
    public static readonly ExcelSheet<Pet> PetSheet;
    public static readonly ExcelSheet<Companion> CompanionSheet;
    public static readonly ExcelSheet<NotoriousMonster> NotoriousMonsterSheet;
    
    public static readonly ExcelSheet<SubmarineExploration> SubmarineExplorationSheet;
    public static readonly ExcelSheet<SubmarineMap> SubmarineMapSheet;
    
    public static readonly SubrowExcelSheet<MapMarker> MapMarkerSheet;
    public static readonly ExcelSheet<HousingLandSet> HousingLandSetSheet;
    public static readonly SubrowExcelSheet<HousingMapMarkerInfo> HousingMapMarkerSheet;
    
    public static readonly ExcelSheet<World> WorldSheet;
    public static readonly ExcelSheet<WorldDCGroupType> WorldDCGroupSheet;

    // Item
    public static readonly uint MaxItemId;
    
    // Submarine
    private static readonly uint[] ReversedMaps;
    
    // Bnpc tracking
    public static HashSet<uint> RankedBnpcBase;

    static Sheets()
    {
        Lumina = new GameData(Environment.GetEnvironmentVariable("game_path")!);

        MapSheet = Lumina.GetExcelSheet<Map>()!;
        ItemSheet = Lumina.GetExcelSheet<Item>()!;
        MountSheet = Lumina.GetExcelSheet<Mount>()!;
        TreasureSheet = Lumina.GetExcelSheet<Treasure>()!;
        ContentTypeSheet = Lumina.GetExcelSheet<ContentType>()!;
        RetainerTaskSheet = Lumina.GetExcelSheet<RetainerTask>()!;
        GCSupplySheet = Lumina.GetExcelSheet<GCSupplyDutyReward>()!;
        TerritoryTypeSheet = Lumina.GetExcelSheet<TerritoryType>()!;
        ExplorationSheet = Lumina.GetExcelSheet<SubmarineExploration>()!;
        BNPCNameSheet = Lumina.GetExcelSheet<BNpcName>()!;
        PetSheet = Lumina.GetExcelSheet<Pet>()!;
        CompanionSheet = Lumina.GetExcelSheet<Companion>()!;
        NotoriousMonsterSheet = Lumina.GetExcelSheet<NotoriousMonster>()!;
        SubmarineExplorationSheet = Lumina.GetExcelSheet<SubmarineExploration>()!;
        SubmarineMapSheet = Lumina.GetExcelSheet<SubmarineMap>()!;
        MapMarkerSheet = Lumina.GetSubrowExcelSheet<MapMarker>()!;
        HousingLandSetSheet = Lumina.GetExcelSheet<HousingLandSet>()!;
        HousingMapMarkerSheet = Lumina.GetSubrowExcelSheet<HousingMapMarkerInfo>()!;
        WorldSheet = Lumina.GetExcelSheet<World>()!;
        WorldDCGroupSheet = Lumina.GetExcelSheet<WorldDCGroupType>()!;

        MaxItemId = ItemSheet.MaxBy(i => i.RowId).RowId;
        
        ReversedMaps = ExplorationSheet.Where(s => s.StartingPoint).Select(s => s.RowId).Reverse().ToArray();
        
        RankedBnpcBase = NotoriousMonsterSheet.Where(n => n.Rank is 1 or 2 or 3).Select(n => n.RowId).ToHashSet();
    }
    
    public static SubmarineExploration FindVoyageStart(uint sector)
    {
        // This works because we reversed the list of start points
        return ExplorationSheet.GetRow(ReversedMaps.FirstOrDefault(m => sector >= m));
    }
}