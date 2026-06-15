namespace SupabaseExporter.Structures.Temps;

public class ReductionTemp
{
    public long Total;
    public Dictionary<uint, Dictionary<string, Dictionary<uint, ReductionReward>>> Rewards = [];
    // Tiers -> Patches -> Rewards
        
    public class ReductionReward
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

        public void AddExisting(ReductionReward other)
        {
            Amount += other.Amount;
            Total += other.Total;
            Min = Math.Min(Min, other.Min);
            Max = Math.Max(Max, other.Max);
        }
    }
}