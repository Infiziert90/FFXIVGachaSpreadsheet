using System.ComponentModel.DataAnnotations.Schema;

namespace SupabaseExporter.Models;

[Table("Loot")]
public class SubmarineLootModel : IBaseModel
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

    public SubmarineLootModel() {}
}