using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
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

public sealed class RandomCofferMap : ClassMap<RandomCofferModel>
{
    public RandomCofferMap()
    {
        Map(m => m.Version).Name("version");
        
        Map(m => m.Id).Name("Id", "id");
        Map(m => m.Coffer).Name("coffer");
        Map(m => m.ItemId).Name("item_id");
        Map(m => m.Amount).Name("amount");
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}