using System.Numerics;

namespace SupabaseExporter.Structures;

public class Occult : CofferBase
{
    public void ProcessAllData(List<Models.OccultTreasure> treasureData, List<Models.OccultBunny> bunnyData)
    {
        Console.WriteLine("Processing occult data");
        FetchTreasure(treasureData);
        Combine();
        
        CollectedData.Clear();
        
        FetchBunny(bunnyData);
        Combine();
        
        Export("Occult.json");
        Dispose();
    }

    private Dictionary<Vector3, uint> Positions = [];
    private void FetchTreasure(List<Models.OccultTreasure> data)
    {
        foreach (var treasure in data)
        {
            if (!CollectedData.ContainsKey((uint)OccultCategory.Treasure))
                CollectedData[(uint)OccultCategory.Treasure] = [];

            var adjustedCofferId = Sheets.TreasureSheet.GetRow(treasure.BaseId).SGB;
            
            var coffers = CollectedData[(uint)OccultCategory.Treasure];
            if (!coffers.ContainsKey(adjustedCofferId.RowId))
                coffers[adjustedCofferId.RowId] = [];
            
            var patch = treasure.GetPatch;
            var patches = coffers[adjustedCofferId.RowId];
            if (!patches.ContainsKey(patch))
                patches[patch] = new CofferTemp();

            patches[patch].AddMultiRecordWithAmount(treasure.GetRewards());
            
            var pos = new Vector3(treasure.ChestX, treasure.ChestY, treasure.ChestZ);
            if (!Positions.TryAdd(pos, 1))
                Positions[pos] += 1;
        }

        foreach (var (pos, counter) in Positions.OrderByDescending(kvp => kvp.Value))
            Console.WriteLine($"new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f), // Counter: {counter}");
    }
    
    private void FetchBunny(List<Models.OccultBunny> data)
    {
        foreach (var treasure in data)
        {
            var category = treasure.Coffer.ToCategory();
            if (!CollectedData.ContainsKey((uint)category))
                CollectedData[(uint)category] = [];
            
            var coffers = CollectedData[(uint)category];
            if (!coffers.ContainsKey(treasure.Coffer))
                coffers[treasure.Coffer] = [];
            
            var patch = treasure.GetPatch;
            var patches = coffers[treasure.Coffer];
            if (!patches.ContainsKey(patch))
                patches[patch] = new CofferTemp();

            patches[patch].AddMultiRecordWithAmount(treasure.GetRewards());
        }
    }

    private void Combine() 
    {
        foreach (var (category, rarities) in CollectedData)
        {
            var cofferList = new List<CofferData.CofferVariant>();
            foreach (var (rarity, patches) in rarities.OrderByDescending(pair => RaritySort(pair.Key)))
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

            ProcessedData.Add(new CofferData(category, ((OccultCategory)category).ToName(), cofferList));
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

    private uint RaritySort(uint key)
    {
        if (key == 1597)
            return key - 100; // Treasure is special as Silver is above Bronze in the order

        return key;
    } 
}