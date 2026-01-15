using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("BnpcPairs")]
public class BnpcPairModel : BaseModel
{
    [Name("id")] 
    [Column("id")]
    public uint Id { get; set; }
    
    [Name("base")] 
    [Column("base")]
    public uint BaseId { get; set; }

    [Name("name")]
    [Column("name")]
    public uint NameId { get; set; }

    [Name("territory")]
    [Column("territory")]
    public uint TerritoryId { get; set; }

    [Name("map")]
    [Column("map")]
    public uint MapId { get; set; }

    [Name("level_id")]
    [Column("level_id")]
    public uint LevelId { get; set; }

    [Name("x")]
    [Column("x")]
    public float X { get; set; }

    [Name("y")]
    [Column("y")]
    public float Y { get; set; }

    [Name("z")]
    [Column("z")]
    public float Z { get; set; }

    [Name("level")]
    [Column("level")]
    public ushort Level { get; set; }

    [Name("object_kind")]
    [Column("object_kind")]
    public ushort ObjectKind { get; set; }

    [Name("enemy_type")]
    [Column("enemy_type")]
    public ushort Battalion { get; set; }

    [Name("hash")]
    [Column("hash")]
    public string Hashed { get; set; }
}

public sealed class BnpcPairMap : ClassMap<BnpcPairModel>
{
    public BnpcPairMap()
    {
        Map(m => m.Id).Name("id");
        Map(m => m.Battalion).Name("enemy_type", "sector");
        Map(m => m.BaseId).Name("base");
        Map(m => m.NameId).Name("name");
        Map(m => m.TerritoryId).Name("territory");
        Map(m => m.MapId).Name("map");
        Map(m => m.LevelId).Name("level_id");
        Map(m => m.X).Name("x");
        Map(m => m.Y).Name("y");
        Map(m => m.Z).Name("z");
        Map(m => m.Level).Name("level");
        Map(m => m.Hashed).Name("hash");
        
        Map(m => m.ObjectKind).Name("object_kind").Optional();

        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}