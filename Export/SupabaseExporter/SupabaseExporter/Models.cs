using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter;

public class Models
{
    public class BaseModel
    {
        [Name("version")]
        [Column("version")]
        public string Version { get; set; } = string.Empty;

        private bool IsCalculated;
        private int NumberVersion;
        private string Patch;
        
        public int GetVersion
        {
            get
            {
                if (!IsCalculated)
                    Calculate();
                
                return NumberVersion;
            }
        }
        
        public string GetPatch
        {
            get
            {
                if (!IsCalculated)
                    Calculate();
                
                return Patch;
            }
        }

        private void Calculate()
        {
            IsCalculated = true;
            
            NumberVersion = Utils.VersionToNumber(Version);
            Patch = Utils.VersionToPatch(NumberVersion);
        }
    }
    
    [Table("Loot")]
    public class Loot
    {
        [Column("id")]
        public uint Id { get; set; }

        [Column("sector")]
        public uint Sector { get; set; }

        [Column("unlocked")]
        public uint Unlocked { get; set; }

        [Column("primary")]
        public uint Primary { get; set; }
        [Column("primary_count")]
        public ushort PrimaryCount { get; set; }
        [Column("additional")]
        public uint Additional { get; set; }
        [Column("additional_count")]
        public ushort AdditionalCount { get; set; }

        [Column("rank")]
        public int Rank { get; set; }
        [Column("surv")]
        public int Surv { get; set; }
        [Column("ret")]
        public int Ret { get; set; }
        [Column("fav")]
        public int Fav { get; set; }

        [Column("primary_surv_proc")]
        public uint PrimarySurvProc { get; set; }
        [Column("additional_surv_proc")]
        public uint AdditionalSurvProc { get; set; }
        [Column("primary_ret_proc")]
        public uint PrimaryRetProc { get; set; }
        [Column("fav_proc")]
        public uint FavProc { get; set; }

        [Column("hash")]
        public string Hash { get; set; } = "";

        [Column("version")]
        public string Version { get; set; } = "";

        public Loot() {}
    }

    [Table("Gacha")]
    public class Gacha : BaseModel
    {
        [Name("id")]
        [Column("id")]
        [Ignore]
        public uint Id { get; set; }

        [Name("coffer")]
        [Column("coffer")]
        public uint Coffer { get; set; }

        [Name("item_id")]
        [Column("item_id")]
        public uint ItemId { get; set; }

        [Name("amount")]
        [Column("amount")]
        public uint Amount { get; set; }

        public Gacha() {}
    }

    [Table("Bnuuy")]
    public class Bnuuy : BaseModel
    {
        [Column("id")]
        [Ignore]
        public uint Id { get; set; }

        [Name("territory")]
        [Column("territory")]
        public uint Territory { get; set; }

        [Name("coffer")]
        [Column("coffer")]
        public uint Coffer { get; set; }

        [Name("items")]
        [NotMapped]
        public string Items { get; set; } = string.Empty;

        [Column("items")]
        [Ignore]
        public uint[] ItemsArray { get; set; } = new uint[12]; // There shouldn't be more than 6 items at any time, 12 is just safety

        public Bnuuy() {}

        public ReadOnlySpan<uint> GetItems()
        {
            if (Items == string.Empty)
                return ItemsArray;

            var span = Items.Trim('{', '}').AsSpan();

            var counter = 0;
            foreach (var range in span.Split(','))
            {
                ItemsArray[counter] = uint.Parse(span[range]);
                counter++;
            }

            Items = string.Empty;
            return ItemsArray;
        }
    }

    public sealed class BnuuyMap : ClassMap<Bnuuy>
    {
        public BnuuyMap()
        {
            Map(m => m.Territory).Index(0).Name("Territory");
            Map(m => m.Coffer).Index(1).Name("Coffer");
            Map(m => m.Items).Index(2).Name("Items").Convert(l => $"[{string.Join(",", l.Value.Items)}]");
        }
    }

    [Table("Ventures")]
    public class Venture : BaseModel
    {
        [Ignore]
        [Column("id")]
        public uint Id { get; set; }

        [Name("venture_type")]
        [Column("venture_type")]
        public ushort VentureType { get; set; }

        [Name("primary_id")]
        [Column("primary_id")]
        public uint PrimaryId { get; set; }

        [Name("primary_count")]
        [Column("primary_count")]
        public short PrimaryCount { get; set; }

        [Name("primary_hq")]
        [Column("primary_hq")]
        public bool PrimaryHq { get; set; }

        [Name("additional_id")]
        [Column("additional_id")]
        public uint AdditionalId { get; set; }

        [Name("additional_count")]
        [Column("additional_count")]
        public short AdditionalCount { get; set; }

        [Name("additional_hq")]
        [Column("additional_hq")]
        public bool AdditionalHq { get; set; }

        [Name("max_level")]
        [Column("max_level")]
        public bool MaxLevel { get; set; }

        [Name("quick_venture")]
        [Column("quick_venture")]
        public bool QuickVenture { get; set; }

        public Venture() {}
    }

    [Table("Desynthesis")]
    public class Desynthesis : BaseModel
    {
        [Name("id")]
        [Column("id")]
        public uint Id { get; set; }

        [Name("source")]
        [Column("source")]
        public uint Source { get; set; }

        [Name("rewards")]
        [NotMapped]
        public string Rewards { get; set; } = string.Empty;

        [Column("rewards")]
        [Ignore]
        public uint[] RewardArray { get; set; } = new uint[6];

        public Desynthesis() {}

        public ReadOnlySpan<uint> GetRewards()
        {
            if (Rewards == string.Empty)
                return RewardArray;

            var span = Rewards.AsSpan().Trim(['{', '}']);

            var counter = 0;
            foreach (var range in span.Split(','))
            {
                if (counter >= 6)
                {
                    Console.Error.WriteLine($"Invalid length found, ID: {Id}");
                    return [];
                }
                
                RewardArray[counter] = uint.Parse(span[range]);
                counter++;
            }
            
            Rewards = string.Empty;
            return RewardArray;
        }
    }
    
    [Table("DutyLootV2")]
    public class DutyLoot : BaseModel
    {
        [Name("id")] 
        [Column("id")]
        public uint Id { get; set; }
        
        [Name("map")] 
        [Column("map")]
        public uint Map { get; set; }
        
        [Name("territory")] 
        [Column("territory")]
        public uint Territory { get; set; }
        
        [Name("chest_id")] 
        [Column("chest_id")]
        public uint ChestId { get; set; }
        
        [Name("content")]
        [NotMapped]
        public string Content { get; set; } = string.Empty;

        [Column("content")]
        [Ignore]
        public uint[] ContentArray { get; set; } = new uint[25];
        
        [Name("hashed")] 
        [Column("hashed")]
        public string Hashed { get; set; }
        
        [Name("created_at")] 
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Currently unused
        [Name("chest_x")] 
        [Column("chest_x")]
        public float ChestX { get; set; }
        
        // Currently unused
        [Name("chest_y")] 
        [Column("chest_y")]
        public float ChestY { get; set; }
        
        // Currently unused
        [Name("chest_z")] 
        [Column("chest_z")]
        public float ChestZ { get; set; }
        
        public ReadOnlySpan<uint> GetContent()
        {
            if (Content == string.Empty)
                return ContentArray;

            var span = Content.AsSpan().Trim(['{', '}']);

            var counter = 0;
            foreach (var range in span.Split(','))
            {
                ContentArray[counter] = uint.Parse(span[range]);
                counter++;
            }
            
            if (counter % 2 != 0)
            {
                Console.Error.WriteLine($"Invalid length found, ID: {Id}");
                return [];
            }
            
            Content = string.Empty;
            return ContentArray;
        }
    }
}