using System.Numerics;
using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Coffers;

public class OccultTreasures : CofferBase
{
    private HashSet<uint> IgnoreAmount = [51975, 51976, 45044, 45043];
    
    public void ProcessAllData(Models.OccultTreasureModel[] treasureData, Models.OccultBunnyModel[] bunnyData)
    {
        Logger.Information("Processing occult data");
        FetchTreasure(treasureData);
        FetchBunny(bunnyData);
        Combine();
        
        Export("OccultTreasuresV2.json");
        Dispose();
    }

    private Dictionary<Vector3, (uint, uint, uint)> Positions = [];
    private Dictionary<uint, Dictionary<Vector3, (uint Counter, Dictionary<CofferRarity, uint> Type, Dictionary<uint, uint> FateIds)>> PotPositions = [];
    private Dictionary<uint, Dictionary<Vector3, uint>> BunnyPositions = [];
    private void FetchTreasure(Models.OccultTreasureModel[] data)
    {
        foreach (var treasure in data)
        {
            // This range should include all treasure coffers
            if (treasure.Territory is 0 or 1252)
            {
                if (treasure.BaseId is > 1856 or < 1789)
                    continue;
            }
            else
            {
                if (treasure.BaseId is > 2073 or < 2006)
                    return;
            }
            
            var territory = treasure.Territory == 0 ? 1252 : treasure.Territory;
            if (!CollectedData.ContainsKey(territory))
                CollectedData[territory] = [];
            
            var coffers = CollectedData[territory];

            var adjustedCofferId = Sheets.TreasureSheet.GetRow(treasure.BaseId).SGB;
            if (!coffers.ContainsKey(adjustedCofferId.RowId))
                coffers[adjustedCofferId.RowId] = [];
            
            var patch = treasure.GetPatch;
            var patches = coffers[adjustedCofferId.RowId];
            if (!patches.ContainsKey(patch))
                patches[patch] = new CofferTemp();

            patches[patch].AddMultiRecordWithAmount(treasure.GetRewards());
            
            var pos = new Vector3(treasure.ChestX, treasure.ChestY, treasure.ChestZ);
            if (pos == Vector3.Zero)
            {
                Logger.Error($"Treasure Invalid position, {treasure.Id}");
                continue;
            }
            
            if (!Positions.TryAdd(pos, (1, adjustedCofferId.RowId, treasure.Id)))
            {
                var valueTuple = Positions[pos];
                valueTuple.Item1 += 1;
                Positions[pos] = valueTuple;
                
                if (valueTuple.Item2 != adjustedCofferId.RowId)
                    Logger.Warning($"Different BaseId, {treasure.Id} | {valueTuple.Item2} | {adjustedCofferId.RowId}");
            }
            
            // Check all entries for erroneous data
            foreach (var (i, (itemId, amount)) in treasure.GetRewards().Index())
            {
                if (i > 3)
                    Logger.Warning($"Weird length: {i} | {treasure.Id}");
                
                if (!IgnoreAmount.Contains(itemId))
                {
                    if (amount > 3)
                        Logger.Error($"Invalid amount: {amount} {treasure.Id}");
                }

                var item = Sheets.ItemSheet.GetRow(itemId);
                if (item.Rarity >= 4)
                    Logger.Error($"Invalid rarity?: {item.Name.ExtractText()} {item.Rarity} {treasure.Id}");
            }
        }

        Logger.Debug($"Random Treasure: Unique {Positions.Count}");
        foreach (var (pos, counter) in Positions.OrderByDescending(kvp => kvp.Value))
        {
            foreach (var (otherPos, otherCounter) in Positions)
            {
                var dis = Vector3.Distance(otherPos, pos);
                if (dis != 0.0 && dis < 10.0)
                    Logger.Warning($"Found Small Distance ({dis}): {otherCounter.Item1}-{otherCounter.Item3} | {counter.Item1}-{counter.Item3}");
            }
            
            Logger.Debug($"(new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f), {counter.Item2}), // Counter: {counter.Item1}");
        }
    }
    
    private void FetchBunny(Models.OccultBunnyModel[] data)
    {
        foreach (var bunny in data)
        {
            var category = bunny.Coffer.ToCategory();
            if (!CollectedData.ContainsKey(bunny.Territory))
                CollectedData[bunny.Territory] = [];
            
            var coffers = CollectedData[bunny.Territory];
            if (!coffers.ContainsKey(bunny.Coffer))
                coffers[bunny.Coffer] = [];
            
            var patch = bunny.GetPatch;
            var patches = coffers[bunny.Coffer];
            if (!patches.ContainsKey(patch))
                patches[patch] = new CofferTemp();

            patches[patch].AddMultiRecordWithAmount(bunny.GetRewardsWithoutCoins());

            var pos = new Vector3(bunny.ChestX, bunny.ChestY, bunny.ChestZ);
            if (pos == Vector3.Zero)
            {
                Logger.Error($"Bunny Invalid position, {bunny.Id}");
                continue;
            }
            
            // Check all entries for erroneous data
            foreach (var (i, (itemId, amount)) in bunny.GetRewardsWithoutCoins().Index())
            {
                if (i > 5)
                    Logger.Warning($"Weird length: {i} | {bunny.Id}");

                if (!IgnoreAmount.Contains(itemId))
                {
                    if (amount > 10)
                        Logger.Error($"Invalid amount: {amount} {bunny.Id}");
                }

                var item = Sheets.ItemSheet.GetRow(itemId);
                if (item.Rarity >= 4)
                    Logger.Error($"Invalid rarity?: {item.Name.ExtractText()} {item.Rarity} {bunny.Id}");
            }
            
            if (category == OccultCategory.Pot)
            {
                if (!PotPositions.ContainsKey(bunny.Territory))
                    PotPositions[bunny.Territory] = [];
                
                if (!PotPositions[bunny.Territory].TryGetValue(pos, out var potPosition))
                    potPosition = (0, [], []);

                potPosition.Counter += 1;

                if (!potPosition.Type.ContainsKey((CofferRarity)bunny.Coffer))
                    potPosition.Type[(CofferRarity)bunny.Coffer] = 0;
                
                potPosition.Type[(CofferRarity)bunny.Coffer]++;
                
                if (!potPosition.FateIds.ContainsKey(bunny.FateId))
                    potPosition.FateIds[bunny.FateId] = 0;
                potPosition.FateIds[bunny.FateId]++;
                
                PotPositions[bunny.Territory][pos] = potPosition;
            }
            else
            {
                if (!BunnyPositions.ContainsKey(bunny.Territory))
                    BunnyPositions[bunny.Territory] = [];
                
                if (!BunnyPositions[bunny.Territory].TryAdd(pos, 1))
                    BunnyPositions[bunny.Territory][pos] += 1;
            }
        }
        
        var bronze = 0L;
        var silver = 0L;
        var gold = 0L;

        foreach (var (key, value) in PotPositions)
        {
            if (key != 1346)
                continue;
            
            Logger.Debug($"Area: {key}");
            Logger.Debug($"Pot Treasure: Unique {value.Count} | Total Records {value.Sum(pair => pair.Value.Item1)}");
            foreach (var (pos, counter) in value.OrderByDescending(kvp => kvp.Value.FateIds.Keys.Max()).ThenBy(kvp => kvp.Value.Item1))
            {
                if (counter.Type.Count == 3)
                {
                    foreach (var type in counter.Type)
                    {
                        switch (type.Key)
                        {
                            case CofferRarity.OccultPotBronze:
                                bronze += type.Value;
                                break;
                            case CofferRarity.OccultPotSilver:
                                silver += type.Value;
                                break;
                            case CofferRarity.OccultPotGold:
                                gold += type.Value;
                                break;
                        }
                    }
                }
            
                Logger.Debug($"new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f), // Counter: {counter.Counter} // Treasures: {string.Join(',', counter.Type.OrderByDescending(s => s.Key).Select(s => s.Key.ToName() + $": {s.Value}"))} // FateId: {string.Join(", ", counter.FateIds.Select(pair => $"{pair.Key}:{pair.Value}"))}");
            }
            foreach (var (pos, counter) in value.OrderByDescending(kvp => kvp.Value.FateIds.Keys.Max()).ThenBy(kvp => kvp.Value.Item1))
            {
                if (counter.Type.Count == 3)
                {
                    foreach (var type in counter.Type)
                    {
                        switch (type.Key)
                        {
                            case CofferRarity.OccultPotBronze:
                                bronze += type.Value;
                                break;
                            case CofferRarity.OccultPotSilver:
                                silver += type.Value;
                                break;
                            case CofferRarity.OccultPotGold:
                                gold += type.Value;
                                break;
                        }
                    }
                }
            
                Logger.Debug($"[{{ x: {pos.X}, y: {pos.Y}, z: {pos.Z} }}, {counter.FateIds.Keys.Max()}, 0], ");
            }
            Logger.Debug($"Total Without Reroll: {bronze+silver+gold} | Gold: {gold} | Silver: {silver} | Bronze: {bronze}");
        }

        foreach (var (key, value) in BunnyPositions)
        {
            if (key != 1346)
                continue;
            
            Logger.Debug($"Area: {key}");
            Logger.Debug($"Bunny Treasure: Unique {value.Count} | Total Records {value.Sum(pair => pair.Value)}");
            foreach (var (pos, counter) in value.OrderByDescending(kvp => kvp.Value))
                Logger.Debug($"new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f), // Counter: {counter}");
            foreach (var (pos, counter) in value.OrderByDescending(kvp => kvp.Value))
                Logger.Debug($"[{{ x: {pos.X}, y: {pos.Y}, z: {pos.Z} }}, 0, 0], ");
        }
    }

    private void Combine() 
    {
        foreach (var (territory, rarities) in CollectedData)
        {
            var cofferList = new List<Coffer.Variant>();
            foreach (var (rarity, patches) in rarities.OrderBy(pair => RaritySort(pair.Key)))
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

    private uint RaritySort(uint key)
    {
        return key switch
        {
            2012936 => key + 1000000,
            2014742 => key + 10,
            2014741 => key + 20,
            _ => key
        };
    } 
}