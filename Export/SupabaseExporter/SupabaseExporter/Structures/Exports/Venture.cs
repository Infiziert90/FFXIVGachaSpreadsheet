using Lumina.Excel.Sheets;

namespace SupabaseExporter.Structures.Exports;

/// <summary>
/// The base of venture data.
/// </summary>
[Serializable]
public record Venture
{
    public string Name;
    public uint Category;
    public List<Task> Tasks = [];

    public Venture() { }
    
    public Venture(RetainerTask task)
    {
        Category = task.ClassJobCategory.RowId;

        Name = Category switch
        {
            0 => "All",
            34 => "DoW/DoM",
            _ => task.ClassJobCategory.Value.Name.ExtractText()
        };
    }

    public record Task(string Name, uint Type, Dictionary<string, Content> Patches);
    public record Content(uint Total, List<Reward> Primaries, List<Reward> Secondaries);
}