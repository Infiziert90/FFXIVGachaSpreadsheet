using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("DutyLootV2")]
public class ChestDropModel : BaseModel
{
    [Name("id")] 
    [Column("id")]
    public uint Id { get; set; }
    
    [Name("map")] 
    [Column("map")]
    public uint Map { get; set; }
    
    [Name("territory")] 
    [Column("territory")]
    public uint Territory { get; set; }
    
    [Name("chest_id")] 
    [Column("chest_id")]
    public uint ChestId { get; set; }
    
    [Name("content")]
    [NotMapped]
    public string Content { get; set; } = string.Empty;

    [Column("content")]
    [Ignore]
    public uint[] ContentArray { get; set; } = new uint[60]; // Longest seen so far was 30 entries
    
    [Name("hashed")] 
    [Column("hashed")]
    public string Hashed { get; set; }
    
    [Name("created_at")] 
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Currently unused
    [Name("chest_x")] 
    [Column("chest_x")]
    public float ChestX { get; set; }
    
    // Currently unused
    [Name("chest_y")] 
    [Column("chest_y")]
    public float ChestY { get; set; }
    
    // Currently unused
    [Name("chest_z")] 
    [Column("chest_z")]
    public float ChestZ { get; set; }
    
    public IEnumerable<(uint, uint)> GetRewards() 
        => Utils.PairIter(ProcessRewards());

    public ReadOnlySpan<uint> GetContent() 
        => ProcessRewards();
    
    private uint[] ProcessRewards()
    {
        if (Content == string.Empty)
            return ContentArray;

        var span = Content.AsSpan().Trim(['{', '}']);

        var counter = 0;
        foreach (var range in span.Split(','))
        {
            ContentArray[counter] = uint.Parse(span[range]);
            counter++;
        }
        
        if (counter % 2 != 0)
        {
            Logger.Error($"Invalid length found, ID: {Id}");
            return [];
        }
        
        Content = string.Empty;
        return ContentArray;
    }
}

public sealed class ChestDropExportMap : ClassMap<ChestDropModel>
{
    public ChestDropExportMap()
    {
        Map(m => m.Version).Name("version");
        
        Map(m => m.Id).Name("id");
        Map(m => m.Map).Name("map");
        Map(m => m.Territory).Name("territory");
        Map(m => m.ChestId).Name("chest_id");
        Map(m => m.ContentArray).Name("content").Convert(l =>
        {
            l.Value.GetContent();
            return $"{{{string.Join(",", l.Value.ContentArray)}}}";
        });
        Map(m => m.Hashed).Name("hashed");
        Map(m => m.CreatedAt).Name("created_at");
        Map(m => m.ChestX).Name("chest_x");
        Map(m => m.ChestY).Name("chest_y");
        Map(m => m.ChestZ).Name("chest_z");
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}

public sealed class ChestDropImportMap : ClassMap<ChestDropModel>
{
    public ChestDropImportMap()
    {
        Map(m => m.Version).Name("version");
        
        Map(m => m.Id).Name("id");
        Map(m => m.Map).Name("map");
        Map(m => m.Territory).Name("territory");
        Map(m => m.ChestId).Name("chest_id");
        Map(m => m.Content).Name("content");
        Map(m => m.Hashed).Name("hashed");
        Map(m => m.CreatedAt).Name("created_at");
        Map(m => m.ChestX).Name("chest_x");
        Map(m => m.ChestY).Name("chest_y");
        Map(m => m.ChestZ).Name("chest_z");
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}