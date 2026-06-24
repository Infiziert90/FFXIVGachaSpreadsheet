using Lumina.Excel.Sheets;
using Lumina.Extensions;
using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Reduction;

public class Reduction : IDisposable
{
    private readonly ReduceTemp CollectedData = new();
    private readonly Reduce ProcessData = new();
    
    private readonly Dictionary<uint, ClassJob> ItemToJob = new();
    
    private static readonly HashSet<uint> SpecialItems = // These items get rewarded randomly upon harvesting others
    [
        37695, // Sublime Siderite
        37692, // Sublime Crystalbloom
        39235, // Sublime Sphongos
        39238, // Sublime Achondrite
        39907, // Sublime Haritaki
        39910, // Sublime Chloroschist
        41417, // Sublime Fossilized Dragon's Scale
    ];
    
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
        CollectedData.Jobs.Clear();
        ProcessData.Jobs.Clear();
        ItemToJob.Clear();
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

            ClassJob job;
            
            var sourceId = SpecialItems.Contains(record.Source) ? record.Source - 1 : record.Source;
            if (ItemToJob.TryGetValue(sourceId, out var value))
            {
                job = value;
            }
            else if (Sheets.FishParameterSheet.TryGetFirst(f => f.Item.RowId == sourceId, out _))
            {
                job = Sheets.ClassJobSheet.GetRow(18);
                ItemToJob[sourceId] = job;
            }
            else
            {
                var itemRowId = 0u;
                if (Sheets.GathererItemSheet.TryGetFirst(g => g.Item.RowId == sourceId, out var gatheringItem))
                {
                    itemRowId = gatheringItem.RowId;
                }
                else if (Sheets.SpearfishingItemSheet.TryGetFirst(s => s.Item.RowId == sourceId, out var spearfishingItem))
                {
                    itemRowId = spearfishingItem.RowId;
                }

                if (itemRowId == 0)
                {
                    Logger.Error($"Unable to find base item: {record.Id}");
                    continue;
                }

                if (!Sheets.GatheringPointBaseSheet.TryGetFirst(g => FindMatchingItem(g, gatheringItem.RowId), out var gatheringBase))
                {
                    Logger.Error($"Source not found in GatheringBase? ID: {record.Id} ItemId: {record.Source}");
                    continue;
                }

                job = ((ReductionJobs)gatheringBase.GatheringType.RowId).ToJob();
                ItemToJob[sourceId] = job;
            }
            
            CollectedData.Records += 1;
            if (!CollectedData.Jobs.ContainsKey(job.RowId))
                CollectedData.Jobs[job.RowId] = new ReduceTemp.ReductionJob { Id = job.RowId, Name = job.NameEnglish.ToString() };
            
            var jobs = CollectedData.Jobs[job.RowId];
            jobs.Records += 1;
            
            if (!jobs.Sources.ContainsKey(record.Source))
                jobs.Sources[record.Source] = new ReduceTemp.ReductionSource { Id = record.Source, MainTier = sourceItem.AetherialReduce };
            
            var reductionTemp = jobs.Sources[record.Source];
            reductionTemp.Records += 1;
            reductionTemp.Maximum = Math.Max(reductionTemp.Maximum, record.Collectability);
            if (record.HasBonus)
                reductionTemp.LowestBonus = Math.Min(reductionTemp.LowestBonus, (int)record.Collectability);
            
            var reductionRow = sourceItem.AetherialReduce;
            var subrowRewards = Sheets.GathererReductionRewardSheet.GetRow(reductionRow);

            var collectabilityTier = 0u;
            var subRowList = subrowRewards.OrderBy(r => r.SubrowId).ToArray();
            for (var idx = 0; idx < subRowList.Length; idx++)
            {
                // End reached, it must be the last tier
                if (idx + 1 == subRowList.Length)
                {
                    collectabilityTier = subRowList[^1].SubrowId;
                    break;
                }

                var currentRow = subRowList[idx];
                var nextRow = subRowList[idx + 1];
                if (record.Collectability < nextRow.Unknown0)
                {
                    collectabilityTier = currentRow.SubrowId;
                    break;
                }
            }
            
            if (!reductionTemp.Tiers.ContainsKey(collectabilityTier))
                reductionTemp.Tiers[collectabilityTier] = new ReduceTemp.ReductionTier { Tier = collectabilityTier };
            
            var patches = reductionTemp.Tiers[collectabilityTier];
            patches.Records += 1;
            
            var patch = record.GetPatch;
            if (!patches.Patches.ContainsKey(patch))
                patches.Patches[patch] = new ReduceTemp.ReductionPatch();
                
            var patchData = patches.Patches[patch];
            if (record.HasBonus)
                patchData.BonusCount += 1;
            else
                patchData.NormalCount += 1;
            
            
            foreach (var (itemId, quantity) in record.GetRewards())
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
                    reductionTemp.LowestSand = Math.Min(reductionTemp.LowestSand, (int)record.Collectability);
                
                if (record.HasBonus)
                {
                    if (!patchData.Bonus.ContainsKey(itemId))
                        patchData.Bonus[itemId] = new ReduceTemp.ReductionReward();
                    
                    patchData.Bonus[itemId].AddRewardRecord(quantity);
                }
                else
                {
                    if (!patchData.Normal.ContainsKey(itemId))
                        patchData.Normal[itemId] = new ReduceTemp.ReductionReward();
                    
                    patchData.Normal[itemId].AddRewardRecord(quantity);
                }
            }
        }
    }

    private void Combine()
    {
        ProcessData.Records = CollectedData.Records;
        foreach (var jobTemp in CollectedData.Jobs.Values)
        {
            var job = new Reduce.ReductionJob(jobTemp);
            
            foreach (var sourceTemp in jobTemp.Sources.Values)
            {
                var source = new Reduce.ReductionSource(sourceTemp);
                MappingHelper.AddItem(sourceTemp.Id);

                foreach (var tierTemp in sourceTemp.Tiers.Values)
                {
                    var tier = new Reduce.ReductionTier(tierTemp);
                    
                    foreach (var (patchId, patchTemp) in tierTemp.Patches)
                    {
                        var patch = new Reduce.ReductionPatch(patchTemp);
                        
                        foreach (var (rewardId, rewardTemp) in patchTemp.Normal)
                        {
                            patch.Normal.Add(Reward.FromReduceReward(rewardId, patchTemp.NormalCount, rewardTemp));
                            MappingHelper.AddItem(rewardId);
                        }

                        foreach (var (rewardId, rewardTemp) in patchTemp.Bonus)
                        {
                            patch.Bonus.Add(Reward.FromReduceReward(rewardId, patchTemp.BonusCount, rewardTemp));
                            MappingHelper.AddItem(rewardId);
                        }
                        
                        patch.Normal = patch.Normal.OrderBy(r => r.Id).ToList();
                        patch.Bonus = patch.Bonus.OrderBy(r => r.Id).ToList();
                        tier.Patches.Add(patchId, patch);
                    }
                    
                    source.Tiers.Add(tier);
                }
                
                source.Tiers = source.Tiers.OrderBy(t => t.SubTier).ToList();
                job.Sources.Add(source);
            }
            
            job.Sources = job.Sources.OrderBy(s => s.Id).ToList();
            ProcessData.Jobs.Add(job);
        }
        
        ProcessData.Jobs = ProcessData.Jobs.OrderBy(s => s.Id).ToList();
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("Reduction.json", ProcessData);
        Logger.Information("Done exporting data ...");
    }

    private bool FindMatchingItem(GatheringPointBase row, uint itemRowId)
        => row.Item.Any(rowRef => rowRef.RowId == itemRowId);
}