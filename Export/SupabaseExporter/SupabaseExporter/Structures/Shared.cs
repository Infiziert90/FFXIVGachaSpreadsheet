using Lumina.Excel.Sheets;

namespace SupabaseExporter.Structures;

/// <summary>
/// Reward of a given task, e.g. a venture item, or a coffer item.
/// </summary>
[Serializable]
public record Reward(string Name, uint Id, long Amount, double Percentage, long Total = 0, long Min = 0, long Max = 0)
{
    public static Reward FromTaskReward(Item item, long taskTotal, VentureTemp.TaskReward taskReward)
    {
        return new Reward(
            item.Name.ExtractText(),
            item.RowId,
            taskReward.Amount,
            taskReward.Amount / (double)taskTotal,
            taskReward.Total,
            taskReward.Min,
            taskReward.Max);
    }
    
    public static Reward FromDutyLoot(Item item, long chestTotal, DutyLootTemp.ChestReward reward)
    {
        return new Reward(
            item.Name.ExtractText(),
            item.RowId,
            reward.Amount,
            reward.Amount / (double)chestTotal,
            reward.Total,
            reward.Min,
            reward.Max);
    }
    
    public static Reward FromCofferReward(Item item, long chestTotal, CofferTemp.ChestReward reward)
    {
        return new Reward(
            item.Name.ExtractText(),
            item.RowId,
            reward.Amount,
            reward.Amount / (double)chestTotal,
            reward.Total,
            reward.Min,
            reward.Max);
    }
}

/// <summary>
/// Used as a JSON structure for export.
/// </summary>
[Serializable]
public record CofferData
{
    public string Name;
    public uint Territory;
    
    public List<CofferVariant> Coffers = [];

    public CofferData(uint territory, string name, List<CofferVariant> coffers)
    {
        Name = name;
        Territory = territory;
        
        Coffers = coffers;
    }

    public record CofferVariant(uint CofferId, string CofferName)
    {
        public uint CofferId = CofferId;
        public string CofferName = CofferName;

        public Dictionary<string, CofferContent> Patches = [];
    }
    
    public record CofferContent(long Total, List<Reward> Items)
    {
        public long Total = Total;
        public List<Reward> Items = Items;
    }
}

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
    
    public void AddMultiRecordWithAmount(ReadOnlySpan<uint> rewards)
    {
        Total += 1;
        for (var i = 0; i < rewards.Length / 2; i++)
        {
            var itemId = rewards[2 * i];
            var amount = rewards[(2 * i) + 1];
                
            // hitting an item with ID 0 means we reached the last valid item
            if (itemId == 0)
                break;
                
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