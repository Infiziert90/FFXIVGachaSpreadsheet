namespace SupabaseExporter.Structures;

public class Lockbox : CofferBase
{
    private static readonly uint[] ValidCoffers = [22508, 23142, 23379, 24141, 24142, 24848, 24849, 31357, 33797];
    
    public void ProcessAllData(List<Models.Gacha> data)
    {
        Console.WriteLine("Processing lockbox data");
        Fetch(data);
        Combine();
        Export("LockboxData.json");
        Dispose();
    }

    private void Fetch(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting lockbox data");
        foreach (var coffer in data.Where(l => ValidCoffers.Contains(l.Coffer)).OrderBy(l => l.Id))
        {
            var type = (uint) ((LockboxTypes)coffer.Coffer).ToTerritory();
            if (!CollectedData.ContainsKey(type))
                CollectedData[type] = [];
            
            if (!CollectedData[type].ContainsKey(coffer.Coffer))
                CollectedData[type][coffer.Coffer] = [];
            
            var patch = coffer.GetPatch;
            if (!CollectedData[type][coffer.Coffer].ContainsKey(patch))
                CollectedData[type][coffer.Coffer][patch] = new CofferTemp();

            CollectedData[type][coffer.Coffer][patch].AddSimpleRecord(coffer.ItemId, 1);
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
                    processingBunny.AddExisting(tmp);
                
                cofferVariant.Patches["All"] = CalculateContent(processingBunny);
                
                cofferList.Add(cofferVariant);
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

            IconHelper.AddItem(item);
        }

        return new CofferData.CofferContent(coffer.Total, rewards);   
    }
}