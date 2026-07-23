using CsvHelper.Configuration;

namespace SupabaseExporter.Structures.Temps;

public class FashionDyeTemp
{
    public Dictionary<uint, Slot> Slots = [];

    public class Slot
    {
        public Dictionary<uint, DyeDataTemp> Dyes = [];

        public void Update((uint, uint) stainIds, float weight)
        {
            var stains = new[] { stainIds.Item1, stainIds.Item2 };
            foreach (var stainId in stains)
            {
                if (stainId == 0)
                    continue;

                if (!Dyes.ContainsKey(stainId))
                    Dyes[stainId] = new DyeDataTemp();

                Dyes[stainId].Count += 1;
                Dyes[stainId].Confidence += weight;
            }
        }
    }

    public class DyeDataTemp
    {
        public long Count;
        public float Confidence;
    }
}

public class AvantGardeOldModel
{
    public uint RowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<uint> ItemIds { get; set; } = [];
}

public sealed class AvantGardeOldModelMap : ClassMap<AvantGardeOldModel>
{
    public AvantGardeOldModelMap()
    {
        Map(m => m.RowId).Convert(row => (uint)(row.Row.Parser.Row - 1));
        Map(m => m.Name).Name("Name");
        Map(m => m.ItemIds).Name("IDs").Convert(args =>
        {
            string value = args.Row.GetField<string>("IDs") ?? "#N/A";
            if (value == "#N/A")
                return [];
            return value.Split(',').Select(i => uint.Parse(i)).ToList();
        });
    }
}