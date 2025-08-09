using Lumina.Excel.Sheets;

namespace SupabaseExporter;

public static class PrintOutput
{
    public static void PrintVentureStats(List<Models.Venture> data)
    {
        // All valid gears are rarity green or higher
        PrintStats("Total quick ventures:", data.Where(v => v.QuickVenture).ToArray());

        // Calculate for only max level retainers
        PrintStats("Total quick ventures (Max Level):", data.Where(v => v is { QuickVenture: true, MaxLevel: true }).ToArray());
        
        // Calculate current patch 
        PrintStats("Total quick ventures (7.3X):", data.Where(v => v.QuickVenture).Where(v => v.GetVersion >= Utils.Patch730).ToArray());
    }
    
    private static void PrintStats(string title, Models.Venture[] ventures)
    {
        (Item Item, bool HQ)[] validGear = ventures.Select(v => (Sheets.ItemSheet.GetRow(v.PrimaryId), v.PrimaryHq)).Where(i => i.Item1.Rarity > 1).ToArray();
        var totalLvl = (double) validGear.Sum(i => i.Item.LevelItem.RowId);
        var totalSeals = (double) validGear.Sum(i => Sheets.GCSupplySheet.GetRow(i.Item.LevelItem.RowId).SealsExpertDelivery);
        var totalFCPoints = validGear.Sum(Utils.CalculateFCPoints);
            
        Logger.Information($"{title} {ventures.Length:N0}");
        Logger.Information("");
        Logger.Information("= Gear Average =");
        Logger.Information($"iLvL: {totalLvl / ventures.Length:F2}");
        Logger.Information($"FC Points: {totalFCPoints / ventures.Length:F2}");
        Logger.Information($"GC Seals: {totalSeals / ventures.Length:F2}");
    }
}