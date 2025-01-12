using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter;

public class Models
{
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
    public class Gacha
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
    public class Bnuuy
    {
        [Column("id")]
        [Ignore]
        public uint Id { get; set; }

        [Column("territory")]
        public uint Territory { get; set; }

        [Column("coffer")]
        public uint Coffer { get; set; }

        [Column("items")]
        public uint[] Items { get; set; } = [];

        public Bnuuy() {}
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
    public class Venture
    {
        [Column("id")]
        [Ignore]
        public uint Id { get; set; }

        [Column("venture_type")]
        public ushort VentureType { get; set; }

        [Column("primary_id")]
        public uint PrimaryId { get; set; }

        [Column("primary_count")]
        public short PrimaryCount { get; set; }

        [Column("primary_hq")]
        public bool PrimaryHq { get; set; }

        [Column("additional_id")]
        public uint AdditionalId { get; set; }

        [Column("additional_count")]
        public short AdditionalCount { get; set; }

        [Column("additional_hq")]
        public bool AdditionalHq { get; set; }

        [Column("max_level")]
        public bool MaxLevel { get; set; }

        [Column("quick_venture")]
        public bool QuickVenture { get; set; }

        [Column("version")]
        public string Version { get; set; } = "";

        public Venture() {}
    }

    [Table("Desynthesis")]
    public class Desynthesis
    {
        [Name("id")]
        [Column("id")]
        public uint Id { get; set; }
        [Name("source")]
        [Column("source")]
        public uint Source { get; set; }

        [Name("rewards")]
        [NotMapped]
        public string Rewards { get; set; } = "";

        [Column("rewards")]
        [Ignore]
        public uint[] RewardArray { get; set; } = [];

        public Desynthesis() {}

        public uint[] GetRewards()
        {
            if (RewardArray.Length > 0)
                return RewardArray;

            var spl = Rewards.Trim('{', '}').Split(",");
            var length = spl.Length / 2;
            if (length > 3)
            {
                Console.Error.WriteLine($"Invalid length found, ID: {Id}");
                return [];
            }

            return spl.Select(uint.Parse).ToArray();
        }
    }
}