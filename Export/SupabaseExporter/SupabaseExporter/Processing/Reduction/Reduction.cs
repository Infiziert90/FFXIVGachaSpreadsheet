using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Reduction;

public class Reduction : IDisposable
{
    private Reduce ProcessedData = new();
    private readonly Dictionary<uint, ReductionTemp> CollectedData = [];
    
    public void ProcessAllData(Models.ReductionModel[] data)
    {
        Logger.Information("Processing reduction data");
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }
    
    public void Dispose()
    {
        ProcessedData.Sources.Clear();
        CollectedData.Clear();
        GC.Collect();
    }
    
    private void Fetch(Models.ReductionModel[] data)
    {
        foreach (var record in data)
        {
            if (record.Source > Sheets.MaxItemId)
            {
                Logger.Error($"Invalid source data found, ID: {record.Id}");
                continue;
            }

            var sourceItem = Sheets.ItemSheet.GetRow(record.Source);
            if (sourceItem.AetherialReduce == 0)
            {
                Logger.Error($"Source doesn't allow reduction? ID: {record.Id}");
                continue;
            }
            
            if (!CollectedData.ContainsKey(record.Source))
                CollectedData[record.Source] = new ReductionTemp();
            
            var reductionTemp = CollectedData[record.Source];
            reductionTemp.Total += 1;
            
            foreach (var (itemId, amount) in record.GetRewards())
            {
                if (itemId > Sheets.MaxItemId)
                {
                    Logger.Error($"Invalid reward data found, ID: {record.Id}");
                    break;
                }

                // Check that our reward is different from source
                if (itemId == sourceItem.RowId)
                {
                    Logger.Error($"Source and reward are the same, ID: {record.Id}");
                    break;
                }
                
                // Check item rarity
                if (Sheets.ItemSheet.GetRow(itemId).Rarity >= 3)
                {
                    Logger.Error($"Reward rarity check, ID: {record.Id}");
                    break;
                }
                
                var reductionRow = sourceItem.AetherialReduce;
                var subrowRewards = Sheets.GathererReductionRewardSheet.GetRow(reductionRow);

                var collectabilityTier = 0u;
                foreach (var rewardRow in subrowRewards)
                {
                    if (record.Collectability > rewardRow.Unknown0)
                        continue;

                    collectabilityTier = rewardRow.SubrowId > 0u ? rewardRow.SubrowId - 1u : 0u;
                }

                if (!reductionTemp.Rewards.ContainsKey(collectabilityTier))
                    reductionTemp.Rewards[collectabilityTier] = [];
                
                var patch = record.GetPatch;
                var patches = reductionTemp.Rewards[collectabilityTier];
                if (!patches.ContainsKey(patch))
                    patches[patch] = [];
                
                if (!patches[patch].ContainsKey(itemId))
                    patches[patch][itemId] = [];
                
                if (!patches[patch][itemId].ContainsKey(record.HasBonus))
                    patches[patch][itemId][record.HasBonus] = new ReductionTemp.ReductionReward();
                
                patches[patch][itemId][record.HasBonus].AddRewardRecord(amount);
            }
        }
    }

    private void Combine() 
    {
        foreach (var (sourceId, reductionTemp) in CollectedData)
        {
            ProcessedData.Total += reductionTemp.Total;
            if (!ProcessedData.Sources.ContainsKey(sourceId))
                ProcessedData.Sources[sourceId] = new Reduce.ReductionSource();
            
            var sources = ProcessedData.Sources[sourceId];
            foreach (var (tier, patches) in reductionTemp.Rewards)
            {
                if (!sources.Tiers.ContainsKey(tier))
                    sources.Tiers[tier] = new Reduce.ReductionTier();

                var tiers = sources.Tiers[tier];
                foreach (var (patch, rewards) in patches)
                {
                    if (!tiers.Patches.ContainsKey(patch))
                        tiers.Patches[patch] = new Reduce.ReductionPatch();

                    var patchData = tiers.Patches[patch];
                    foreach (var (rewardId, rewardSplits) in rewards)
                    {
                        MappingHelper.AddItem(rewardId);

                        foreach (var (hasBonus, rewardData) in rewardSplits)
                        {
                            if (!hasBonus)
                            {
                                patchData.NormalCount += rewardData.Amount;
                                patchData.Normal[rewardId] = rewardData;
                            }
                            else
                            {
                                patchData.BonusCount += rewardData.Amount;
                                patchData.Bonus[rewardId] = rewardData;
                            }
                        }
                    }
                }
            }
            
            MappingHelper.AddItem(sourceId);
        }
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("Reduction.json", ProcessedData);
        Logger.Information("Done exporting data ...");
    }
}