using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("DesynthesisV2")]
public class DesynthesisModel2 : BaseModel
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
    
    [Name("class_level")]
    [Column("class_level")]
    public float ClassLevel { get; set; }
    
    [Name("increase")]
    [Column("increase")]
    public float Increase { get; set; }

    public DesynthesisModel2() {}
        
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

// public sealed class DesynthesisExportMap : ClassMap<DesynthesisModel>
// {
//     public DesynthesisExportMap()
//     {
//         Map(m => m.Version).Name("version");
//         
//         Map(m => m.Id).Name("id");
//         Map(m => m.Source).Name("source");
//         Map(m => m.RewardArray).Name("rewards").Convert(l =>
//         {
//             l.Value.GetRewards();
//             return $"{{{string.Join(",", l.Value.RewardArray)}}}";
//         });
//         Map(m => m.ClassLevel).Name("class_level").Optional();
//         
//         Map(m => m.GetVersion).Ignore();
//         Map(m => m.GetPatch).Ignore();
//     }
// }
//
// public sealed class DesynthesisImportMap : ClassMap<DesynthesisModel>
// {
//     public DesynthesisImportMap()
//     {
//         Map(m => m.Version).Name("version");
//         
//         Map(m => m.Id).Name("id");
//         Map(m => m.Source).Name("source");
//         Map(m => m.Rewards).Name("rewards");
//         Map(m => m.ClassLevel).Name("class_level").Optional();
//         
//         Map(m => m.GetVersion).Ignore();
//         Map(m => m.GetPatch).Ignore();
//     }
// }