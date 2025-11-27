namespace SupabaseExporter.Structures.Temps;

/// <summary>
/// Coffer data bundled together for calculations.
/// </summary>
public class CofferTemp
{
    public long Total;
    public Dictionary<uint, ChestReward> Rewards = [];

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
        
        public void AddExisting(ChestReward other)
        {
            Amount += other.Amount;
            Total += other.Total;
            Min = Math.Min(Min, other.Min);
            Max = Math.Max(Max, other.Max);
        }
    }
    
    public void AddSimpleRecord(uint itemId, uint amount)
    {
        Total += 1;
        
        if (!Rewards.ContainsKey(itemId))
            Rewards[itemId] = new ChestReward();
                
        Rewards[itemId].AddRewardRecord(amount);
    }
    
    public void AddMultiRecord(ReadOnlySpan<uint> rewards)
    {
        // We have nothing as content, so we just skip this entry
        if (rewards.Length == 0)
            return;
        
        Total += 1;
        foreach (var itemId in rewards)
        {
            // hitting an item with ID 0 means we reached the last valid item
            if (itemId == 0)
                break;
                
            if (!Rewards.ContainsKey(itemId))
                Rewards[itemId] = new ChestReward();
                
            Rewards[itemId].AddRewardRecord(1);
        }
    }
    
    public void AddMultiRecordWithAmount(IEnumerable<(uint, uint)> rewards)
    {
        Total += 1;
        foreach (var (itemId, amount) in rewards)
        {
            if (!Rewards.ContainsKey(itemId))
                Rewards[itemId] = new ChestReward();
                
            Rewards[itemId].AddRewardRecord(amount);
        }
    }
    
    public void AddExisting(CofferTemp other)
    {
        Total += other.Total;
        foreach (var (itemId, chestReward) in other.Rewards)
        {
            if (!Rewards.ContainsKey(itemId))
                Rewards[itemId] = new ChestReward();
            
            Rewards[itemId].AddExisting(chestReward);
        }
    }
}