using Newtonsoft.Json;

namespace SupabaseExporter.Structures;

[Serializable]
public class DutyLootStruct(string name, uint category)
{
    public string Name = name;
    public uint Category = category;
    
    public List<DutyExpansion> Expansions = [];
    
    [JsonIgnore]
    public Dictionary<uint, DutyExpansion> InternalExpansions = [];

    public class DutyExpansion(string name, uint category)
    {
        public string Name = name;
        public uint Category = category;
        
        public List<DutyTitle> Titles = [];
        
        [JsonIgnore]
        public Dictionary<uint, DutyTitle> InternalTitles = [];
    }
    
    public class DutyTitle(string name, uint category)
    {
        public string Name = name;
        public uint Category = category;
        
        public List<Duty> Duties = [];
    }
    
    public class Duty(DutyLootTemp temp)
    {
        public int Records = temp.Records;
    
        public uint DutyId = temp.DutyId;
        public string DutyName = temp.DutyName;
        public uint SortKey = temp.SortKey;
    
        public List<ChestLoot> Chests = [];
    }

    public class ChestLoot(DutyLootTemp.Chest chest)
    {
        public int Records = chest.Records;
        
        public uint ChestId = chest.ChestId;
        public string ChestName = chest.ChestName;
        
        public uint MapId = chest.MapId;
        public uint TerritoryId = chest.TerritoryId;
        public string PlaceNameSub = chest.PlaceNameSub;
        
        public List<Reward> Rewards = [];

        public void AddReward(uint itemId, DutyLootTemp.ChestReward reward)
        {
            var item = Sheets.ItemSheet.GetRow(itemId);
            Rewards.Add(Reward.FromDutyLoot(item, Records, reward));
            
            IconHelper.AddItem(item);
        }
    }
}

public class DutyLootTemp(uint dutyId, string name, uint category, string expansion, uint expansionKey, string uiCategory, uint uiCategoryKey, uint sort)
{
    public int Records;
    public Dictionary<uint, Chest> Chests = [];

    public uint DutyId = dutyId;
    public string DutyName = name;
    public uint DutyCategory = category;

    public string Expansion = expansion;
    public uint ExpansionKey = expansionKey;
    
    public string UICategory = uiCategory.Split("(")[0].TrimEnd();
    public uint UICategoryKey = uiCategoryKey;
    
    public uint SortKey = sort;

    public class Chest(uint chestId, string chestName, uint territory, uint map, string placeNameSub)
    {
        public int Records;
        
        public uint MapId = map;
        public uint TerritoryId = territory;
        public string PlaceNameSub = placeNameSub;
        
        public uint ChestId = chestId;
        public string ChestName = chestName;
        public Dictionary<uint, ChestReward> Rewards = [];
    }
    
    public class ChestReward
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
    }
}

public class DutyLoot : IDisposable
{
    private Dictionary<string, Models.DutyLoot> HashedCache = [];
    
    private readonly Dictionary<uint, DutyLootStruct> ProcessedData = [];
    private readonly Dictionary<uint, DutyLootTemp> CollectedData = [];
    
    public void ProcessAllData(List<Models.DutyLoot> data)
    {
        Console.WriteLine("Processing duty loot data");
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }
    
    public void Dispose()
    {
        HashedCache.Clear();
        ProcessedData.Clear();
        CollectedData.Clear();
        GC.Collect();
    }

    private void Fetch(List<Models.DutyLoot> data)
    {
        foreach (var record in data)
        {
            if (!HashedCache.TryAdd(record.Hashed, record))
                if (HashedCache[record.Hashed].GetContent().SequenceEqual(record.GetContent()))
                    continue;

            if (!Sheets.TerritoryTypeSheet.TryGetRow(record.Territory, out var territoryType))
            {
                Console.Error.WriteLine($"Invalid territory found, ID: {record.Id}");
                continue;
            }
            
            if (!Sheets.MapSheet.TryGetRow(record.Map, out var map))
            {
                Console.Error.WriteLine($"Invalid map found, ID: {record.Id}");
                continue;
            }
            
            if (!Sheets.TreasureSheet.TryGetRow(record.ChestId, out var treasure))
            {
                Console.Error.WriteLine($"Invalid treasure found, ID: {record.Id}");
                continue;
            }
            
            // Check if it is a non-duty drop (open world treasure hunt e.g.)
            var contentRowId = territoryType.ContentFinderCondition.RowId;
            if (contentRowId == 0)
                contentRowId = territoryType.RowId + 100_000;
            
            if (!CollectedData.TryGetValue(contentRowId, out var dutyLoot))
            {
                var expansionId = territoryType.ExVersion.RowId;
                var expansionName = Utils.UpperCaseStr(territoryType.ExVersion.Value.Name);
                if (contentRowId < 100_000)
                {
                    var cfd = territoryType.ContentFinderCondition.Value;
                    dutyLoot = new DutyLootTemp(contentRowId, Utils.UpperCaseStr(cfd.Name), cfd.ContentType.RowId, expansionName, expansionId, Utils.UpperCaseStr(cfd.ContentUICategory.Value.Name), cfd.ContentUICategory.RowId, cfd.SortKey);
                }
                else
                {
                    dutyLoot = new DutyLootTemp(contentRowId, Utils.UpperCaseStr(territoryType.PlaceName.Value.Name), 100_000, expansionName, expansionId, "", 0, 0);
                }
            }
            
            if (!dutyLoot.Chests.TryGetValue(record.ChestId, out var chest))
                chest = new DutyLootTemp.Chest(record.ChestId, Utils.UpperCaseStr(treasure.Unknown0), record.Territory, record.Map, Utils.UpperCaseStr(map.PlaceNameSub.Value.Name));
            
            dutyLoot.Records++;
            chest.Records++;
            
            var content = record.GetContent();
            for (var i = 0; i < content.Length / 2; i++)
            {
                var itemId = content[2 * i];
                var amount = content[(2 * i) + 1];
                
                // hitting an item with ID 0 means we reached the last valid item
                if (itemId == 0)
                    break;
            
                if (!chest.Rewards.ContainsKey(itemId))
                    chest.Rewards[itemId] = new DutyLootTemp.ChestReward();
                
                chest.Rewards[itemId].AddRewardRecord(amount);
            }
            
            dutyLoot.Chests[record.ChestId] = chest;
            CollectedData[contentRowId] = dutyLoot;
        }
    }

    private void Combine() 
    {
        foreach (var dutyLoot in CollectedData.Values)
        {
            var dutyCategoryName = "Open World";
            if (Sheets.ContentTypeSheet.TryGetRow(dutyLoot.DutyCategory, out var categoryRow))
                dutyCategoryName = Utils.UpperCaseStr(categoryRow.Name);
            
            if (!ProcessedData.ContainsKey(dutyLoot.DutyCategory))
                ProcessedData[dutyLoot.DutyCategory] = new DutyLootStruct(dutyCategoryName, dutyLoot.DutyCategory);
            var selectedCategory = ProcessedData[dutyLoot.DutyCategory];
            
            if (!selectedCategory.InternalExpansions.ContainsKey(dutyLoot.ExpansionKey))
                selectedCategory.InternalExpansions[dutyLoot.ExpansionKey] = new DutyLootStruct.DutyExpansion(dutyLoot.Expansion, dutyLoot.ExpansionKey);
            var selectedExpansion = selectedCategory.InternalExpansions[dutyLoot.ExpansionKey];
            
            if (!selectedExpansion.InternalTitles.ContainsKey(dutyLoot.UICategoryKey))
                selectedExpansion.InternalTitles[dutyLoot.UICategoryKey] = new DutyLootStruct.DutyTitle(dutyLoot.UICategory, dutyLoot.UICategoryKey);
            var selectedTitle = selectedExpansion.InternalTitles[dutyLoot.UICategoryKey];
            
            var finalDutyLoot = new DutyLootStruct.Duty(dutyLoot);
            foreach (var chestLoot in dutyLoot.Chests.Values.OrderBy(c => c.ChestId))
            {
                var lootContainer = new DutyLootStruct.ChestLoot(chestLoot);
                foreach (var (itemId, reward) in chestLoot.Rewards)
                    lootContainer.AddReward(itemId, reward);
                
                finalDutyLoot.Chests.Add(lootContainer);
            }
    
            finalDutyLoot.Chests = finalDutyLoot.Chests.OrderBy(c => c.MapId).ThenBy(c => c.ChestId).ToList();
            selectedTitle.Duties.Add(finalDutyLoot);
        }

        foreach (var dutyLoot in ProcessedData.Values)
        {
            dutyLoot.Expansions = dutyLoot.InternalExpansions.Values.OrderBy(e => e.Category).ToList();
            foreach (var expansion in dutyLoot.Expansions)
            {
                expansion.Titles = expansion.InternalTitles.Values.OrderBy(e => e.Category).ToList();
                foreach (var title in expansion.Titles)
                    title.Duties = title.Duties.OrderBy(t => t.SortKey).ToList();
            }
        }
    }
    
    private void Export()
    {
        Console.WriteLine("Start export of processed duty loot data ...");
        ExportHandler.WriteDataJson("DutyLoot.json", ProcessedData.Values.OrderBy(d => d.Category));
        Console.WriteLine("Done ...");
    }
}