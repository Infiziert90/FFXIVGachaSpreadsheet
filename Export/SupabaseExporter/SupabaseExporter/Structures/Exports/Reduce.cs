using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// Struct filled with history of reduction results.
/// </summary>
public class Reduce
{
    public long Total;
    public Dictionary<uint, ReductionSource> Sources = [];
    
    // Source -> Tier -> Patch -> Rewards
    
    public class ReductionSource
    {
        public Dictionary<uint, ReductionTier> Tiers = [];
    }
    
    public class ReductionTier
    {
        public Dictionary<string, ReductionPatch> Patches = [];
    }

    public class ReductionPatch
    {
        public Dictionary<uint, ReductionTemp.ReductionReward> Rewards = [];
    }
}