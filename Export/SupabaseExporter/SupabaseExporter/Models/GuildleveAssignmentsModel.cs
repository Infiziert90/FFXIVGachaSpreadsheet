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
    public uint RowId;

    [Name("categoryRowId")]
    [Column("categoryRowId")]
    public byte CategoryRowId;

    [Name("categoryIndex")]
    [Column("categoryIndex")]
    public byte CategoryIndex;

    [Name("leveIds")]
    [Column("leveIds")]
    public List<ushort> LeveIds = [];
}
