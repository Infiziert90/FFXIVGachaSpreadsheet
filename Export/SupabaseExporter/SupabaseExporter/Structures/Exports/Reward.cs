using Newtonsoft.Json;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// Reward of a given record, e.g., a venture item, or a coffer item.
/// </summary>
[Serializable]
public record Reward(uint Id, long Amount, [property: JsonConverter(typeof(LessPrecisionDouble))] double Pct, long Total = 0, long Min = 0, long Max = 0)
{
    public static Reward FromTaskReward(uint itemId, long total, VentureTemp.TaskReward temp) =>
        new(itemId, temp.Amount, temp.Amount / (double)total, temp.Total, temp.Min, temp.Max);
    
    public static Reward FromDutyLoot(uint itemId, long total, ChestDropTemp.ChestReward temp) =>
        new(itemId, temp.Amount, temp.Amount / (double)total, temp.Total, temp.Min, temp.Max);
    
    public static Reward FromCofferReward(uint itemId, long total, CofferTemp.ChestReward temp) =>
        new(itemId, temp.Amount, temp.Amount / (double)total, temp.Total, temp.Min, temp.Max);

    public static Reward FromDesyntReward(uint itemId, long total, DesynthTemp.DesynthReward temp) =>
        new(itemId, temp.Amount, temp.Amount / (double)total, 0, temp.Min, temp.Max);
}