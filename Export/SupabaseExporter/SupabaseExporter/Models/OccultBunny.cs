using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("OccultBunny")]
public class OccultBunnyModel : BaseModel
{
    [Name("id")]
    [Column("id")]
    public uint Id { get; set; }

    [Name("coffer")]
    [Column("coffer")]
    public uint Coffer { get; set; }

    [Name("territory")]
    [Column("territory")]
    public uint Territory { get; set; }
    
    [Name("rewards")]
    [NotMapped]
    public string Rewards { get; set; } = string.Empty;

    [Column("rewards")]
    [Ignore]
    public uint[] RewardsArray { get; set; } = new uint[12]; // There shouldn't be more than 6 items at any time, 12 is just safety

    [Name("pos_x")] 
    [Column("pos_x")]
    public float ChestX { get; set; }
    
    [Name("pos_y")] 
    [Column("pos_y")]
    public float ChestY { get; set; }
    
    [Name("pos_z")] 
    [Column("pos_z")]
    public float ChestZ { get; set; }
    
    [Name("fate_id")] 
    [Column("fate_id")]
    public ushort FateId { get; set; }
    
    public OccultBunnyModel() {}
    
    public IEnumerable<(uint, uint)> GetRewards() 
        => Utils.PairIter(ProcessRewards());

    private uint[] ProcessRewards()
    {
        if (Rewards == string.Empty)
        {
            if (RewardsArray.Any(reward => reward > 1_000_000))
            {
                Logger.Error($"Invalid bunny result found, ID: {Id}");
                return [];
            }

            return RewardsArray;
        }

        var span = Rewards.Trim('{', '}').AsSpan();

        var counter = 0;
        foreach (var range in span.Split(','))
        {
            var result = uint.Parse(span[range]);
            if (result > 1_000_000)
            {
                Logger.Error($"Invalid bunny result found, ID: {Id}");
                return [];
            }
            
            RewardsArray[counter] = result;
            counter++;
        }

        Rewards = string.Empty;
        return RewardsArray;
    }
}

public sealed class OccultBunnyExportMap : ClassMap<OccultBunnyModel>
{
    public OccultBunnyExportMap()
    {
        Map(m => m.Version).Name("version");
        
        Map(m => m.Id).Name("id");
        Map(m => m.Coffer).Name("coffer");
        Map(m => m.Territory).Name("territory");
        Map(m => m.RewardsArray).Name("rewards").Convert(l =>
        {
            l.Value.GetRewards();
            return $"{{{string.Join(",", l.Value.RewardsArray)}}}";
        });
        Map(m => m.ChestX).Name("pos_x");
        Map(m => m.ChestY).Name("pos_y");
        Map(m => m.ChestZ).Name("pos_z");
        Map(m => m.FateId).Name("fate_id");
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}

public sealed class OccultBunnyImportMap : ClassMap<OccultBunnyModel>
{
    public OccultBunnyImportMap()
    {
        Map(m => m.Version).Name("version");
        
        Map(m => m.Id).Name("id");
        Map(m => m.Coffer).Name("coffer");
        Map(m => m.Territory).Name("territory");
        Map(m => m.Rewards).Name("rewards");
        Map(m => m.ChestX).Name("pos_x");
        Map(m => m.ChestY).Name("pos_y");
        Map(m => m.ChestZ).Name("pos_z");
        Map(m => m.FateId).Name("fate_id");
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}