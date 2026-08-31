namespace SupabaseExporter.Structures.Exports;

public class FateReward
{
    public List<Expansion> Expansions = [];

    public class Expansion
    {
        public uint Id;
        public long Records;

        public List<Territory> Territories = [];
    }
    
    public class Territory
    {
        public uint Id;
        public long Records;

        public List<FateType> FateTypes = [];
    }

    public class FateType
    {
        public byte Id;
        public long Records;

        public List<Fate> Fates = [];
    }

    public class Fate
    {
        public uint Id;
        public long Records;
        
        public List<Reward> Rewards = [];
    }
}