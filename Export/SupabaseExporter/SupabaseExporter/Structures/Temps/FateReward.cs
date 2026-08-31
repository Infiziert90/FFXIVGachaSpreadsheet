namespace SupabaseExporter.Structures.Exports;

public class FateRewardTemp
{
    public Dictionary<uint, Expansion> Expansions = [];

    public class Expansion(uint id)
    {
        public uint Id = id;
        public long Records;

        public Dictionary<uint, Territory> Territories = [];
    }
    
    public class Territory(uint id)
    {
        public uint Id = id;
        public long Records;

        public Dictionary<uint, FateType> FateTypes = [];
    }

    public class FateType(byte id)
    {
        public byte Id = id;
        public long Records;

        public Dictionary<uint, Fate> Fates = [];
    }

    public class Fate(uint id)
    {
        public uint Id = id;
        public long Records;
        
        public Dictionary<uint, RewardTemp> Rewards = [];
    }
    
    public class RewardTemp
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