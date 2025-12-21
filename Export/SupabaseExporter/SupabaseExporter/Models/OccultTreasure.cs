using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("OccultTreasure")]
public class OccultTreasureModel : BaseModel
{
    [Name("id")]
    [Column("id")]
    public uint Id { get; set; }

    [Name("base_id")]
    [Column("base_id")]
    public uint BaseId { get; set; }

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
        
    public OccultTreasureModel() {}

    public IEnumerable<(uint, uint)> GetRewards() 
        => Utils.PairIter(ProcessRewards());
        
    private uint[] ProcessRewards()
    {
        if (Rewards == string.Empty)
        {
            if (RewardsArray.Any(reward => reward > 1_000_000))
            {
                Logger.Error($"Invalid treasure result found, ID: {Id}");
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
                Logger.Error($"Invalid treasure result found, ID: {Id}");
                return [];
            }
                
            RewardsArray[counter] = result;
            counter++;
        }

        Rewards = string.Empty;
        return RewardsArray;
    }
}