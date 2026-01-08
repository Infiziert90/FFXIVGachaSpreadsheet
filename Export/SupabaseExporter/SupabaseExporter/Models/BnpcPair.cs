using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("BnpcPairs")]
public class BnpcPair : BaseModel
{
    [Name("id")] 
    [Column("id")]
    public uint Id { get; set; }
    
    [Column("base")]
    public uint BaseId { get; set; }

    [Column("name")]
    public uint NameId { get; set; }

    [Column("territory")]
    public uint TerritoryId { get; set; }

    [Column("map")]
    public uint MapId { get; set; }

    [Column("level_id")]
    public uint LevelId { get; set; }

    [Column("x")]
    public float X { get; set; }

    [Column("y")]
    public float Y { get; set; }

    [Column("z")]
    public float Z { get; set; }

    [Column("rotation")]
    public uint Rotation { get; set; }

    [Column("enemy_type")]
    public ushort EnemyType { get; set; }

    [Column("level")]
    public ushort Level { get; set; }

    [Column("display_flags")]
    public uint DisplayFlags { get; set; }

    [Column("gm_rank")]
    public ushort GMRank { get; set; }

    [Column("spawn_type")]
    public ushort SpawnType { get; set; }

    [Column("hash")]
    public string Hashed { get; set; }
}