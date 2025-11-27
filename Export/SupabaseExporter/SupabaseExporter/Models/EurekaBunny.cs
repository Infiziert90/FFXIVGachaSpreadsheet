using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("Bnuuy")]
public class EurekaBunnyModel : BaseModel
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

    public EurekaBunnyModel() {}

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

public sealed class EurekaBunnyMap : ClassMap<EurekaBunnyModel>
{
    public EurekaBunnyMap()
    {
        Map(m => m.Territory).Index(0).Name("Territory");
        Map(m => m.Coffer).Index(1).Name("Coffer");
        Map(m => m.Items).Index(2).Name("Items").Convert(l => $"[{string.Join(",", l.Value.Items)}]");
    }
}