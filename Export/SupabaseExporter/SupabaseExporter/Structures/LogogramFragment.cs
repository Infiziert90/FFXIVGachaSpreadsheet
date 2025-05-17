namespace SupabaseExporter.Structures;

public class LogogramFragment : CofferBase
{
    private static readonly uint[] ValidLogograms =  [24007, 24008, 24009, 24010, 24011, 24012, 24013, 24014, 24809];
    private static readonly uint[] ValidFragments = [30884, 30885, 30886, 30887, 30888, 30889, 30890, 30891, 30892, 30893, 30894, 30895, 30896, 30897, 30898, 30899, 32162, 32163, 32164, 32165, 32831, 32832, 32833, 32834, 33768, 33769, 33770, 33771, 33772, 33773, 33774, 33775, 33776, 33777, 33778, 33779];

    public void ProcessAllData(List<Models.Gacha> data)
    {
        Console.WriteLine("Processing logograms and fragments data");
        Fetch(data);
        Combine();
        Export("LogoFrag.json");
        Dispose();
    }

    private void Fetch(List<Models.Gacha> data)
    {
        foreach (var coffer in data.Where(l => ValidLogograms.Contains(l.Coffer) || ValidFragments.Contains(l.Coffer)).OrderBy(l => l.Id))
        {
            var type = (uint)EnumExtensions.ToLogoFrag(coffer.Coffer);
            if (!CollectedData.ContainsKey(type))
                CollectedData[type] = [];
            
            var coffers = CollectedData[type];
            if (!coffers.ContainsKey(coffer.Coffer))
                coffers[coffer.Coffer] = [];
            
            var patch = coffer.GetPatch;
            var patches = coffers[coffer.Coffer];
            if (!patches.ContainsKey(patch))
                patches[patch] = new CofferTemp();
            
            patches[patch].AddSimpleRecord(coffer.ItemId, 1);
        }
    }

    private void Combine() 
    {
        foreach (var (territory, rarities) in CollectedData)
        {
            var cofferList = new List<CofferData.CofferVariant>();
            foreach (var (cofferId, patches) in rarities.OrderBy(c => c.Key))
            {
                var coffer = Sheets.ItemSheet.GetRow(cofferId);
                var cofferVariant = new CofferData.CofferVariant(cofferId, coffer.Name.ExtractText());
                
                // Go over existing patches and calculate all averages
                foreach (var (patch, cofferData) in patches)
                    cofferVariant.Patches[patch] = CalculateContent(cofferData);
                
                // Add a combined total of all existing patches
                // TODO rewrite to use existing data and aggregate together
                var processingBunny = new CofferTemp();
                foreach (var tmp in patches.Values)
                {
                    processingBunny.Total += tmp.Total;
                
                    foreach (var (itemId, reward) in tmp.Rewards)
                    {
                        processingBunny.Rewards.TryAdd(itemId, 0);
                        processingBunny.Rewards[itemId] += reward;
                    }
                }
                cofferVariant.Patches["All"] = CalculateContent(processingBunny);
                
                cofferList.Add(cofferVariant);
            }

            ProcessedData.Add(new CofferData(territory, ((LogoFrag)territory).ToArea(), cofferList));
        }
    }
    
    private CofferData.CofferContent CalculateContent(CofferTemp coffer)
    {
        var rewards = new List<Reward>();
        foreach (var (itemId, amount) in coffer.Rewards.OrderBy(pair => pair.Value))
        {
            var item = Sheets.ItemSheet.GetRow(itemId);
            rewards.Add(new Reward(item.Name.ExtractText(), item.RowId, (uint)amount, amount / coffer.Total));

            IconHelper.AddItem(item);
        }

        return new CofferData.CofferContent(coffer.Total, rewards);   
    }
}