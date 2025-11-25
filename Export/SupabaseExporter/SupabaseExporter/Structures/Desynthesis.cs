namespace SupabaseExporter.Structures;
    
/// <summary>
/// Used as a JSON structure for export.
/// </summary>
public record DesynthesisBase
{
    public Dictionary<uint, History> Sources = [];
    public Dictionary<uint, History> Rewards = [];
}

/// <summary>
/// A simple helper class for named JSON keys.
/// </summary>
public class History
{
    public uint Records;
    public List<Reward> Rewards = [];

    public void AddRecord(uint itemId, DesynthTemp.DesynthReward reward)
    {
        Records += (uint)reward.Amount;
        Rewards.Add(Reward.FromDesynthesisReward(itemId, 10000, reward)); // Total is unknown at this point, so give it a fake value
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
    private DesynthesisBase ProcessedData = new();
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
        ProcessedData = new DesynthesisBase();
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
        Dictionary<uint, History> tempRewards = [];
        foreach (var (sourceId, desynthTemp) in CollectedData)
        {
            var sourceItem = Sheets.ItemSheet.GetRow(sourceId);

            var results = new List<Reward>();
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

                MappingHelper.AddItem(itemId);
                results.Add(Reward.FromDesynthesisReward(itemId, desynthTemp.Total, reward));

                if (!tempRewards.ContainsKey(itemId))
                    tempRewards[itemId] = new History();

                tempRewards[itemId].AddRecord(sourceId, reward);
            }

            MappingHelper.AddItem(sourceId);
            ProcessedData.Sources.Add(sourceId, new History { Records = (uint)desynthTemp.Total, Rewards = results });
        }

        foreach (var (rewardItemId, history) in tempRewards)
        {
            ProcessedData.Rewards[rewardItemId] = new History {Records = history.Records};
            foreach (var tempReward in history.Rewards)
            {
                var source = ProcessedData.Sources[tempReward.Id].Rewards.Find(r => r.Id == rewardItemId);
                if (source == null)
                    throw new Exception($"Source for id {rewardItemId} not found!");
                
                ProcessedData.Rewards[rewardItemId].Rewards.Add(new Reward(
                    tempReward.Id,
                    tempReward.Amount,
                    source.Pct,
                    source.Total,
                    tempReward.Min,
                    tempReward.Max
                ));
            }
        }
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("DesynthesisData.json", ProcessedData);
        Logger.Information("Done exporting data ...");
    }
}