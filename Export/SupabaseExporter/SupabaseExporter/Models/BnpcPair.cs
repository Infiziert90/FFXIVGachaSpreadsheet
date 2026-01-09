using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("BnpcPairs")]
public class BnpcPair : BaseModel
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

    [Name("rotation")]
    [Column("rotation")]
    public uint Rotation { get; set; }

    [Name("enemy_type")]
    [Column("enemy_type")]
    public ushort EnemyType { get; set; }

    [Name("level")]
    [Column("level")]
    public ushort Level { get; set; }

    [Name("display_flags")]
    [Column("display_flags")]
    public uint DisplayFlags { get; set; }

    [Name("gm_rank")]
    [Column("gm_rank")]
    public ushort GMRank { get; set; }

    [Name("spawn_type")]
    [Column("spawn_type")]
    public ushort SpawnType { get; set; }

    [Name("hash")]
    [Column("hash")]
    public string Hashed { get; set; }
}