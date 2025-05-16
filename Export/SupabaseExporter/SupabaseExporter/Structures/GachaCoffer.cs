namespace SupabaseExporter.Structures;

public class RandomCoffer : CofferBase
{
    private static readonly uint[] ValidCoffers = [32161, 36635, 36636, 41667];
    
    public async Task ProcessAllData(List<Models.Gacha> data)
    {
        Console.WriteLine("Processing lockbox data");
        Fetch(data);
        Combine();
        await Export("CofferData.json");
    }

    private void Fetch(List<Models.Gacha> data)
    {
        foreach (var coffer in data.Where(l => ValidCoffers.Contains(l.Coffer)).OrderBy(l => l.Id))
        {
            var type = (uint)Coffer.Any;
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

            ProcessedData.Add(new CofferData(territory, ((Coffer)territory).ToName(), cofferList));
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