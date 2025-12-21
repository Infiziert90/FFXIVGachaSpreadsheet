namespace SupabaseExporter.Structures.Temps;

public class DesynthTemp
{
    public long Total;
    public Dictionary<uint, Dictionary<string, DesynthReward>> Rewards = [];
        
    public class DesynthReward
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

        public void AddExisting(DesynthReward other)
        {
            Amount += other.Amount;
            Total += other.Total;
            Min = Math.Min(Min, other.Min);
            Max = Math.Max(Max, other.Max);
        }
    }
}