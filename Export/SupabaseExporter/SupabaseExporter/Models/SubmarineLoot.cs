using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("Loot")]
public class SubmarineLootModel : BaseModel
{
    [Name("Id", "id")]
    [Column("id")]
    public uint Id { get; set; }

    [Name("Sector", "sector")]
    [Column("sector")]
    public uint Sector { get; set; }

    [Name("Unlocked", "unlocked")]
    [Column("unlocked")]
    public uint Unlocked { get; set; }

    [Name("Primary", "primary")]
    [Column("primary")]
    public uint Primary { get; set; }
    
    [Name("PrimaryCount", "primary_count")]
    [Column("primary_count")]
    public ushort PrimaryCount { get; set; }
    
    [Name("Additional", "additional")]
    [Column("additional")]
    public uint Additional { get; set; }
    
    [Name("AdditionalCount", "additional_count")]
    [Column("additional_count")]
    public ushort AdditionalCount { get; set; }

    [Name("Rank", "rank")]
    [Column("rank")]
    public int Rank { get; set; }
    
    [Name("Surv", "surv")]
    [Column("surv")]
    public int Surv { get; set; }
    
    [Name("Ret", "ret")]
    [Column("ret")]
    public int Ret { get; set; }
    
    [Name("Fav", "fav")]
    [Column("fav")]
    public int Fav { get; set; }

    [Name("PrimarySurvProc", "primary_surv_proc")]
    [Column("primary_surv_proc")]
    public uint PrimarySurvProc { get; set; }
    
    [Name("AdditionalSurvProc", "additional_surv_proc")]
    [Column("additional_surv_proc")]
    public uint AdditionalSurvProc { get; set; }
    
    [Name("PrimaryRetProc", "primary_ret_proc")]
    [Column("primary_ret_proc")]
    public uint PrimaryRetProc { get; set; }
    
    [Name("FavProc", "fav_proc")]
    [Column("fav_proc")]
    public uint FavProc { get; set; }

    [Name("Hash", "hash")]
    [Column("hash")]
    public string Hash { get; set; } = "";

    public SubmarineLootModel() {}
}

public sealed class SubmarineLootMap : ClassMap<SubmarineLootModel>
{
    public SubmarineLootMap()
    {
        Map(m => m.Version).Name("Version", "version").Optional();
        Map(m => m.Unlocked).Name("Unlocked", "unlocked").Optional();
        Map(m => m.Hash).Name("Hash", "hash").Optional();
        
        Map(m => m.Id).Name("Id", "id");
        Map(m => m.Sector).Name("Sector", "sector");
        Map(m => m.Primary).Name("Primary", "primary");
        Map(m => m.PrimaryCount).Name("PrimaryCount", "primary_count");
        Map(m => m.Additional).Name("Additional", "additional");
        Map(m => m.AdditionalCount).Name("AdditionalCount", "additional_count");
        Map(m => m.Rank).Name("Rank", "rank");
        Map(m => m.Surv).Name("Surv", "surv");
        Map(m => m.Ret).Name("Ret", "ret");;
        Map(m => m.Fav).Name("Fav", "fav");
        Map(m => m.PrimarySurvProc).Name("PrimarySurvProc", "primary_surv_proc");
        Map(m => m.AdditionalSurvProc).Name("AdditionalSurvProc", "additional_surv_proc");
        Map(m => m.PrimaryRetProc).Name("PrimaryRetProc", "primary_ret_proc");
        Map(m => m.FavProc).Name("FavProc", "fav_proc");
    }
}