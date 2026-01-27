using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Coffers;

public class FieldOpLockboxes : CofferBase
{
    private static readonly uint[] ValidLockboxes = [22508, 23142, 23379, 24141, 24142, 24848, 24849, 31357, 33797, 50411, 50412, 50413];
    
    public void ProcessAllData(Models.RandomCofferModel[] data)
    {
        Logger.Information("Processing lockbox data");
        Fetch(data);
        Combine();
        Export("FieldOpLockboxes.json");
        Dispose();
    }

    private void Fetch(Models.RandomCofferModel[] data)
    {
        Logger.Information("Exporting lockbox data");
        foreach (var coffer in data.Where(l => ValidLockboxes.Contains(l.Coffer)).OrderBy(l => l.Id))
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
            var cofferList = new List<Coffer.Variant>();
            foreach (var (cofferId, patches) in rarities.OrderBy(c => c.Key))
            {
                var coffer = Sheets.ItemSheet.GetRow(cofferId);
                var cofferVariant = new Coffer.Variant(cofferId, coffer.Name.ExtractText(), []);
                
                // Go over existing patches and calculate all averages
                foreach (var (patch, cofferData) in patches)
                    cofferVariant.Patches[patch] = CalculateContent(cofferData);
                
                cofferList.Add(cofferVariant);
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