using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("Desynthesis")]
public class DesynthesisModel : BaseModel
{
    [Name("id")]
    [Column("id")]
    public uint Id { get; set; }

    [Name("source")]
    [Column("source")]
    public uint Source { get; set; }

    [Name("rewards")]
    [NotMapped]
    public string Rewards { get; set; } = string.Empty;

    [Column("rewards")]
    [Ignore]
    public uint[] RewardArray { get; set; } = new uint[6];

    public DesynthesisModel() {}
        
    public IEnumerable<(uint, uint)> GetRewards() 
        => Utils.PairIter(ProcessRewards());

    private uint[] ProcessRewards()
    {
        if (Rewards == string.Empty)
            return RewardArray;

        var span = Rewards.AsSpan().Trim(['{', '}']);

        var counter = 0;
        foreach (var range in span.Split(','))
        {
            if (counter >= 6)
            {
                Logger.Error($"Invalid length found, ID: {Id}");
                return [];
            }
                
            RewardArray[counter] = uint.Parse(span[range]);
            counter++;
        }
            
        Rewards = string.Empty;
        return RewardArray;
    }
}