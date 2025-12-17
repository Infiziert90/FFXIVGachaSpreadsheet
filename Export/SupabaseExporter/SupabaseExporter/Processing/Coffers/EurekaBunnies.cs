using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Coffers;

public class EurekaBunnies : CofferBase
{
    public void ProcessAllData(Models.EurekaBunnyModel[] data)
    {
        Logger.Information("Processing eureka bunny data");
        Fetch(data);
        Combine();
        Export("EurekaBunnies.json");
        Dispose();
    }
    
    private void Fetch(Models.EurekaBunnyModel[] data)
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
            var cofferList = new List<Coffer.Variant>();
            foreach (var (rarity, patches) in rarities.OrderByDescending(c => c.Key))
            {
                var coffer = new Coffer.Variant(rarity, ((CofferRarity)rarity).ToName(), []);
                // Go over existing patches and calculate all averages
                foreach (var (patch, cofferData) in patches)
                    coffer.Patches[patch] = CalculateContent(cofferData);
                
                cofferList.Add(coffer);
            }

            ProcessedData.Add(new Coffer(((Territory)territory).ToName(), territory, cofferList));
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