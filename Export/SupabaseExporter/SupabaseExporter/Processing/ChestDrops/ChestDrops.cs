using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.ChestDrops;

public class ChestDrops : IDisposable
{
    private Dictionary<string, Models.ChestDropModel> HashedCache = [];
    
    private readonly Dictionary<uint, ChestDrop> ProcessedData = [];
    private readonly Dictionary<uint, ChestDropTemp> CollectedData = [];
    
    public void ProcessAllData(Models.ChestDropModel[] data)
    {
        Logger.Information("Processing chest drop data");
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }
    
    public void Dispose()
    {
        HashedCache.Clear();
        ProcessedData.Clear();
        CollectedData.Clear();
        GC.Collect();
    }

    private void Fetch(Models.ChestDropModel[] data)
    {
        foreach (var record in data)
        {
            if (!HashedCache.TryAdd(record.Hashed, record))
                if (HashedCache[record.Hashed].GetContent().SequenceEqual(record.GetContent()))
                    continue;

            if (!Sheets.TerritoryTypeSheet.TryGetRow(record.Territory, out var territoryType))
            {
                Logger.Error($"Invalid territory found, ID: {record.Id}");
                continue;
            }
            
            if (!Sheets.MapSheet.TryGetRow(record.Map, out var map))
            {
                Logger.Error($"Invalid map found, ID: {record.Id}");
                continue;
            }
            
            if (!Sheets.TreasureSheet.TryGetRow(record.ChestId, out var treasure))
            {
                Logger.Error($"Invalid treasure found, ID: {record.Id}");
                continue;
            }
            
            // Check if it is a non-duty drop (open world treasure hunt e.g.)
            var contentRowId = territoryType.ContentFinderCondition.RowId;
            if (contentRowId == 0)
                contentRowId = territoryType.RowId + 100_000;
            
            if (!CollectedData.TryGetValue(contentRowId, out var dutyLoot))
            {
                var expansionId = territoryType.ExVersion.RowId;
                var expansionName = Utils.UpperCaseStr(territoryType.ExVersion.Value.Name);
                if (contentRowId < 100_000)
                {
                    var cfd = territoryType.ContentFinderCondition.Value;
                    dutyLoot = new ChestDropTemp(contentRowId, Utils.UpperCaseStr(cfd.Name), cfd.ContentType.RowId, expansionName, expansionId, Utils.UpperCaseStr(cfd.ContentUICategory.Value.Name), cfd.ContentUICategory.RowId, cfd.SortKey);
                }
                else
                {
                    dutyLoot = new ChestDropTemp(contentRowId, Utils.UpperCaseStr(territoryType.PlaceName.Value.Name), 100_000, expansionName, expansionId, "", 0, 0);
                }
            }
            
            if (!dutyLoot.Chests.TryGetValue(record.ChestId, out var chest))
                chest = new ChestDropTemp.Chest(record.ChestId, Utils.UpperCaseStr(treasure.Unknown0), record.Territory, record.Map, Utils.UpperCaseStr(map.PlaceNameSub.Value.Name));
            
            foreach (var (itemId, amount) in record.GetRewards())
            {
                if (!chest.Rewards.ContainsKey(itemId))
                    chest.Rewards[itemId] = new ChestDropTemp.ChestReward();
                
                chest.Rewards[itemId].AddRewardRecord(amount);
            }
            
            dutyLoot.Records++;
            chest.Records++;
            
            dutyLoot.Chests[record.ChestId] = chest;
            CollectedData[contentRowId] = dutyLoot;
        }
    }

    private void Combine() 
    {
        foreach (var dutyLoot in CollectedData.Values)
        {
            var dutyCategoryName = "Open World";
            if (Sheets.ContentTypeSheet.TryGetRow(dutyLoot.DutyCategory, out var categoryRow))
                dutyCategoryName = Utils.UpperCaseStr(categoryRow.Name);
            
            if (!ProcessedData.ContainsKey(dutyLoot.DutyCategory))
                ProcessedData[dutyLoot.DutyCategory] = new ChestDrop(dutyCategoryName, dutyLoot.DutyCategory);
            var selectedCategory = ProcessedData[dutyLoot.DutyCategory];
            
            if (!selectedCategory.InternalExpansions.ContainsKey(dutyLoot.ExpansionKey))
                selectedCategory.InternalExpansions[dutyLoot.ExpansionKey] = new ChestDrop.Expansion(dutyLoot.Expansion, dutyLoot.ExpansionKey);
            var selectedExpansion = selectedCategory.InternalExpansions[dutyLoot.ExpansionKey];
            
            if (!selectedExpansion.InternalHeaders.ContainsKey(dutyLoot.UICategoryKey))
                selectedExpansion.InternalHeaders[dutyLoot.UICategoryKey] = new ChestDrop.Header(dutyLoot.UICategory, dutyLoot.UICategoryKey);
            var selectedTitle = selectedExpansion.InternalHeaders[dutyLoot.UICategoryKey];
            
            var finalDutyLoot = new ChestDrop.Duty(dutyLoot);
            foreach (var chestLoot in dutyLoot.Chests.Values.OrderBy(c => c.ChestId))
            {
                var lootContainer = new ChestDrop.Chest(chestLoot);
                foreach (var (itemId, reward) in chestLoot.Rewards)
                    lootContainer.AddReward(itemId, reward);
                
                finalDutyLoot.Chests.Add(lootContainer);
            }
    
            finalDutyLoot.Chests = finalDutyLoot.Chests.OrderBy(c => c.MapId).ThenBy(c => c.Id).ToList();
            selectedTitle.Duties.Add(finalDutyLoot);
        }

        foreach (var dutyLoot in ProcessedData.Values)
        {
            dutyLoot.Expansions = dutyLoot.InternalExpansions.Values.OrderBy(e => e.Id).ToList();
            foreach (var expansion in dutyLoot.Expansions)
            {
                expansion.Headers = expansion.InternalHeaders.Values.OrderBy(h => h.Id).ToList();
                foreach (var title in expansion.Headers)
                    title.Duties = title.Duties.OrderBy(t => t.SortKey).ToList();
            }
        }
    }
    
    private void Export()
    {
        Logger.Information("Start export of processed chest drop data ...");
        ExportHandler.WriteDataJson("ChestDrops.json", ProcessedData.Values.OrderBy(cd => cd.Id));
        Logger.Information("Done ...");
    }
}