using Lumina.Excel.Sheets;
using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Ventures;

public class Ventures : IDisposable
{
    private readonly Dictionary<uint, Venture> ProcessedData = [];
    private readonly Dictionary<VentureTypes, Dictionary<string, VentureTemp>> CollectedData = [];
    
    public void ProcessAllData(Models.VentureModel[] data)
    {
        Logger.Information("Processing venture data");
        PrintVentureStats(data);
        
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }
    
    public void Dispose()
    {
        ProcessedData.Clear();
        CollectedData.Clear();
        GC.Collect();
    }
    
    private void Fetch(Models.VentureModel[] data)
    {
        Logger.Information("Start fetching all existing venture records ...");
        
        foreach (var venture in data)
        {
            GenerateTaskOutput(venture, (VentureTypes)venture.VentureType);

            if (!venture.QuickVenture) 
                continue;
            
            foreach (var extraType in EnumExtensions.ExtraQuickVentureTypes)
            {
                switch (extraType)
                {
                    case VentureTypes.QuickVentureMaxLevel when !venture.MaxLevel:
                    case VentureTypes.QuickVentureMinLevel when venture.MaxLevel:
                        continue;
                }
                    
                GenerateTaskOutput(venture, extraType);
            }
        }
    }
    
    private void Combine()
    {
        Logger.Information("Start processing collected venture task data ...");

        foreach (var (key, patches) in CollectedData)
        {
            var venture = patches.Values.First();
            var taskId = venture.Type;
            if (key is VentureTypes.QuickVentureMaxLevel or VentureTypes.QuickVentureMinLevel)
                taskId = 395; // Quick Venture
            
            // Check if the venture returns randomized reward items, no use in exporting fixed reward ones
            var task = Sheets.RetainerTaskSheet.GetRow(taskId);
            if (!task.IsRandom)
                continue;

            // Get the retainer random task, if this fails something must have been corrupted in the upload
            if (!task.Task.TryGetValue<RetainerTaskRandom>(out var retainerTask))
                continue;
            
            ProcessedData.TryAdd(task.ClassJobCategory.RowId, new Venture(task));

            // Rename task if it is an extra venture task that the game doesn't know about
            var type = venture.Type;
            var taskName = retainerTask.Name.ExtractText();
            if (key is VentureTypes.QuickVentureMaxLevel)
            {
                type = (uint)key;
                taskName += " (Only Max Level)";
            }
            else if (key is VentureTypes.QuickVentureMinLevel)
            {
                type = (uint)key;
                taskName += " (< Level 100)";
            }
            
            var ventureTask = new Venture.Task(taskName, type, []);
            
            // Go over existing patches and calculate all averages
            foreach (var (patch, ventureData) in patches)
                ventureTask.Patches[patch] = ProcessVentureTask(ventureData);
            
            ProcessedData[task.ClassJobCategory.RowId].Tasks.Add(ventureTask); 
        }
    }

    private void Export()
    {
        Logger.Information("Start export of processed venture data ...");
        var ventureList = new List<Venture>();
        foreach (var ventureData in ProcessedData.Values)
        {
            // Order them from oldest to newest task (DoL I -> XII)
            ventureData.Tasks = ventureData.Tasks.OrderBy(t => t.Type).ToList();
            ventureList.Add(ventureData);
        }

        ExportHandler.WriteDataJson("Ventures.json", ventureList.OrderBy(l => l.Category));
        Logger.Information("Done ...");
    }
    
    private void GenerateTaskOutput(Models.VentureModel venture, VentureTypes type)
    {
        if (!CollectedData.ContainsKey(type))
            CollectedData[type] = [];

        var venturePatch = venture.GetPatch;
        var patches = CollectedData[type];
        if (!patches.ContainsKey(venturePatch))
            patches[venturePatch] = new VentureTemp((uint)type);

        patches[venturePatch].AddVentureRecord(venture);
    }
    
    private Venture.Content ProcessVentureTask(VentureTemp venture)
    {
        var primaryList = new List<Reward>();
        var additionalList = new List<Reward>();
        foreach (var (itemId, primary) in venture.PrimaryRewards.OrderBy(pair => pair.Value.Amount))
        {
            if (itemId == 0 || primary.Amount == 0)
                continue;
            
            primaryList.Add(Reward.FromTaskReward(itemId, venture.Total, primary));
            MappingHelper.AddItem(itemId);
        }

        foreach (var (itemId, additional) in venture.AdditionalRewards.OrderBy(pair => pair.Value.Amount))
        {
            if (itemId == 0 || additional.Amount == 0)
                continue;
            
            additionalList.Add(Reward.FromTaskReward(itemId, venture.Total, additional));
            MappingHelper.AddItem(itemId);
        }

        return new Venture.Content((uint) venture.Total, primaryList, additionalList);   
    }
    
    private static void PrintVentureStats(Models.VentureModel[] data)
    {
        // All valid gears are rarity green or higher
        PrintStats("Total quick ventures:", data.Where(v => v.QuickVenture).ToArray());

        // Calculate for only max level retainers
        PrintStats("Total quick ventures (Max Level):", data.Where(v => v is { QuickVenture: true, MaxLevel: true }).ToArray());
        
        // Calculate current patch 
        PrintStats("Total quick ventures (7.4X):", data.Where(v => v.QuickVenture).Where(v => v.GetVersion >= Utils.Patch740).ToArray());
    }
    
    private static void PrintStats(string title, Models.VentureModel[] ventures)
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