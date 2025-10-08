namespace SupabaseExporter.Structures;

public class DeepDungeonSack : CofferBase
{
    private static readonly uint[] ValidCoffers = [16170, 16171, 16172, 16173, 23223, 23224, 23225, 38945, 38946, 38947, 47104, 47105, 47106, 47742];

    public void ProcessAllData(List<Models.Gacha> data)
    {
        Logger.Information("Processing deep dungeon data");
        Fetch(data);
        Combine();
        Export("DeepDungeonData.json");
        Dispose();
    }
    
    private void Fetch(List<Models.Gacha> data)
    {
        Logger.Information("Exporting deep dungeon data");
        foreach (var coffer in data.Where(l => ValidCoffers.Contains(l.Coffer)).OrderBy(l => l.Id))
        {
            var type = (uint)EnumExtensions.ToDeepDungeon(coffer.Coffer);
            if (!CollectedData.ContainsKey(type))
                CollectedData[type] = [];
            
            var coffers = CollectedData[type];
            if (!coffers.ContainsKey(coffer.Coffer))
                coffers[coffer.Coffer] = [];
            
            var patch = coffer.GetPatch;
            var patches = coffers[coffer.Coffer];
            if (!patches.ContainsKey(patch))
                patches[patch] = new CofferTemp();
            
            patches[patch].AddSimpleRecord(coffer.ItemId, coffer.Amount);
        }
    }

    public void Combine() 
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

            ProcessedData.Add(new CofferData(territory, ((DeepDungeon)territory).ToName(), cofferList));
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