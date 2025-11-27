namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// The base of coffer data.
/// </summary>
[Serializable]
public class Coffer(string name, uint territoryId, List<Coffer.Variant> variants)
{
    public string Name = name;
    public uint TerritoryId = territoryId;
    public List<Variant> Variants = variants;

    public record Variant(uint Id, string Name, Dictionary<string, Content> Patches);
    public record Content(long Total, List<Reward> Items);
}