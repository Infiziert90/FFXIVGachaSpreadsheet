namespace SupabaseExporter.Structures.Temps;

/// <summary>
/// Struct filled with history of reduction results.
/// </summary>
public class ReduceTemp
{
    public long Records;
    
    public Dictionary<uint, ReductionJob> Jobs = [];

    public class ReductionJob
    {
        public long Records;
        
        public uint Id;
        public string Name = string.Empty;
        
        public Dictionary<uint, ReductionSource> Sources = [];
    }
    
    public class ReductionSource
    {
        public long Records;
        
        public uint Id;
        
        public uint MainTier;
        public uint Maximum;
        
        public int LowestSand = 10000;
        public int LowestBonus = 10000;
        
        public Dictionary<uint, ReductionTier> Tiers = [];
    }
    
    public class ReductionTier
    {
        public long Records;

        public uint Tier;
        
        public Dictionary<string, ReductionPatch> Patches = [];
    }

    public class ReductionPatch
    {
        public long NormalCount;
        public long BonusCount;
        
        public Dictionary<uint, ReductionReward> Normal = [];
        public Dictionary<uint, ReductionReward> Bonus = [];
    }
        
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
    }
}