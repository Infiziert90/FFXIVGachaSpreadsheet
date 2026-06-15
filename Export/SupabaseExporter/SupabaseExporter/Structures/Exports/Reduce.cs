using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// Struct filled with history of reduction results.
/// </summary>
public class Reduce
{
    public long Total;
    public Dictionary<uint, ReductionSource> Sources = [];
    
    public class ReductionSource
    {
        public long Total;
        
        public Dictionary<uint, ReductionTier> Tiers = [];
    }
    
    public class ReductionTier
    {
        public long Total;
        
        public Dictionary<string, ReductionPatch> Patches = [];
    }

    public class ReductionPatch
    {
        public long NormalCount;
        public long BonusCount;
        
        public Dictionary<uint, ReductionTemp.ReductionReward> Normal = [];
        public Dictionary<uint, ReductionTemp.ReductionReward> Bonus = [];
    }
}