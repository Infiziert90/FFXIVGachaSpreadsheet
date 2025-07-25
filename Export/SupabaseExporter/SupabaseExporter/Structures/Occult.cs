﻿using System.Numerics;

namespace SupabaseExporter.Structures;

public class Occult : CofferBase
{
    public void ProcessAllData(List<Models.OccultTreasure> treasureData, List<Models.OccultBunny> bunnyData)
    {
        Logger.Information("Processing occult data");
        FetchTreasure(treasureData);
        Combine();
        
        CollectedData.Clear();

        FetchBunnyWitFateId(bunnyData);
        
        FetchBunny(bunnyData);
        Combine();
        
        Export("Occult.json");
        Dispose();
    }

    private Dictionary<Vector3, (uint, uint, uint)> Positions = [];
    private Dictionary<Vector3, (uint, uint, uint)> PotPositions = [];
    private Dictionary<Vector3, uint> BunnyPositions = [];
    private void FetchTreasure(List<Models.OccultTreasure> data)
    {
        foreach (var treasure in data)
        {
            // This range should include all random coffer
            if (treasure.BaseId is > 1856 or < 1789)
                continue;
            
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
            if (!Positions.TryAdd(pos, (1, adjustedCofferId.RowId, treasure.Id)))
            {
                var valueTuple = Positions[pos];
                valueTuple.Item1 += 1;
                Positions[pos] = valueTuple;
                
                if (valueTuple.Item2 != adjustedCofferId.RowId)
                    Logger.Warning("Different BaseId");
            }

            var rewards = treasure.GetRewards();
            
            var counter = 0;
            foreach (var reward in rewards)
            {
                if (reward != 0)
                    counter++;
            }
            
            if (counter > 6)
                Logger.Warning($"Weird length: {counter} | {treasure.Id}");

            if (rewards.Length == 0)
                continue;
            
            for (var i = 0; i < rewards.Length / 2; i++)
            {
                var itemId = rewards[2 * i];
                var amount = rewards[(2 * i) + 1];
                
                // hitting an item with ID 0 means we reached the last valid item
                if (itemId == 0)
                    break;
                
                if (amount > 3)
                    Logger.Error($"Invalid amount: {amount} {treasure.Id}");

                var item = Sheets.ItemSheet.GetRow(itemId);
                if (item.Rarity >= 3)
                    Logger.Error($"Invalid rarity?: {item.Name.ExtractText()} {item.Rarity} {treasure.Id}");
            }
        }

        Logger.Information($"Random Treasure: Unique {Positions.Count}");
        foreach (var (pos, counter) in Positions.OrderByDescending(kvp => kvp.Value))
        {
            foreach (var (otherPos, otherCounter) in Positions)
            {
                var dis = Vector3.Distance(otherPos, pos);
                if (dis != 0.0 && dis < 10.0)
                    Logger.Warning($"Found Small Distance ({dis}): {otherCounter.Item1}-{otherCounter.Item3} | {counter.Item1}-{counter.Item3}");
            }
            
            Logger.Information($"(new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f), {counter.Item2}), // Counter: {counter.Item1}");
        }
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

            var pos = new Vector3(treasure.ChestX, treasure.ChestY, treasure.ChestZ);
            if (pos == Vector3.Zero)
                continue;
            
            if (category == OccultCategory.Pot)
            {
                if (!PotPositions.TryAdd(pos, (1, treasure.FateId, treasure.Id)))
                {
                    var potPosition = PotPositions[pos];
                    potPosition.Item1 += 1;
                    PotPositions[pos] = potPosition;
                }
            }
            else
            {
                if (!BunnyPositions.TryAdd(pos, 1))
                    BunnyPositions[pos] += 1;
            }
        }
        
        Logger.Information($"Pot Treasure: Unique {PotPositions.Count} | Total Records {PotPositions.Sum(pair => pair.Value.Item1)}");
        foreach (var (pos, counter) in PotPositions.OrderByDescending(kvp => kvp.Value))
        {
            foreach (var (otherPos, otherCounter) in PotPositions)
            {
                var dis = Vector3.Distance(otherPos, pos);
                if (dis != 0.0 && dis < 10.0)
                    Logger.Error($"(FateId) Found Small Distance ({dis}): {otherCounter.Item1}-{otherCounter.Item3} | {counter.Item1}-{counter.Item3}");
            }
            
            Logger.Information($"new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f), // Counter: {counter}");
        }
        
        Logger.Information($"Bunny Treasure: Unique {BunnyPositions.Count} | Total Records {BunnyPositions.Sum(pair => pair.Value)}");
        foreach (var (pos, counter) in BunnyPositions.OrderByDescending(kvp => kvp.Value))
            Logger.Information($"new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f), // Counter: {counter}");
    }
    
    private void FetchBunnyWitFateId(List<Models.OccultBunny> data)
    {
        Dictionary<Vector3, (uint, uint, uint)> potPositions = [];
        
        foreach (var treasure in data)
        {
            if (treasure.FateId == 0)
                continue;
            
            if (treasure.Coffer.ToCategory() != OccultCategory.Pot) 
                continue;
            
            var pos = new Vector3(treasure.ChestX, treasure.ChestY, treasure.ChestZ);
            if (pos == Vector3.Zero)
                continue;
            
            if (!potPositions.TryAdd(pos, (1, treasure.FateId, treasure.Id)))
            {
                var potPosition = potPositions[pos];
                potPosition.Item1 += 1;
                    
                if (potPosition.Item2 == 0)
                    potPosition.Item2 = treasure.FateId;
                    
                potPositions[pos] = potPosition;

                if (potPosition.Item2 != treasure.FateId)
                    Logger.Warning($"(FateId) Overlap!!! {treasure.Id} {potPosition.Item2}");
            }
        }
        
        Logger.Information($"(FateId) Pot Treasure: Unique {potPositions.Count} | Total Records {potPositions.Sum(pair => pair.Value.Item1)}");
        foreach (var (pos, counter) in potPositions.OrderByDescending(kvp => kvp.Value))
        {
            foreach (var (otherPos, otherCounter) in potPositions)
            {
                var dis = Vector3.Distance(otherPos, pos);
                if (dis != 0.0 && dis < 10.0)
                    Logger.Error($"(FateId) Found Small Distance ({dis}): {otherCounter.Item1}-{otherCounter.Item3} | {counter.Item1}-{counter.Item3}");
            }
            
            Logger.Information($"(FateId) new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f), // Counter: {counter}");
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