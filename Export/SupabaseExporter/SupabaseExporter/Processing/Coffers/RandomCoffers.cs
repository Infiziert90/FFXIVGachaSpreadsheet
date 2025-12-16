using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Coffers;

public class RandomCoffers : CofferBase
{
    private static readonly uint[] ValidCoffers = [32161, 36635, 36636, 41667];
    
    public void ProcessAllData(Models.RandomCofferModel[] data)
    {
        Logger.Information("Processing random coffer data");
        Fetch(data);
        Combine();
        Export("RandomCoffers.json");
        Dispose();
    }

    private void Fetch(Models.RandomCofferModel[] data)
    {
        foreach (var coffer in data.Where(l => ValidCoffers.Contains(l.Coffer)).OrderBy(l => l.Id))
        {
            if (!CollectedData.ContainsKey(1))
                CollectedData[1] = [];
            
            var coffers = CollectedData[1];
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
            var cofferList = new List<Coffer.Variant>();
            foreach (var (cofferId, patches) in rarities.OrderBy(c => c.Key))
            {
                var coffer = Sheets.ItemSheet.GetRow(cofferId);
                var cofferVariant = new Coffer.Variant(cofferId, coffer.Name.ExtractText(), []);
                
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

            ProcessedData.Add(new Coffer("Coffers", territory, cofferList));
        }
    }
    
    private Coffer.Content CalculateContent(CofferTemp coffer)
    {
        var rewards = new List<Reward>();
        foreach (var (itemId, chestReward) in coffer.Rewards.OrderBy(pair => pair.Value.Amount))
        {
            rewards.Add(Reward.FromCofferReward(itemId, coffer.Total, chestReward));
            MappingHelper.AddItem(itemId);
        }

        return new Coffer.Content(coffer.Total, rewards);   
    }
}