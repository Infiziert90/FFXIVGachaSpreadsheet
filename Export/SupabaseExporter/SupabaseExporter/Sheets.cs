using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace SupabaseExporter;

public static class Sheets
{
    private static readonly GameData Lumina;

    public static readonly ExcelSheet<Map> MapSheet;
    public static readonly ExcelSheet<Item> ItemSheet;
    public static readonly ExcelSheet<Treasure> TreasureSheet;
    public static readonly ExcelSheet<ContentType> ContentTypeSheet;
    public static readonly ExcelSheet<RetainerTask> RetainerTaskSheet;
    public static readonly ExcelSheet<GCSupplyDutyReward> GCSupplySheet;
    public static readonly ExcelSheet<TerritoryType> TerritoryTypeSheet;

    public static readonly uint MaxItemId;

    static Sheets()
    {
        Lumina = new GameData(Environment.GetEnvironmentVariable("game_path")!);

        MapSheet = Lumina.GetExcelSheet<Map>()!;
        ItemSheet = Lumina.GetExcelSheet<Item>()!;
        TreasureSheet = Lumina.GetExcelSheet<Treasure>()!;
        ContentTypeSheet = Lumina.GetExcelSheet<ContentType>()!;
        RetainerTaskSheet = Lumina.GetExcelSheet<RetainerTask>()!;
        GCSupplySheet = Lumina.GetExcelSheet<GCSupplyDutyReward>()!;
        TerritoryTypeSheet = Lumina.GetExcelSheet<TerritoryType>()!;

        MaxItemId = ItemSheet.MaxBy(i => i.RowId).RowId;
    }
}