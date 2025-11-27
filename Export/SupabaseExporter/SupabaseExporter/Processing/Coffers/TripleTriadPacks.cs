using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Coffers;

public class TripleTriadPacks : CofferBase
{
    // Also used for orderBy in the fetch method, so it's important to keep it in order
    private static readonly uint[] ValidPacks =  [
        10128, 10129, 10130, 17702, 28652, 
        13380, 10077, 17696, 17697, 17698, 
        17693, 17694, 17695, 17699, 17700, 
        17701, 17690, 17691, 17692
    ];
    
    public void ProcessAllData(List<Models.RandomCofferModel> data)
    {
        Logger.Information("Processing triple triad packs data");
        Fetch(data);
        Combine();
        Export("TripleTriadPacks.json");
        Dispose();
    }

    private void Fetch(List<Models.RandomCofferModel> data)
    {
        foreach (var coffer in data.Where(l => ValidPacks.Contains(l.Coffer)).OrderBy(l => l.Id))
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
            foreach (var (cofferId, patches) in rarities.OrderBy(c => Array.IndexOf(ValidPacks, c.Key)))
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

            ProcessedData.Add(new Coffer("Card Packs", territory, cofferList));
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