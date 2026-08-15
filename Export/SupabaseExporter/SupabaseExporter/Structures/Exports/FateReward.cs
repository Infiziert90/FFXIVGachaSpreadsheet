namespace SupabaseExporter.Structures.Exports;

public class FateReward
{
    public List<Territory> Territories = [];
    
    public class Territory
    {
        public uint Id;
        public long Records;

        public List<Type> Types = [];
    }

    public class Type
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
        public List<Reward> AdditionalRewards = [];
    }
}