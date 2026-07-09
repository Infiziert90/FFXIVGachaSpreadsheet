using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.FashionReport;

public class FashionReport : IDisposable
{
    private Fashion ProcessedData = new();
    private Dictionary<uint, FashionDyeTemp> CollectedSolutions = new();

    public void ProcessAllData(Models.FashionReportModel[] data)
    {
        Logger.Information("Processing fashion report data");
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }

    public void Dispose()
    {
        ProcessedData.Categories.Clear();
        ProcessedData.WeeklyDyes.Clear();
        CollectedSolutions.Clear();
        GC.Collect();
    }

    private void Fetch(Models.FashionReportModel[] data)
    {
        foreach (var record in data)
        {
            var score = record.Score;
            var slots = SetSlots(record);

            if (!CollectedSolutions.ContainsKey(record.WeekNum))
                CollectedSolutions[record.WeekNum] = new();
            var collectedDyes = CollectedSolutions[record.WeekNum].Slots;

            if (slots.Any(slot => (slot.Stamp == 5 && slot.Hint != 0) || (slot.Hint == 0 && slot.Stamp != 5))) 
            {
                Logger.Error($"Invalid record: Stamp ID 5 is reserved for inactive hints. ID: {record.Id}");
                continue;
            }

            foreach (var slot in slots)
            {
                if (!collectedDyes.ContainsKey(slot.Id))
                    collectedDyes[slot.Id] = new();
                var collectedDyesSlot = collectedDyes[slot.Id];
                
                collectedDyesSlot.IncrementDyes(slot.Dyes);

                switch (slot.Stamp)
                {
                    case 0:
                        ProcessedData.AddGoldRecord(slot.Hint, slot.Item);
                        score -= 10;
                        break;
                    case 1:
                        score -= 8;
                        break;
                    case 2:
                        score -= 6;
                        break;
                    case 3:
                        // It's theoretically possible for an item to score 4 points purely on its own,
                        // in which case incorrect dyes will be marked as the solutions. Hasn't been observed in many years though.
                        if (slot.Dyes.Item1 != 0 && slot.Dyes.Item2 == 0)
                        {
                            collectedDyesSlot.LockSolutionSingle(slot.Dyes.Item1);
                        }
                        else if (slot.Dyes.Item1 == 0 && slot.Dyes.Item2 != 0)
                        {
                            collectedDyesSlot.LockSolutionSingle(slot.Dyes.Item2);
                        }
                        else if (slot.Dyes.Item1 != 0 && slot.Dyes.Item2 != 0)
                        {
                            collectedDyesSlot.LockSolutionSplit(slot.Dyes);
                        }
                        score -= 4;
                        break;
                    case 4:
                        score -= 2;
                        break;
                    case 5:
                        score -= 10;
                        break;
                }

                // TODO: Check remaining score and compare against available dyes
            }
        }
    }

    private void Combine()
    {
        foreach (var data in CollectedSolutions)
        {
            var slots = data.Value.Slots;
            List<Fashion.Slot> slotData = [];
            foreach (var slot in slots)
            {
                var dyes = slot.Value.Dyes;
                Dictionary<uint, DyeData> dyeData = [];
                foreach (var dye in dyes)
                {
                    dyeData.Add(dye.Key, new DyeData(dye.Value.Count, dye.Value.Confidence));
                }
                dyeData = dyeData.OrderByDescending(x => x.Value.Pct).ToDictionary();
                
                if (slot.Key <= 38)
                {
                    var slotName = slot.Key == 1 ? "Weapon" : Sheets.ItemUICategorySheet.GetRow(slot.Key).Name.ToString();
                    slotData.Add(new(slot.Key, slotName, dyeData));
                }
            }
            ProcessedData.WeeklyDyes.Add(data.Key, slotData);
        }
    }

    private void Export()
    {
        ExportHandler.WriteDataJson("FashionReport.json", ProcessedData);
        Logger.Information("Done exporting data ...");
    }

    private List<SlotInfo> SetSlots(Models.FashionReportModel record)
    {
        List<SlotInfo> slots = [];
        var enCats = record.GetCategories().GetEnumerator();
        var enDyes = record.GetDyes().GetEnumerator();
        var items = record.GetItems();
        var advanceDyes = true;
        for (var i = 0; i < items.Length; i++)
        {
            if (advanceDyes && !enDyes.MoveNext())
                advanceDyes = false;

            if (!enCats.MoveNext())
                break;

            var dyes = i < 9 || i > 12 ? enDyes.Current : (0, 0);
            
            if (!Sheets.ItemSheet.TryGetRow(items[i], out var item))
            {
                Logger.Error($"Can't get item from rowid. ID: {record.Id}");
                return [];
            }

            uint slotId = item.EquipSlotCategory.RowId is 1 or 2 or 13 ? 1 : item.ItemUICategory.RowId;
            slots.Add(new SlotInfo(slotId, enCats.Current.Item1, enCats.Current.Item2, items[i], dyes));
        }
        return slots;
    }
}

public record SlotInfo(uint Id, uint Hint, uint Stamp, uint Item, (uint, uint) Dyes);