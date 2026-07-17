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
                if (stainId == 0) continue;

                if (!Dyes.ContainsKey(stainId))
                    Dyes[stainId] = new();

                Dyes[stainId].Count += 1;
                Dyes[stainId].Confidence += weight;
            }
        }
    }

    public class DyeDataTemp
    {
        public long Count = 0;
        public float Confidence = 0;
    }
}