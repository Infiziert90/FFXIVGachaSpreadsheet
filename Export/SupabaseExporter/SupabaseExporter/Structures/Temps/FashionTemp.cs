namespace SupabaseExporter.Structures.Temps;

public class FashionDyeTemp
{
    public Dictionary<uint, Slot> Slots = [];

    public class Slot
    {
        public bool FoundPrecise = false;
        public List<(uint, uint)> Potentials = [];
        public Dictionary<uint, DyeDataTemp> Dyes = [];

        public void IncrementDyes((uint, uint) stainIds)
        {
            var stains = new[] { stainIds.Item1, stainIds.Item2 };
            foreach (var stainId in stains)
            {
                if (stainId == 0) continue;

                if (!Dyes.ContainsKey(stainId))
                    Dyes[stainId] = new();

                Dyes[stainId].Count += 1;
            }
        }

        public void LockSolutionSingle(uint stainId)
        {
            if (FoundPrecise)
                return;

            if (stainId == 0)
                return;

            if (!Dyes.ContainsKey(stainId))
                Dyes[stainId] = new();

            var dyeData = Dyes[stainId];
            dyeData.Confidence = 1;
            FoundPrecise = true;
        }

        public void LockSolutionSplit((uint, uint) stainIds)
        {
            if (FoundPrecise)
                return;

            if (Potentials.Contains(stainIds) || Potentials.Contains((stainIds.Item2, stainIds.Item1)))
                return;

            var stains = new[] { stainIds.Item1, stainIds.Item2 };
            for (int i = 0; i < 2; i++)
            {
                if (stains[i] == 0) continue;

                if (Potentials.Find(pair => pair.Item1 == stains[i] || pair.Item2 == stains[i]) != (0, 0))
                {
                    FoundPrecise = true;
                    if (!Dyes.ContainsKey(stains[i]))
                        Dyes[stains[i]] = new();
                    
                    Dyes[stains[i]].Confidence = 1;
                    break;
                }
            }
            Potentials.Add(stainIds);
        }
    }

    public class DyeDataTemp
    {
        public long Count = 0;
        public float Confidence = 0;
    }
}