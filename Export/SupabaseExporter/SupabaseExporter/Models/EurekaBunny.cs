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

public sealed class EurekaBunnyExportMap : ClassMap<EurekaBunnyModel>
{
    public EurekaBunnyExportMap()
    {
        Map(m => m.Version).Name("version").Optional();
        
        Map(m => m.Id).Name("id");
        Map(m => m.Territory).Index(0).Name("territory");
        Map(m => m.Coffer).Index(1).Name("coffer");
        Map(m => m.ItemsArray).Index(2).Name("items").Convert(l =>
        {
            l.Value.GetItems();
            return $"{{{string.Join(",", l.Value.ItemsArray)}}}";
        });
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}

public sealed class EurekaBunnyImportMap : ClassMap<EurekaBunnyModel>
{
    public EurekaBunnyImportMap()
    {
        Map(m => m.Version).Name("version").Optional();
        
        Map(m => m.Id).Name("id");
        Map(m => m.Territory).Index(0).Name("territory");
        Map(m => m.Coffer).Index(1).Name("coffer");
        Map(m => m.Items).Index(2).Name("items");
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}