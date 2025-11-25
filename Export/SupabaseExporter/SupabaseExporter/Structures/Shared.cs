using Lumina.Excel.Sheets;
using Newtonsoft.Json;

namespace SupabaseExporter.Structures;

/// <summary>
/// Reward of a given task, e.g., a venture item, or a coffer item.
/// </summary>
[Serializable]
public record Reward(uint Id, long Amount, [property: JsonConverter(typeof(LessPrecisionDouble))] double Pct, long Total = 0, long Min = 0, long Max = 0)
{
    public static Reward FromTaskReward(Item item, long total, VentureTemp.TaskReward taskReward)
    {
        return new Reward(
            item.RowId,
            taskReward.Amount,
            taskReward.Amount / (double)total,
            taskReward.Total,
            taskReward.Min,
            taskReward.Max);
    }
    
    public static Reward FromDutyLoot(Item item, long total, DutyLootTemp.ChestReward reward)
    {
        return new Reward(
            item.RowId,
            reward.Amount,
            reward.Amount / (double)total,
            reward.Total,
            reward.Min,
            reward.Max);
    }
    
    public static Reward FromCofferReward(Item item, long total, CofferTemp.ChestReward reward)
    {
        return new Reward(
            item.RowId,
            reward.Amount,
            reward.Amount / (double)total,
            reward.Total,
            reward.Min,
            reward.Max);
    }

    public static Reward FromDesynthesisReward(uint itemId, long total, DesynthTemp.DesynthReward reward)
    {
        return new Reward(
            itemId, 
            reward.Amount, 
            reward.Amount / (double)total, 
            0, 
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