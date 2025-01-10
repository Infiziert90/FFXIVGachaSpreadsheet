using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace SupabaseExporter;

public static class Sheets
{
    public static readonly GameData Lumina;

    public static readonly ExcelSheet<Item> ItemSheet;
    public static readonly ExcelSheet<GCSupplyDutyReward> GCSupplySheet;
    public static readonly ExcelSheet<RetainerTask> RetainerTaskSheet;

    static Sheets()
    {
        Lumina = new GameData(Environment.GetEnvironmentVariable("game_path")!);

        ItemSheet = Lumina.GetExcelSheet<Item>()!;
        GCSupplySheet = Lumina.GetExcelSheet<GCSupplyDutyReward>()!;
        RetainerTaskSheet = Lumina.GetExcelSheet<RetainerTask>()!;
    }
}