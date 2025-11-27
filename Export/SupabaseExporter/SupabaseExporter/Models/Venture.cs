using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("Ventures")]
public class VentureModel : IBaseModel
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