namespace SupabaseExporter.Structures.Exports;

public class Fashion
{
    public Dictionary<uint, Dictionary<uint, long>> Categories = [];
    public Dictionary<uint, List<Slot>> WeeklyDyes = [];
    
    public Fashion() {}

    public void AddGoldRecord(uint category, uint item)
    {
        if (Categories.TryGetValue(category, out var data))
            data[item] = data.TryGetValue(item, out var count) ? count + 1 : 1;
        else
            Categories.Add(category, new Dictionary<uint, long> {{item, 1}});
    }

    public class Slot(uint id, string name, Dictionary<uint, DyeData> data)
    {
        public uint Id = id;
        public string Name = name;
        public Dictionary<uint, DyeData> Dyes = data;
    }
}

public record DyeData(long Count, double Pct);