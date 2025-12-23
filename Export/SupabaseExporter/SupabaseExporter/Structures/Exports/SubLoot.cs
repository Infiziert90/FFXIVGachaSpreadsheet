using Lumina.Excel.Sheets;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// The base of submarine loot data.
/// </summary>
[Serializable]
public class SubLoot
{
    // Used for internal cache keeping
    public uint ProcessedId;
    
    public int Total;
    public Dictionary<uint, Sector> Sectors = [];
    
    public SubLoot() { }

    public class Sector
    {
        public int Records;
        
        public uint Id;
        public string Name;
        public string Letter;
        public uint Rank;
        public uint Stars;
        public uint UnlockedFrom;
        
        public Dictionary<SurvTier, LootPool> Pools = new()
        {
            { SurvTier.Tier1, new LootPool() },
            { SurvTier.Tier2, new LootPool() },
            { SurvTier.Tier3, new LootPool() },
        };
        
        public Sector(SubmarineExploration sector)
        {
            Id = sector.RowId;
            Name = sector.Destination.ToString();
            Letter = sector.Location.ToString();
            Rank = sector.RankReq;
            Stars = sector.Stars;
        }
        
        public Sector() { }
    }

    public class LootPool
    {
        public int Records;
        
        public Dictionary<uint, PoolReward> Rewards = [];
        public Stats Stats = new();
        
        public LootPool() { }

        public void AddRecord(uint itemId, int quantity, RetTier type)
        {
            if (!Rewards.ContainsKey(itemId))
                Rewards[itemId] = new PoolReward(itemId);
                
            Records += 1;
            Rewards[itemId].AddPoorRecord(quantity, type);
        }
    }
    
    public record PoolReward
    {
        public uint Id;
        public long Amount;
        public long Total;

        public Dictionary<RetTier, int[]> MinMax = new()
        {
            { RetTier.Poor, [0, 0] },
            { RetTier.Normal, [0, 0] },
            { RetTier.Optimal, [0, 0] },
        };

        public PoolReward(uint itemId)
        {
            Id = itemId;
        }
        
        public PoolReward() { }

        public void AddPoorRecord(int quantity, RetTier type)
        {
            Amount += 1;
            Total += quantity;
            
            var minMax = MinMax[type];
            if (minMax[0] == 0)
                minMax[0] = quantity;
            
            minMax[0] = Math.Min(minMax[0], quantity);
            minMax[1] = Math.Max(minMax[1], quantity);
            MinMax[type] = minMax;
        }
    }

    public class Stats
    {
        // Surveillance
        public int Min;
        public int Mid;
        public int High;

        // Retrieval
        public int Low;
        public int Normal;
        public int Optimal;

        // Favor
        public int Favor;
        public int DoubleDips;
        
        public Stats() { }

        public void IncreaseSurveillance(uint survProc)
        {
            switch ((SurveillanceProc)survProc)
            {
                case SurveillanceProc.T1Low:
                    Min += 1;
                    break;
                case SurveillanceProc.T1Mid:
                case SurveillanceProc.T2Mid:
                    Mid += 1;
                    break;
                case SurveillanceProc.T1High:
                case SurveillanceProc.T2High:
                case SurveillanceProc.T3High:
                    High += 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void IncreaseRetrieval(uint retProc)
        {
            switch ((RetrievalProc)retProc)
            {
                case RetrievalProc.Poor:
                    Low += 1;
                    break;
                case RetrievalProc.Normal:
                    Normal += 1;
                    break;
                case RetrievalProc.Optimal:
                    Optimal += 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void IncreaseFavor(uint favorProc)
        {
            switch ((FavorProc)favorProc)
            {
                case FavorProc.Low:
                    break;
                case FavorProc.StatsEnoughButFailed:
                    Favor += 1;
                    break;
                case FavorProc.Yes:
                    Favor += 1;
                    DoubleDips += 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}