namespace SupabaseExporter.Structures.Temps;

/// <summary>
/// Venture task data bundled together for calculations.
/// </summary>
public class VentureTemp(uint type)
{
    public uint Type = type;
    public long Total;
    public readonly Dictionary<uint, TaskReward> PrimaryRewards = [];
    public readonly Dictionary<uint, TaskReward> AdditionalRewards = [];
    
    public void AddVentureRecord(Models.VentureModel venture)
    {
        Total += 1;
        
        if (!PrimaryRewards.ContainsKey(venture.PrimaryId))
            PrimaryRewards[venture.PrimaryId] = new TaskReward();

        PrimaryRewards[venture.PrimaryId].AddRewardRecord(venture.PrimaryCount);

        if (!AdditionalRewards.ContainsKey(venture.AdditionalId))
            AdditionalRewards[venture.AdditionalId] = new TaskReward();
        
        AdditionalRewards[venture.AdditionalId].AddRewardRecord(venture.AdditionalCount);
    }
    
    public class TaskReward
    {
        public long Amount;
        public long Total;
        public long Min = long.MaxValue;
        public long Max = long.MinValue;

        public void AddRewardRecord(short quantity)
        {
            Amount += 1;
            Total += quantity;
            Min = Math.Min(Min, quantity);
            Max = Math.Max(Max, quantity);
        }

        public void AddExisting(TaskReward other)
        {
            Amount += other.Amount;
            Total += other.Total;
            Min = Math.Min(Min, other.Min);
            Max = Math.Max(Max, other.Max);
        }
    }
}