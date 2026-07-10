using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Desynthesis;

public class Desynthesis2 : IDisposable
{
    private Desynth2 ProcessedData = new();
    private readonly Dictionary<uint, DesynthSourceTemp> CollectedData = [];

    public void ProcessOldData(Models.DesynthesisModel[] data)
    {
        FetchOld(data);
    }
    
    public void ProcessAllData(Models.DesynthesisModel2[] data)
    {
        Logger.Information("Processing desynthesis data");
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }
    
    public void Dispose()
    {
        ProcessedData.Patches.Clear();
        CollectedData.Clear();
        GC.Collect();
    }
    
    private void FetchOld(Models.DesynthesisModel[] data)
    {
        foreach (var record in data)
        {
            if (record.ClassLevel == 0)
                continue;
            
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
                CollectedData[record.Source] = new DesynthSourceTemp();
            
            var desynthTemp = CollectedData[record.Source];
            desynthTemp.Total += 1;
            
            var patch = "7.5";
            if (!desynthTemp.Patches.ContainsKey(patch))
                desynthTemp.Patches[patch] = new DesynthSourceTemp.DesynthPatch();
            
            var patches = desynthTemp.Patches[patch];
            patches.Total += 1;
            
            Dictionary<uint, DesynthSourceTemp.DesynthReward> selectedDict;
            if (record.ClassLevel < sourceItem.LevelItem.RowId)
            {
                patches.BTotal += 1;
                selectedDict = patches.Below;
            }
            else
            {
                patches.ATotal += 1;
                selectedDict = patches.Above;
            }
            
            foreach (var (itemId, amount) in record.GetRewards())
            {
                if (!selectedDict.ContainsKey(itemId))
                    selectedDict[itemId] = new DesynthSourceTemp.DesynthReward();
                
                selectedDict[itemId].AddRewardRecord(amount);
            }
        }
    }
    
    private void Fetch(Models.DesynthesisModel2[] data)
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
                CollectedData[record.Source] = new DesynthSourceTemp();
            
            var desynthTemp = CollectedData[record.Source];
            desynthTemp.Total += 1;
            
            var patch = record.GetPatch;
            if (!desynthTemp.Patches.ContainsKey(patch))
                desynthTemp.Patches[patch] = new DesynthSourceTemp.DesynthPatch();
            
            var patches = desynthTemp.Patches[patch];
            patches.Total += 1;
            
            Dictionary<uint, DesynthSourceTemp.DesynthReward> selectedDict;
            if (record.ClassLevel < sourceItem.LevelItem.RowId)
            {
                patches.BTotal += 1;
                selectedDict = patches.Below;
            }
            else
            {
                patches.ATotal += 1;
                selectedDict = patches.Above;
            }
            
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

                // Check the amount for rewards that aren't crystal
                if (!SkipRewardChecks.Contains(sourceItem.RowId) && itemId > 100)
                {
                    if (amount >= 30)
                    {
                        Logger.Error($"Reward amount check ({amount}), ID: {record.Id}");
                        break;
                    }
                    
                    // Check item rarity
                    if (Sheets.ItemSheet.GetRow(itemId).Rarity >= 3)
                    {
                        Logger.Error($"Reward rarity check, ID: {record.Id}");
                        break;
                    }
                }
                
                if (!selectedDict.ContainsKey(itemId))
                    selectedDict[itemId] = new DesynthSourceTemp.DesynthReward();
                
                selectedDict[itemId].AddRewardRecord(amount);
            }
        }
    }

    private void Combine() 
    {
        Dictionary<string, Dictionary<uint, Desynth2.RewardHistory>> tempRewards = [];
        foreach (var (sourceId, desynthTemp) in CollectedData)
        {
            var results = new List<Reward>();
            
            var newTemp = new Dictionary<string, DesynthSourceTemp.DesynthPatch>();
            foreach (var (patch, patchData) in desynthTemp.Patches)
            {
                if (!newTemp.ContainsKey(patch))
                    newTemp[patch] = new DesynthSourceTemp.DesynthPatch();
                
                newTemp[patch].ATotal += patchData.ATotal;
                newTemp[patch].BTotal += patchData.BTotal;
                
                if (!ProcessedData.Patches.ContainsKey(patch))
                    ProcessedData.Patches[patch] = new Desynth2.Patch2();
                
                var selectedPatch = ProcessedData.Patches[patch];
                
                if (!selectedPatch.Sources.ContainsKey(sourceId))
                    selectedPatch.Sources[sourceId] = new Desynth2.SourceHistory();
                
                selectedPatch.Sources[sourceId].ARecords += patchData.ATotal;
                selectedPatch.Sources[sourceId].BRecords += patchData.BTotal;
                
                foreach (var (rewardId, rewards) in patchData.Above)
                {
                    if (!newTemp[patch].Above.ContainsKey(rewardId))
                        newTemp[patch].Above[rewardId] = new DesynthSourceTemp.DesynthReward();
                    
                    newTemp[patch].Above[rewardId].AddExisting(rewards);
                    selectedPatch.Sources[sourceId].AddAboveRecord(rewardId, patchData.ATotal, rewards);
                }
                
                foreach (var (rewardId, rewards) in patchData.Below)
                {
                    if (!newTemp[patch].Below.ContainsKey(rewardId))
                        newTemp[patch].Below[rewardId] = new DesynthSourceTemp.DesynthReward();
                    
                    newTemp[patch].Below[rewardId].AddExisting(rewards);
                    selectedPatch.Sources[sourceId].AddBelowRecord(rewardId, patchData.BTotal, rewards);
                }
            }

            foreach (var (patch, patchData) in newTemp)
            {
                if (!tempRewards.ContainsKey(patch))
                    tempRewards[patch] = new Dictionary<uint, Desynth2.RewardHistory>();
                
                foreach (var (itemId, reward) in patchData.Above)
                {
                    MappingHelper.AddItem(itemId);
                    results.Add(Reward.FromDesyntReward2(itemId, patchData.ATotal, reward));

                    if (!tempRewards[patch].ContainsKey(itemId))
                        tempRewards[patch][itemId] = new Desynth2.RewardHistory();

                    tempRewards[patch][itemId].AddRecord(sourceId, reward);
                }                
                
                foreach (var (itemId, reward) in patchData.Below)
                {
                    MappingHelper.AddItem(itemId);
                    results.Add(Reward.FromDesyntReward2(itemId, patchData.BTotal, reward));

                    if (!tempRewards[patch].ContainsKey(itemId))
                        tempRewards[patch][itemId] = new Desynth2.RewardHistory();

                    tempRewards[patch][itemId].AddRecord(sourceId, reward);
                }
            }

            MappingHelper.AddItem(sourceId);
        }

        foreach (var (patch, patchData) in tempRewards)
        {
            if (!ProcessedData.Patches.ContainsKey(patch))
                ProcessedData.Patches[patch] = new Desynth2.Patch2();
            
            foreach (var (rewardItemId, history) in patchData)
            {
                ProcessedData.Patches[patch].Rewards[rewardItemId] = new Desynth2.RewardHistory { Records = history.Records };
                foreach (var tempReward in history.Rewards)
                {
                    var source = ProcessedData.Patches[patch].Sources[tempReward.Id];
                    var aboveReward = source.Above.Find(r => r.Id == rewardItemId);
                    var belowReward = source.Below.Find(r => r.Id == rewardItemId);
                    if (aboveReward == null && belowReward == null)
                        throw new Exception($"Source for id {rewardItemId} not found!");

                    ProcessedData.Patches[patch].Rewards[rewardItemId].Rewards.Add(new Reward(
                        tempReward.Id,
                        tempReward.Amount,
                        aboveReward?.Pct ?? belowReward!.Pct,
                        aboveReward != null ? source.ARecords : source.BRecords,
                        tempReward.Min,
                        tempReward.Max
                    ));
                }
            }
        }
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("DesynthesisV2Collected.json", CollectedData);
        ExportHandler.WriteDataJson("DesynthesisV2.json", ProcessedData);
        
        // Logger.Information("Splitting data ...");
        // var desynthBase = new DesynthesisBase();
        // for (var i = 1000; i <= Sheets.MaxItemId + 1000; i += 1000)
        // {
        //     var split = new Desynth();
        //     foreach (var (sourceId, history) in ProcessedData.Sources.Where(pair => pair.Key >= i - 999 && pair.Key <= i))
        //     {
        //         desynthBase.Sources.Add(sourceId);
        //         split.Sources[sourceId] = history;
        //
        //         if (history is { Records: < 50, Rewards.Count: < 3 })
        //             continue;
        //
        //         var lastAmount = 0L;
        //         foreach (var reward in history.Rewards.Where(r => r.Id > 100).OrderByDescending(r => r.Amount))
        //         {
        //             // Fine Sand
        //             if (SkipPctCheck.Contains(reward.Id))
        //                 continue;
        //
        //             if (lastAmount == 0)
        //             {
        //                 lastAmount = reward.Amount;
        //                 continue;
        //             }
        //             
        //             var tenPerc = (long)(lastAmount * 0.05);
        //             if (tenPerc > reward.Amount)
        //                 Logger.Error($"Amount is under the 5% ({tenPerc}) threshold, check for item id, ID: {sourceId} | {reward.Id} | {reward.Amount}");
        //             else
        //                 lastAmount = reward.Amount;
        //         }
        //     }
        //     
        //     foreach (var (rewardId, history) in ProcessedData.Rewards.Where(pair => pair.Key >= i - 999 && pair.Key <= i))
        //     {
        //         desynthBase.Rewards.Add(rewardId);
        //         split.Rewards[rewardId] = history;
        //     }
        //     
        //     ExportHandler.WriteDataJson($"desynthesis/{i:D6}.json", split);
        // }
        // ExportHandler.WriteDataJson("desynthesis/base.json", desynthBase);
        
        Logger.Information("Done exporting data ...");
    }

    // Diadem, they have huge amount of reward
    private readonly HashSet<uint> SkipRewardChecks = 
    [
        30014, 30015, 30016, 30017, 30018, 30019, 30020, 30021, 30022, 30023, 30024, 30025, 30026, 30027, 30028, 30029, 30030,
        30031, 30032, 30033, // Approved Grade 2, they have huge amount of reward
        
        31604, 31605, 31606, 31607, 31608, 31609, 31610, 31611, 31612, 31613, 31614, 31615, 31616, 31617, 31618, 31619, 31620,
        31621, 31622, 31623, 31624, 31625, 31626, 31627, 31628, 31629, // Approved Grade 3, they have huge amount of reward
        
        32908, 32909, 32910, 32911, 32912, 32913, 32914, 32915, 32916, 32917, 32918, 32919, 32920, 32921, 32922, 32923, 32924, 32925,
        32926, 32927, 32928, 32929, 32930, 32931, 32932, 32933, // Approved Grade 4, they have huge amount of reward
    ];

    private readonly HashSet<uint> SkipPctCheck =
    [
        5267, // Fine Sand
        8142, // Clear Demimateria I
        8143, // Clear Demimateria II
        8144, // Clear Demimateria III
        
        8145, // Battlecraft Demimateria I
        8146, // Battlecraft Demimateria II
        8147, // Battlecraft Demimateria III
        
        8148, // Fieldcraft Demimateria I
        8149, // Fieldcraft Demimateria II
        8150, // Fieldcraft Demimateria III
        
        28062, // Nightworld Bronze Piece
        
        52712, // All-purpose Pigment
    ];
}