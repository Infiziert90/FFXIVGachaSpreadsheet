using Newtonsoft.Json;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// The base of chest drop data.
/// </summary>
[Serializable]
public class ChestDrop(string name, uint category)
{
    public string Name = name;
    public uint Category = category;
    
    public List<Expansion> Expansions = [];
    
    [JsonIgnore]
    public Dictionary<uint, Expansion> InternalExpansions = [];

    public class Expansion(string name, uint category)
    {
        public string Name = name;
        public uint Category = category;
        
        public List<Title> Titles = [];
        
        [JsonIgnore]
        public Dictionary<uint, Title> InternalTitles = [];
    }
    
    public class Title(string name, uint category)
    {
        public string Name = name;
        public uint Category = category;
        
        public List<Duty> Duties = [];
    }

    public class Duty
    {
        public int Records;

        public uint Id;
        public string Name;
        public uint SortKey;

        public List<Chest> Chests = [];

        public Duty() { }
        
        public Duty(ChestDropTemp temp)
        {
            Records = temp.Records;
            Id = temp.DutyId;
            Name = temp.DutyName;
            SortKey = temp.SortKey;
        }
    }

    public class Chest
    {
        public int Records;

        public uint Id;
        public string Name;

        public uint MapId;
        public uint TerritoryId;
        public string PlaceNameSub;

        public List<Reward> Rewards = [];
        
        public Chest() { }

        public Chest(ChestDropTemp.Chest chest)
        {
            Records = chest.Records;
            Id = chest.ChestId;
            Name = chest.ChestName;
            MapId = chest.MapId;
            TerritoryId = chest.TerritoryId;
            PlaceNameSub = chest.PlaceNameSub;
        }

        public void AddReward(uint itemId, ChestDropTemp.ChestReward reward)
        {
            Rewards.Add(Reward.FromDutyLoot(itemId, Records, reward));
            MappingHelper.AddItem(itemId);
        }
    }
}