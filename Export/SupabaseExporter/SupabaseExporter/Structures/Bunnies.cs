namespace SupabaseExporter.Structures;

public class Bunnies : CofferBase
{
    public async Task ProcessAllData(List<Models.Bnuuy> data)
    {
        Console.WriteLine("Processing bunny data");
        Fetch(data);
        Combine();
        await Export("BunnyData.json");
    }
    
    private void Fetch(List<Models.Bnuuy> data)
    {
        foreach (var bunny in data)
        {
            if (!CollectedData.ContainsKey(bunny.Territory))
                CollectedData[bunny.Territory] = [];
            
            var coffers = CollectedData[bunny.Territory];
            if (!coffers.ContainsKey(bunny.Coffer))
                coffers[bunny.Coffer] = [];
            
            var patch = bunny.GetPatch;
            var patches = coffers[bunny.Coffer];
            if (!patches.ContainsKey(patch))
                patches[patch] = new CofferTemp();

            patches[patch].AddMultiRecord(bunny.GetItems());
        }
    }

    private void Combine() 
    {
        foreach (var (territory, rarities) in CollectedData)
        {
            var cofferList = new List<CofferData.CofferVariant>();
            foreach (var (rarity, patches) in rarities.OrderByDescending(c => c.Key))
            {
                var coffer = new CofferData.CofferVariant(rarity, ((CofferRarity)rarity).ToName());
                // Go over existing patches and calculate all averages
                foreach (var (patch, cofferData) in patches)
                    coffer.Patches[patch] = CalculateContent(cofferData);
                
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
                coffer.Patches["All"] = CalculateContent(processingBunny);
                
                cofferList.Add(coffer);
            }

            ProcessedData.Add(new CofferData(territory, ((Territory)territory).ToName(), cofferList));
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