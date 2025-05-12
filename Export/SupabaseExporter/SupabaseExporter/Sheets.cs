using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace SupabaseExporter;

public static class Sheets
{
    private static readonly GameData Lumina;

    public static readonly ExcelSheet<Item> ItemSheet;
    public static readonly ExcelSheet<GCSupplyDutyReward> GCSupplySheet;
    public static readonly ExcelSheet<RetainerTask> RetainerTaskSheet;

    public static readonly uint MaxItemId;

    static Sheets()
    {
        Lumina = new GameData(Environment.GetEnvironmentVariable("game_path")!);

        ItemSheet = Lumina.GetExcelSheet<Item>()!;
        GCSupplySheet = Lumina.GetExcelSheet<GCSupplyDutyReward>()!;
        RetainerTaskSheet = Lumina.GetExcelSheet<RetainerTask>()!;

        MaxItemId = ItemSheet.MaxBy(i => i.RowId).RowId;
    }
}