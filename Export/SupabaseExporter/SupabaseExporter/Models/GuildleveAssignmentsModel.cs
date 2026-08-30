using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("GuildleveAssignments")]
public class GuildleveAssignmentsModel : BaseModel
{
    [Name("id")]
    [Column("id")]
    public uint Id { get; set; }

    [Name("rowId")]
    [Column("rowId")]
    public uint RowId { get; set; }

    [Name("categoryRowId")]
    [Column("categoryRowId")]
    public byte CategoryRowId { get; set; }

    [Name("categoryIndex")]
    [Column("categoryIndex")]
    public byte CategoryIndex { get; set; }

    [Name("leveIds")]
    [Column("leveIds")]
    public List<ushort> LeveIds { get; set; } = [];
}
