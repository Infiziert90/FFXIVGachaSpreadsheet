using SupabaseExporter.Structures.Exports;

namespace SupabaseExporter.Processing.Reduction;

public class Reduction : IDisposable
{
    private readonly Reduce CollectedData = new();
    
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
        CollectedData.Sources.Clear();
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

            CollectedData.Records += 1;
            if (!CollectedData.Sources.ContainsKey(record.Source))
                CollectedData.Sources[record.Source] = new Reduce.ReductionSource();
            
            var reductionTemp = CollectedData.Sources[record.Source];
            reductionTemp.Records += 1;

            if (record.HasBonus)
            {
                if (reductionTemp.LowestBonus == -1 ||  reductionTemp.LowestBonus > record.Collectability)
                    reductionTemp.LowestBonus = (int)record.Collectability;
            }
            
            var reductionRow = sourceItem.AetherialReduce;
            var subrowRewards = Sheets.GathererReductionRewardSheet.GetRow(reductionRow);

            var collectabilityTier = 0u;
            var subRowList = subrowRewards.OrderBy(r => r.SubrowId).ToArray();
            var subRow = subRowList[0];
            if (record.Collectability > subRowList[^1].Unknown0)
            {
                subRow = subRowList[^1];
                collectabilityTier = subRow.SubrowId;
            }
            else
            {
                foreach (var rewardRow in subRowList)
                {
                    if (record.Collectability <= rewardRow.Unknown0)
                    {
                        subRow = rewardRow;
                        collectabilityTier = rewardRow.SubrowId;
                        break;
                    }
                }
            }

            if (!reductionTemp.Tiers.ContainsKey(collectabilityTier))
                reductionTemp.Tiers[collectabilityTier] = new Reduce.ReductionTier { Minimum = subRow.Unknown0 };
            
            var patches = reductionTemp.Tiers[collectabilityTier];
            patches.Records += 1;
            
            var patch = record.GetPatch;
            if (!patches.Patches.ContainsKey(patch))
                patches.Patches[patch] = new Reduce.ReductionPatch();
                
            var patchData = patches.Patches[patch];
            
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
                
                // Reduction rewards crystals as base reward, so we check for anything none crystal
                if (itemId > 100)
                {
                    if (reductionTemp.LowestSand == -1 ||  reductionTemp.LowestSand > record.Collectability)
                        reductionTemp.LowestSand = (int)record.Collectability;
                }
                
                if (record.HasBonus)
                {
                    patchData.BonusCount += 1;
                    if (!patchData.Bonus.ContainsKey(itemId))
                        patchData.Bonus[itemId] = new Reduce.ReductionReward();
                    
                    patchData.Bonus[itemId].AddRewardRecord(amount);
                }
                else
                {
                    patchData.NormalCount += 1;
                    if (!patchData.Normal.ContainsKey(itemId))
                        patchData.Normal[itemId] = new Reduce.ReductionReward();
                    
                    patchData.Normal[itemId].AddRewardRecord(amount);
                }
            }
        }
    }

    private void Combine() 
    {
        foreach (var (sourceId, reductionTemp) in CollectedData.Sources)
        {
            MappingHelper.AddItem(sourceId);
            
            foreach (var patches in reductionTemp.Tiers.Values)
            {
                foreach (var rewards in patches.Patches.Values)
                {
                    foreach (var rewardId in rewards.Normal.Keys)
                        MappingHelper.AddItem(rewardId);                    
                    
                    foreach (var rewardId in rewards.Bonus.Keys)
                        MappingHelper.AddItem(rewardId);
                }
            }
        }
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("Reduction.json", CollectedData);
        Logger.Information("Done exporting data ...");
    }
}