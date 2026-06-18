using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// Struct filled with history of reduction results.
/// </summary>
public class Reduce
{
    public long Records;
    
    public Dictionary<uint, ReductionSource> Sources = [];
    
    public class ReductionSource
    {
        public long Records;
        
        public int LowestSand = -1;
        public int LowestBonus = -1;
        
        public Dictionary<uint, ReductionTier> Tiers = [];
    }
    
    public class ReductionTier
    {
        public long Records;
        public uint Minimum;
        
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