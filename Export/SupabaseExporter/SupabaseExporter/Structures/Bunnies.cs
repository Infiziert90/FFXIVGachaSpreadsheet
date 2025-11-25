namespace SupabaseExporter.Structures;

public class Bunnies : CofferBase
{
    public void ProcessAllData(List<Models.Bnuuy> data)
    {
        Logger.Information("Processing bunny data");
        Fetch(data);
        Combine();
        Export("BunnyData.json");
        Dispose();
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
                    processingBunny.AddExisting(tmp);
                
                coffer.Patches["All"] = CalculateContent(processingBunny);
                
                cofferList.Add(coffer);
            }

            ProcessedData.Add(new CofferData(territory, ((Territory)territory).ToName(), cofferList));
        }
    }
    
    private CofferData.CofferContent CalculateContent(CofferTemp coffer)
    {
        var rewards = new List<Reward>();
        foreach (var (itemId, chestReward) in coffer.Rewards.OrderBy(pair => pair.Value.Amount))
        {
            var item = Sheets.ItemSheet.GetRow(itemId);
            rewards.Add(Reward.FromCofferReward(item, coffer.Total, chestReward));

            MappingHelper.AddItem(itemId);
        }

        return new CofferData.CofferContent(coffer.Total, rewards);   
    }
}