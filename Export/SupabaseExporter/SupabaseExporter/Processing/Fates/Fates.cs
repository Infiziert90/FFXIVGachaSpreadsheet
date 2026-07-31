using SupabaseExporter.Structures.Exports;

namespace SupabaseExporter.Processing.Fates;

public class Fates : IDisposable
{
    class TempReward
    {
        public long Records;
        public Dictionary<string, long> Rewards = [];
        public Dictionary<string, long> AdditionalRewards = [];
    }
    
    private readonly Dictionary<uint, Dictionary<byte, Dictionary<string, TempReward>>> CollectedData = new();
    private readonly Reduce ProcessData = new();
    
    public void ProcessAllData(Models.FateRewardModel[] data)
    {
        Logger.Information("Processing fate data");
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }
    
    public void Dispose()
    {
        CollectedData.Clear();
        ProcessData.Jobs.Clear();
        GC.Collect();
    }
    
    private void Fetch(Models.FateRewardModel[] data)
    {
        foreach (var record in data)
        {
            if (!CollectedData.ContainsKey(record.Territory))
                CollectedData[record.Territory] = [];
            
            var territory =  CollectedData[record.Territory];
            if (!territory.ContainsKey(record.Type))
                territory[record.Type] = [];
            
            var type = territory[record.Type];
            if (!type.ContainsKey(record.Name))
                type[record.Name] = new TempReward();
            
            var rewards = type[record.Name];
            rewards.Records += 1;
            
            foreach (var (itemId, amount) in record.GetRewards())
            {
                if (itemId > Sheets.MaxItemId)
                {
                    Logger.Error($"Invalid reward found, ID: {record.Id}");
                    continue;
                }
                
                MappingHelper.AddItem(itemId);
                
                var item = Sheets.ItemSheet.GetRow(itemId);
                var name = item.Name.ToString();
                if (!rewards.Rewards.ContainsKey(name))
                    rewards.Rewards[name] = 0;

                rewards.Rewards[name] += 1;
            }
            
            foreach (var (itemId, amount) in record.GetAdditionalRewards())
            {
                if (itemId > Sheets.MaxItemId)
                {
                    Logger.Error($"Invalid additional reward found, ID: {record.Id}");
                    continue;
                }
                
                MappingHelper.AddItem(itemId);
                
                var item = Sheets.ItemSheet.GetRow(itemId);
                var name = item.Name.ToString();
                if (!rewards.AdditionalRewards.ContainsKey(name))
                    rewards.AdditionalRewards[name] = 0;

                rewards.AdditionalRewards[name] += 1;
            }
        }
    }

    private void Combine()
    {
        
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("FateReward.json", CollectedData, withIndent: true);
        Logger.Information("Done exporting data ...");
    }
}