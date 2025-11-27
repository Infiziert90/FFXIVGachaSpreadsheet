using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("Gacha")]
public class RandomCofferModel : BaseModel
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

    public RandomCofferModel() {}
}