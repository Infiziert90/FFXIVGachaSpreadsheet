using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// The base of desynthesis data.
/// </summary>
public class Desynth
{
    public Dictionary<uint, History> Sources = [];
    public Dictionary<uint, History> Rewards = [];
    
    public class History
    {
        public uint Records;
        public List<Reward> Rewards = [];

        public void AddRecord(uint itemId, DesynthTemp.DesynthReward reward)
        {
            Records += (uint)reward.Amount;
            Rewards.Add(Reward.FromDesyntReward(itemId, 10000, reward)); // Total is unknown at this point, so give it a fake value
        }
    }
}