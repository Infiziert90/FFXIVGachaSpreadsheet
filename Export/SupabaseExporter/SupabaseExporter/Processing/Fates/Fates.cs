using SupabaseExporter.Structures.Exports;

namespace SupabaseExporter.Processing.Fates;

public enum RewardType : byte {
    FateReward = 0,
    DynamicEventReward = 1,
    TreasureHuntReward = 2,
    GoldSaucerReward = 3,
    MJIReward = 4,
    WKSReward = 5,
}

public class Fates : IDisposable
{
    private readonly FateRewardTemp CollectedData = new();
    private readonly FateReward ProcessedData = new();
    
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
        CollectedData.Expansions.Clear();
        ProcessedData.Expansions.Clear();
        GC.Collect();
    }
    
    private void Fetch(Models.FateRewardModel[] data)
    {
        foreach (var record in data)
        {
            if ((RewardType)record.Type is RewardType.TreasureHuntReward or RewardType.WKSReward or RewardType.MJIReward or RewardType.GoldSaucerReward)
                continue;

            if (record.Success != 1)
                continue;

            if (record.Medal != 3 && record.Medal != 0)
                continue;

            var terri = Sheets.TerritoryTypeSheet.GetRow(record.Territory);
            if (!CollectedData.Expansions.ContainsKey(terri.ExVersion.RowId))
                CollectedData.Expansions[terri.ExVersion.RowId] = new FateRewardTemp.Expansion(terri.ExVersion.RowId);

            var expansion = CollectedData.Expansions[terri.ExVersion.RowId];
            expansion.Records += 1;
            if (!expansion.Territories.ContainsKey(record.Territory))
                expansion.Territories[record.Territory] = new FateRewardTemp.Territory(record.Territory);
            
            var territory = expansion.Territories[record.Territory];
            territory.Records += 1;
            if (!territory.FateTypes.ContainsKey(record.Type))
                territory.FateTypes[record.Type] = new FateRewardTemp.FateType(record.Type);
            
            var type = territory.FateTypes[record.Type];
            type.Records += 1;

            var fateId = record.FateId;
            if (record.Name.Length != 0)
            {
                if (!Sheets.EngagementNames.TryGetValue(record.Name, out fateId))
                {
                    Logger.Error($"Invalid CE name, ID: {record.Id}");
                    continue;
                }
            }

            if (fateId == 0)
            {
                Logger.Error($"Invalid fate id, ID: {record.Id}");
                continue;
            }
            
            if (!type.Fates.ContainsKey(fateId))
                type.Fates[fateId] = new FateRewardTemp.Fate(fateId);
            
            var fate = type.Fates[fateId];
            fate.Records += 1;

            if (record.FateTokenTypeItemId != 0 && record.FateTokenTypeItemId < Sheets.MaxItemId)
            {
                if (!fate.Rewards.ContainsKey(record.FateTokenTypeItemId))
                    fate.Rewards[record.FateTokenTypeItemId] = new FateRewardTemp.RewardTemp();

                fate.Rewards[record.FateTokenTypeItemId].AddRewardRecord(record.FateTokenTypeAmount);
            }
            
            if (record.GCSealsAmount != 0)
            {
                if (!fate.Rewards.ContainsKey(20))
                    fate.Rewards[20] = new FateRewardTemp.RewardTemp();

                fate.Rewards[20].AddRewardRecord(record.GCSealsAmount);
            }
            
            if (record.CurrencyAmount != 0)
            {
                if (!fate.Rewards.ContainsKey(1))
                    fate.Rewards[1] = new FateRewardTemp.RewardTemp();

                fate.Rewards[1].AddRewardRecord(record.CurrencyAmount);
            }
            
            foreach (var (itemId, amount) in record.GetRewards())
            {
                if (itemId > Sheets.MaxItemId)
                {
                    Logger.Error($"Invalid reward found, ID: {record.Id}");
                    continue;
                }
                
                if (!fate.Rewards.ContainsKey(itemId))
                    fate.Rewards[itemId] = new FateRewardTemp.RewardTemp();

                fate.Rewards[itemId].AddRewardRecord(amount);
            }
            
            foreach (var (itemId, amount) in record.GetAdditionalRewards())
            {
                if (itemId > Sheets.MaxItemId)
                {
                    Logger.Error($"Invalid additional reward found, ID: {record.Id}");
                    continue;
                }
                
                if (!fate.Rewards.ContainsKey(itemId))
                    fate.Rewards[itemId] = new FateRewardTemp.RewardTemp();

                fate.Rewards[itemId].AddRewardRecord(amount);
            }
        }
    }

    private void Combine()
    {
        foreach (var expansionTemp in CollectedData.Expansions.Values)
        {
            var expansion = new FateReward.Expansion {Id = expansionTemp.Id, Records = expansionTemp.Records};
            
            foreach (var territoryTemp in expansionTemp.Territories.Values)
            {
                var territory = new FateReward.Territory {Id = territoryTemp.Id, Records = territoryTemp.Records};

                foreach (var fateTypeTemp in territoryTemp.FateTypes.Values)
                {
                    var fateType = new FateReward.FateType {Id = fateTypeTemp.Id, Records = fateTypeTemp.Records};
                    
                    foreach (var fateTemp in fateTypeTemp.Fates.Values)
                    {
                        var fate = new FateReward.Fate {Id = fateTemp.Id, Records = fateTemp.Records};
                        
                        foreach (var (rewardId, rewardTemp) in fateTemp.Rewards)
                        {
                            fate.Rewards.Add(Reward.FromFateReward(rewardId, fateTemp.Records, rewardTemp));
                            MappingHelper.AddItem(rewardId);
                        }
                        
                        fate.Rewards = fate.Rewards.OrderBy(r => r.Id).ToList();
                        fateType.Fates.Add(fate);
                    }
                    
                    fateType.Fates = fateType.Fates.OrderBy(t => t.Id).ToList();
                    territory.FateTypes.Add(fateType);
                }
                
                territory.FateTypes = territory.FateTypes.OrderBy(t => t.Id).ToList();
                expansion.Territories.Add(territory);
            }
            
            expansion.Territories = expansion.Territories.OrderBy(s => s.Id).ToList();
            ProcessedData.Expansions.Add(expansion);
        }
        
        ProcessedData.Expansions = ProcessedData.Expansions.OrderBy(s => s.Id).ToList();
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("FateReward.json", ProcessedData, withIndent: true);
        Logger.Information("Done exporting data ...");
    }
}