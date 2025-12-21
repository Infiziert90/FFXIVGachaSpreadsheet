using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Desynthesis;

public class Desynthesis : IDisposable
{
    private Desynth ProcessedData = new();
    private readonly Dictionary<uint, DesynthTemp> CollectedData = [];
    
    public void ProcessAllData(Models.DesynthesisModel[] data)
    {
        Logger.Information("Processing desynthesis data");
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }
    
    public void Dispose()
    {
        ProcessedData.Sources.Clear();
        ProcessedData.Rewards.Clear();
        CollectedData.Clear();
        GC.Collect();
    }
    
    private void Fetch(Models.DesynthesisModel[] data)
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
        Dictionary<uint, Desynth.History> tempRewards = [];
        foreach (var (sourceId, desynthTemp) in CollectedData)
        {
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
                MappingHelper.AddItem(itemId);
                results.Add(Reward.FromDesyntReward(itemId, desynthTemp.Total, reward));

                if (!tempRewards.ContainsKey(itemId))
                    tempRewards[itemId] = new Desynth.History();

                tempRewards[itemId].AddRecord(sourceId, reward);
            }

            MappingHelper.AddItem(sourceId);
            ProcessedData.Sources.Add(sourceId, new Desynth.History { Records = (uint)desynthTemp.Total, Rewards = results });
        }

        foreach (var (rewardItemId, history) in tempRewards)
        {
            ProcessedData.Rewards[rewardItemId] = new Desynth.History {Records = history.Records};
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
        ExportHandler.WriteDataJson("Desynthesis.json", ProcessedData);
        Logger.Information("Done exporting data ...");
    }
}