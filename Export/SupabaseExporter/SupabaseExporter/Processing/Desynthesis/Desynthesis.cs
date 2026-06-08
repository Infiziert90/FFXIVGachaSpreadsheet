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
                if (!Validated.Contains(record.Id))
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
                            Logger.Error($"Reward amount check, ID: {record.Id}");
                            break;
                        }
                        
                        // Check item rarity
                        if (Sheets.ItemSheet.GetRow(itemId).Rarity >= 3)
                        {
                            Logger.Error($"Reward rarity check, ID: {record.Id}");
                            break;
                        }
                    }
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
                var source = ProcessedData.Sources[tempReward.Id];
                var sourceReward = source.Rewards.Find(r => r.Id == rewardItemId);
                if (sourceReward == null)
                    throw new Exception($"Source for id {rewardItemId} not found!");
                
                ProcessedData.Rewards[rewardItemId].Rewards.Add(new Reward(
                    tempReward.Id,
                    tempReward.Amount,
                    sourceReward.Pct,
                    source.Records,
                    tempReward.Min,
                    tempReward.Max
                ));
            }
        }
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("Desynthesis.json", ProcessedData);
        
        Logger.Information("Splitting data ...");
        var desynthBase = new DesynthesisBase();
        for (var i = 1000; i <= Sheets.MaxItemId; i += 1000)
        {
            var split = new Desynth();
            foreach (var (sourceId, history) in ProcessedData.Sources.Where(pair => pair.Key >= i - 999 && pair.Key <= i))
            {
                desynthBase.Sources.Add(sourceId);
                split.Sources[sourceId] = history;

                if (history is { Records: < 50, Rewards.Count: < 3 })
                    continue;

                var lastAmount = 0L;
                foreach (var reward in history.Rewards.Where(r => r.Id > 100).OrderByDescending(r => r.Amount))
                {
                    // Fine Sand
                    if (SkipPctCheck.Contains(reward.Id))
                        continue;

                    if (lastAmount == 0)
                    {
                        lastAmount = reward.Amount;
                        continue;
                    }
                    
                    var tenPerc = (long)(lastAmount * 0.05);
                    if (tenPerc > reward.Amount)
                        Logger.Error($"Amount is under the 5% ({tenPerc}) threshold, check for item id, ID: {sourceId} | {reward.Id} | {reward.Amount}");
                    else
                        lastAmount = reward.Amount;
                }
            }
            
            foreach (var (rewardId, history) in ProcessedData.Rewards.Where(pair => pair.Key >= i - 999 && pair.Key <= i))
            {
                desynthBase.Rewards.Add(rewardId);
                split.Rewards[rewardId] = history;
            }
            
            ExportHandler.WriteDataJson($"desynthesis/{i:D6}.json", split);
        }
        ExportHandler.WriteDataJson("desynthesis/base.json", desynthBase);
        
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
    ];
    
    private readonly HashSet<uint> Validated = 
    [
        3195, 4579, 5792, 94466, 111009, 139798, 222055, 251105, 274262, 274448, 305782, 308649, 372191, 554983, 583629, 
        587427, 624372, 708427, 708429, 708433, 754177, 891894, 904096, 919361, 1032585, 1036065, 1242576, 1252074, 1259739, 
        1259743, 1262525, 1267860, 1339287, 1376621, 1414005, 1418164, 1437586, 1493446, 1539121, 1737904, 1858371, 1868442, 
        1983144, 1985322, 2051868, 2096041, 2229170, 2229254, 2273526, 2404428, 2426258, 2426297, 2426305, 2452372, 2478925, 
        2478930, 2478937, 2478955, 2478975, 2519354, 2521668, 2565602, 2608772, 2638430, 2739317, 2798781, 2807429, 2917951, 
        2917954, 2925117, 2925126, 2925176, 2925699, 2926154, 2926718, 2926750, 3241508, 3441455, 3594558, 3755256, 3774266, 
        3808281, 3810907, 3888329, 4001604, 4006921, 4006946, 4031986, 4126535, 4130484, 4210206, 4266065, 4268978, 4275898, 
        4303477, 4325748, 4330920, 4330951, 4337870, 4338445, 4338478, 4338819, 4372822, 4373105, 4373106, 4373108, 4373109, 
        4386256, 4412654, 4444343, 4503650, 4533397, 4558916, 4558940, 4625574, 4655490, 4683369, 4782602, 4786622, 4788517, 
        4794775, 4794777, 4794795, 4794855, 4797010, 4797087, 4797088, 4902829, 4986089, 4986091, 5058722, 5059408, 5077052, 
        5156957, 5307510, 5418743, 5501500, 5507464, 5512374, 5552485, 5604101, 5604102, 5737885, 5764591, 5775619, 5776170, 
        5782028, 5799319, 5864372, 5885550, 5886195, 5886224, 5886381, 5886387, 5887709, 5887726, 5925208, 5960924, 5960939, 
        5962172, 5984298, 6001904, 6041161, 6045523, 6045737, 6084663, 6371165, 6371174, 6371178, 6371261, 6394405, 6432569, 
        6440805, 6459746, 6484277, 6484317, 6548801, 6578815, 6643065, 6684211, 6698031, 6755446, 6755448, 6755452, 6876457, 
        7051355, 7131502, 7133494, 7152248, 7155324, 7171525, 7178726, 7254932, 7287623, 7359069, 7360338, 7404684, 7447713, 
        7448423, 7501972, 7620467, 7685635, 7753584, 7811471, 7830781, 7916820, 8049807, 8095339, 8118319, 8128342, 8145383, 
        8215846, 8268072, 8288856, 8336476, 8420406, 8685783, 8812922, 8833214, 8853936, 8881669, 9115821, 9122698, 9230165, 
        9245028, 9356109, 9412262, 9524338, 9555096, 9786376, 9886256, 9902796, 10008211, 10196306, 10411744, 10452131
    ];
}