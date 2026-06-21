using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// Struct filled with history of reduction results.
/// </summary>
public class Reduce
{
    public long Records;
    
    public List<ReductionJob> Jobs = [];
    
    public Reduce() {}

    public class ReductionJob
    {
        public long Records;

        public uint Id;
        public string Name = string.Empty;
        
        public List<ReductionSource> Sources = [];
        
        public ReductionJob() {}

        public ReductionJob(ReduceTemp.ReductionJob job)
        {
            Records = job.Records;
            
            Id = job.Id;
            Name = job.Name;
        }
    }
    
    public class ReductionSource
    {
        public long Records;

        public uint Id;
        
        public uint MainTier;
        public uint Maximum;
        
        public int LowestSand;
        public int LowestBonus;
        
        public List<ReductionTier> Tiers = [];
        
        public ReductionSource() {}
        
        public ReductionSource(ReduceTemp.ReductionSource source)
        {
            Records = source.Records;
            
            Id = source.Id;
            
            MainTier = source.MainTier;
            Maximum = source.Maximum;
            
            LowestSand = source.LowestSand;
            LowestBonus = source.LowestBonus;
        }
    }
    
    public class ReductionTier
    {
        public long Records;

        public uint SubTier;
        
        public Dictionary<string, ReductionPatch> Patches = [];
        
        public ReductionTier() {}
        
        public ReductionTier(ReduceTemp.ReductionTier tier)
        {
            Records = tier.Records;
            
            SubTier = tier.Tier;
        }
    }

    public class ReductionPatch
    {
        public long NormalCount;
        public long BonusCount;
        
        public List<Reward> Normal = [];
        public List<Reward> Bonus = [];
        
        public ReductionPatch() {}

        public ReductionPatch(ReduceTemp.ReductionPatch patch)
        {
            NormalCount = patch.NormalCount;
            BonusCount = patch.BonusCount;
        }
    }
}