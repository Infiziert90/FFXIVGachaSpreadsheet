namespace SupabaseExporter.Structures.Temps;

public class ChestDropTemp(uint dutyId, string name, uint category, string expansion, uint expansionKey, string uiCategory, uint uiCategoryKey, uint sort)
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