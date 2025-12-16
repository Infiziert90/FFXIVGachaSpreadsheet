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

    // Item
    public static readonly uint MaxItemId;
    
    // Submarine
    private static readonly uint[] ReversedMaps;

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

        MaxItemId = ItemSheet.MaxBy(i => i.RowId).RowId;
        
        ReversedMaps = ExplorationSheet.Where(s => s.StartingPoint).Select(s => s.RowId).Reverse().ToArray();
    }
    
    public static SubmarineExploration FindVoyageStart(uint sector)
    {
        // This works because we reversed the list of start points
        return ExplorationSheet.GetRow(ReversedMaps.FirstOrDefault(m => sector >= m));
    }
}