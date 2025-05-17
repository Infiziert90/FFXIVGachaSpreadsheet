namespace SupabaseExporter.Structures;

public class Cards : CofferBase
{
    // Also used for orderBy in the fetch method, so it's important to keep it in order
    private static readonly uint[] ValidCards =  [10128, 10129, 10130, 17702, 28652, 13380, 10077, 17696, 17697, 17698, 17693, 17694, 17695, 17699, 17700, 17701, 17690, 17691, 17692];
    
    public void ProcessAllData(List<Models.Gacha> data)
    {
        Console.WriteLine("Processing triple triad cards data");
        Fetch(data);
        Combine();
        Export("Cards.json");
        Dispose();
    }

    private void Fetch(List<Models.Gacha> data)
    {
        foreach (var coffer in data.Where(l => ValidCards.Contains(l.Coffer)).OrderBy(l => l.Id))
        {
            var type = (uint)TripleTriad.Any;
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
            foreach (var (cofferId, patches) in rarities.OrderBy(c => Array.IndexOf(ValidCards, c.Key)))
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

            ProcessedData.Add(new CofferData(territory, ((TripleTriad)territory).ToName(), cofferList));
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