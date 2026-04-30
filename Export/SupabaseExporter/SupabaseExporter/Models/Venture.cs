using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("Ventures")]
public class VentureModel : BaseModel
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

    public VentureModel() {}
}

public sealed class VentureMap : ClassMap<VentureModel>
{
    public VentureMap()
    {
        Map(m => m.Version).Name("version");
        
        Map(m => m.Id).Name("id");
        Map(m => m.VentureType).Name("venture_type");
        Map(m => m.PrimaryId).Name("primary_id");
        Map(m => m.PrimaryCount).Name("primary_count");
        Map(m => m.PrimaryHq).Name("primary_hq");
        Map(m => m.AdditionalId).Name("additional_id");
        Map(m => m.AdditionalCount).Name("additional_count");
        Map(m => m.AdditionalHq).Name("additional_hq");
        Map(m => m.MaxLevel).Name("max_level");
        Map(m => m.QuickVenture).Name("quick_venture");
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}