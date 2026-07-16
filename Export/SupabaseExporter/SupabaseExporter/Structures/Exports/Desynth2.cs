using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// Struct filled with history of desynthesis results.
/// </summary>
public class Desynth2
{
    public Dictionary<string, Patch2> Patches = [];

    public class Patch2
    {
        public uint Records;
        public Dictionary<uint, SourceHistory> Sources = [];
        public Dictionary<uint, RewardHistory> Rewards = [];
    }
    
    public class SourceHistory
    {
        public uint ILvl;
        public uint Job;
        
        public uint A;
        public List<Reward> Above = [];
        
        public uint B;
        public List<Reward> Below = [];

        public void AddBelowRecord(uint itemId, uint desynthTotal, DesynthSourceTemp.DesynthReward reward)
        {
            Below.Add(Reward.FromDesyntReward2(itemId, desynthTotal, reward));
        }        
        
        public void AddAboveRecord(uint itemId, uint desynthTotal, DesynthSourceTemp.DesynthReward reward)
        {
            Above.Add(Reward.FromDesyntReward2(itemId, desynthTotal, reward));
        }
    }
    
    public class RewardHistory
    {
        public uint Records;
        public List<Reward> Rewards = [];

        public void AddRecord(uint itemId, DesynthSourceTemp.DesynthReward reward)
        {
            Records += (uint)reward.Amount;
            Rewards.Add(Reward.FromDesyntReward2(itemId, 10000, reward)); // Total is unknown at this point, so give it a fake value
        }
    }
}

/// <summary>
/// The base of desynthesis data.
/// </summary>
public class DesynthesisBase2
{
    public HashSet<uint> Sources = [];
    public HashSet<uint> Rewards = [];
}