using Lumina.Excel.Sheets;

namespace SupabaseExporter.Structures;

/// <summary>
/// Used as a JSON structure for export.
/// </summary>
[Serializable]
public record VentureData
{
    public string Name;
    public uint Category;
    public List<VentureTask> Tasks = [];

    public VentureData(RetainerTask task)
    {
        Category = task.ClassJobCategory.RowId;

        Name = Category switch
        {
            0 => "All",
            34 => "DoW/DoM",
            _ => task.ClassJobCategory.Value.Name.ExtractText()
        };
    }

    public record VentureTask(string TaskName, uint TaskType)
    {
        public string TaskName = TaskName;
        public uint TaskType = TaskType;
            
        public Dictionary<string, VenturePatch> Patches = [];
    }

    public record VenturePatch(uint TaskTotal, List<Reward> PrimaryItems, List<Reward> AdditionalItems)
    {
        public uint TaskTotal = TaskTotal;
        public List<Reward> PrimaryItems = PrimaryItems;
        public List<Reward> AdditionalItems = AdditionalItems;
    }
}

/// <summary>
/// Venture task data bundled together for calculations.
/// </summary>
public class VentureTemp(uint type)
{
    public uint Type = type;
    public long Total;
    public readonly Dictionary<uint, TaskReward> PrimaryRewards = [];
    public readonly Dictionary<uint, TaskReward> AdditionalRewards = [];
    
    public void AddVentureRecord(Models.Venture venture)
    {
        Total += 1;
        
        if (!PrimaryRewards.ContainsKey(venture.PrimaryId))
            PrimaryRewards[venture.PrimaryId] = new TaskReward();

        PrimaryRewards[venture.PrimaryId].AddRewardRecord(venture.PrimaryCount);

        if (!AdditionalRewards.ContainsKey(venture.AdditionalId))
            AdditionalRewards[venture.AdditionalId] = new TaskReward();
        
        AdditionalRewards[venture.AdditionalId].AddRewardRecord(venture.AdditionalCount);
    }
    
    public class TaskReward
    {
        public long Amount;
        public long Total;
        public long Min = long.MaxValue;
        public long Max = long.MinValue;

        public void AddRewardRecord(short quantity)
        {
            Amount += 1;
            Total += quantity;
            Min = Math.Min(Min, quantity);
            Max = Math.Max(Max, quantity);
        }

        public void AddExisting(TaskReward other)
        {
            Amount += other.Amount;
            Total += other.Total;
            Min = Math.Min(Min, other.Min);
            Max = Math.Max(Max, other.Max);
        }
    }
}

public class Ventures : IDisposable
{
    private readonly Dictionary<uint, VentureData> ProcessedData = [];
    private readonly Dictionary<VentureTypes, Dictionary<string, VentureTemp>> CollectedData = [];
    
    public async Task ProcessAllData(List<Models.Venture> data)
    {
        Console.WriteLine("Processing venture data");
        Fetch(data);
        Combine();
        await Export();
    }
    
    public void Dispose()
    {
        ProcessedData.Clear();
        CollectedData.Clear();
        GC.Collect();
    }
    
    private void Fetch(List<Models.Venture> data)
    {
        Console.WriteLine("Start fetching all existing venture records ...");
        
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
        Console.WriteLine("Start processing all collected venture task data ...");

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
            
            ProcessedData.TryAdd(task.ClassJobCategory.RowId, new VentureData(task));

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
            
            var ventureTask = new VentureData.VentureTask(taskName, type);
            
            // Go over existing patches and calculate all averages
            foreach (var (patch, ventureData) in patches)
                ventureTask.Patches[patch] = ProcessVentureTask(ventureData);
            
            // Add a combined total of all existing patches
            // TODO rewrite to use existing data and aggregate together
            var processingVenture = new VentureTemp(type);
            foreach (var tmp in patches.Values)
            {
                processingVenture.Total += tmp.Total;
                
                foreach (var (itemId, reward) in tmp.PrimaryRewards)
                {
                    if (!processingVenture.PrimaryRewards.ContainsKey(itemId))
                        processingVenture.PrimaryRewards[itemId] = new VentureTemp.TaskReward();
                    
                    processingVenture.PrimaryRewards[itemId].AddExisting(reward);
                }
                
                foreach (var (itemId, reward) in tmp.AdditionalRewards)
                {
                    if (!processingVenture.AdditionalRewards.ContainsKey(itemId))
                        processingVenture.AdditionalRewards[itemId] = new VentureTemp.TaskReward();
                    
                    processingVenture.AdditionalRewards[itemId].AddExisting(reward);
                }
            }
            ventureTask.Patches["All"] = ProcessVentureTask(processingVenture);
            
            ProcessedData[task.ClassJobCategory.RowId].Tasks.Add(ventureTask); 
        }
    }

    private async Task Export()
    {
        Console.WriteLine("Start export of processed venture data ...");
        var ventureList = new List<VentureData>();
        foreach (var ventureData in ProcessedData.Values)
        {
            // Order them from oldest to newest task (DoL I -> XII)
            ventureData.Tasks = ventureData.Tasks.OrderBy(t => t.TaskType).ToList();
            ventureList.Add(ventureData);
        }

        await ExportHandler.WriteDataJson("VentureData.json", ventureList.OrderBy(l => l.Category));
        Console.WriteLine("Done ...");
    }
    
    private void GenerateTaskOutput(Models.Venture venture, VentureTypes type)
    {
        if (!CollectedData.ContainsKey(type))
            CollectedData[type] = [];

        var venturePatch = venture.GetPatch;
        var patches = CollectedData[type];
        if (!patches.ContainsKey(venturePatch))
            patches[venturePatch] = new VentureTemp((uint)type);

        patches[venturePatch].AddVentureRecord(venture);
    }
    
    private VentureData.VenturePatch ProcessVentureTask(VentureTemp venture)
    {
        var primaryList = new List<Reward>();
        var additionalList = new List<Reward>();
        foreach (var (itemId, primary) in venture.PrimaryRewards.OrderBy(pair => pair.Value.Amount))
        {
            if (itemId == 0 || primary.Amount == 0)
                continue;
            
            var item = Sheets.ItemSheet.GetRow(itemId);
            primaryList.Add(Reward.FromTaskReward(item, primary));

            IconHelper.AddItem(item);
        }

        foreach (var (itemId, additional) in venture.AdditionalRewards.OrderBy(pair => pair.Value.Amount))
        {
            if (itemId == 0 || additional.Amount == 0)
                continue;
            
            var item = Sheets.ItemSheet.GetRow(itemId);
            additionalList.Add(Reward.FromTaskReward(item, additional));

            IconHelper.AddItem(item);
        }

        return new VentureData.VenturePatch((uint) venture.Total, primaryList, additionalList);   
    }
}