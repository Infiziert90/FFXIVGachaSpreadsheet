using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Coffers;

public class FieldOpContainers : CofferBase
{
    private static readonly uint[] ValidLogograms =  [24007, 24008, 24009, 24010, 24011, 24012, 24013, 24014, 24809];
    private static readonly uint[] ValidFragments = [30884, 30885, 30886, 30887, 30888, 30889, 30890, 30891, 30892, 30893, 30894, 30895, 30896, 30897, 30898, 30899, 32162, 32163, 32164, 32165, 32831, 32832, 32833, 32834, 33768, 33769, 33770, 33771, 33772, 33773, 33774, 33775, 33776, 33777, 33778, 33779];

    public void ProcessAllData(Models.RandomCofferModel[] data)
    {
        Logger.Information("Processing logograms and fragments data");
        Fetch(data);
        Combine();
        Export("FieldOpContainers.json");
        Dispose();
    }

    private void Fetch(Models.RandomCofferModel[] data)
    {
        foreach (var coffer in data.Where(l => ValidLogograms.Contains(l.Coffer) || ValidFragments.Contains(l.Coffer)).OrderBy(l => l.Id))
        {
            var type = (uint)EnumExtensions.ToLogoFrag(coffer.Coffer);
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

            ProcessedData.Add(new Coffer(((LogoFrag)territory).ToArea(), territory, cofferList));
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