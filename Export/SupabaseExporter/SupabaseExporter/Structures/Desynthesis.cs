using Lumina.Excel.Sheets;
using Newtonsoft.Json;

namespace SupabaseExporter.Structures;
    
/// <summary>
/// Used as a JSON structure for export.
/// </summary>
public record DesynthData
{
    public Dictionary<uint, History> Sources = [];
    public Dictionary<uint, History> Rewards = [];

    public Dictionary<uint, ItemInfo> ToItem = [];

    public record ItemInfo
    {
        public uint Id;
        public string Name;

        public ItemInfo(Item item)
        {
            Id = item.RowId;
            Name = item.Name.ExtractText();
        }

        public ItemInfo()
        {

        }
    };
}

/// <summary>
/// A simple helper class for named JSON keys.
/// </summary>
public class History
{
    public uint Records;
    public List<Result> Results = [];

    public class Result
    {
        public uint Id;
        public long Min;
        public long Max;
        public long Received;
        [JsonConverter(typeof(LessPrecisionDouble))] public double Percentage;

        public Result(uint itemId, DesynthTemp.DesynthReward reward)
        {
            Id = itemId;
            Min = reward.Min;
            Max = reward.Max;
            Received = reward.Amount;
        }

        public Result()
        {

        }
    }

    public void AddRecord(uint itemId, DesynthTemp.DesynthReward reward)
    {
        Records += (uint)reward.Amount;
        Results.Add(new Result(itemId, reward));
    }
}

public class DesynthTemp
{
    public long Total;
    public Dictionary<uint, Dictionary<string, DesynthReward>> Rewards = [];
        
    public class DesynthReward
    {
        public long Amount;
        public long Total;
        public long Min = long.MaxValue;
        public long Max = long.MinValue;

        public void AddRewardRecord(uint quantity)
        {
            Amount += 1;
            Total += quantity;
            Min = Math.Min(Min, quantity);
            Max = Math.Max(Max, quantity);
        }

        public void AddExisting(DesynthReward other)
        {
            Amount += other.Amount;
            Total += other.Total;
            Min = Math.Min(Min, other.Min);
            Max = Math.Max(Max, other.Max);
        }
    }
}

public class Desynthesis : IDisposable
{
    private DesynthData ProcessedData = new();
    private readonly Dictionary<uint, DesynthTemp> CollectedData = [];
    
    public void ProcessAllData(List<Models.Desynthesis> data)
    {
        Logger.Information("Processing bunny data");
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }
    
    public void Dispose()
    {
        ProcessedData = new DesynthData();
        CollectedData.Clear();
        GC.Collect();
    }
    
    private void Fetch(List<Models.Desynthesis> data)
    {
        foreach (var record in data)
        {
            if (record.Source > Sheets.MaxItemId)
            {
                Logger.Error($"Invalid source data found, ID: {record.Id}");
                continue;
            }

            var sourceItem = Sheets.ItemSheet.GetRow(record.Source);
            if (sourceItem.Desynth == 0)
            {
                Logger.Error($"Source doesn't allow desynthesis? ID: {record.Id}");
                continue;
            }
            
            if (!CollectedData.ContainsKey(record.Source))
                CollectedData[record.Source] = new DesynthTemp();
            
            var desynthTemp = CollectedData[record.Source];
            desynthTemp.Total += 1;
            
            foreach (var (itemId, amount) in record.GetRewards())
            {
                if (itemId > Sheets.MaxItemId)
                {
                    Logger.Error($"Invalid reward data found, ID: {record.Id}");
                    break;
                }

                if (!desynthTemp.Rewards.ContainsKey(itemId))
                    desynthTemp.Rewards[itemId] = [];

                var patch = record.GetPatch;
                var patches = desynthTemp.Rewards[itemId];
                if (!patches.ContainsKey(patch))
                    patches[patch] = new DesynthTemp.DesynthReward();
                
                patches[patch].AddRewardRecord(amount);
            }
        }
    }

    private void Combine() 
    {
        foreach (var (source, desynthTemp) in CollectedData)
        {
            var sourceItem = Sheets.ItemSheet.GetRow(source);
            if (!ProcessedData.ToItem.ContainsKey(source))
                ProcessedData.ToItem[source] = new DesynthData.ItemInfo(sourceItem);

            var results = new List<History.Result>();
            // TODO Use patch data and not just combine them
            var newTemp = new Dictionary<uint, DesynthTemp.DesynthReward>();
            foreach (var (itemId, patches) in desynthTemp.Rewards)
            {
                foreach (var (_, rewards) in patches)
                {
                    if (!newTemp.ContainsKey(itemId))
                        newTemp[itemId] = new DesynthTemp.DesynthReward();
                    
                    newTemp[itemId].AddExisting(rewards);
                }
            }
            
            foreach (var (itemId, reward) in newTemp)
            {
                var rewardItem = Sheets.ItemSheet.GetRow(itemId);
                if (!ProcessedData.ToItem.ContainsKey(itemId))
                    ProcessedData.ToItem[itemId] = new DesynthData.ItemInfo(rewardItem);

                IconHelper.AddItem(rewardItem);
                results.Add(new History.Result(itemId, reward));

                if (!ProcessedData.Rewards.ContainsKey(itemId))
                    ProcessedData.Rewards[itemId] = new History { Records = (uint)reward.Amount, Results = [new History.Result(source, reward)] };
                else
                    ProcessedData.Rewards[itemId].AddRecord(source, reward);
            }

            for (var i = 0; i < results.Count; i++)
            {
                var r = results[i];
                r.Percentage = (double) r.Received / desynthTemp.Total;
                results[i] = r;
            }

            IconHelper.AddItem(sourceItem);
            ProcessedData.Sources.Add(source, new History { Records = (uint)desynthTemp.Total, Results = results });
        }

        foreach (var (rewardItemId, history) in ProcessedData.Rewards)
        {
            for (var i = 0; i < history.Results.Count; i++)
            {
                var historyResult = history.Results[i];
                historyResult.Percentage = ProcessedData.Sources[historyResult.Id].Results.Find(r => r.Id == rewardItemId).Percentage;
                history.Results[i] = historyResult;
            }
        }
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("DesynthesisData.json", ProcessedData);
        Logger.Information("Done exporting data ...");
    }
}