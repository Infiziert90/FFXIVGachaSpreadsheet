using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.FashionReport;

public class FashionReport : IDisposable
{
    private readonly Fashion ProcessedData = new();
    private readonly Dictionary<uint, FashionDyeTemp> CollectedSolutions = [];

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
            var weightSlots = CollectedSolutions[record.WeekNum].Slots;

            if (slots.Any(slot => (slot.Stamp == 5 && slot.Hint != 0) || (slot.Hint == 0 && slot.Stamp != 5))) 
            {
                Logger.Error($"Invalid record: Stamp ID 5 is reserved for inactive hints. ID: {record.Id}");
                continue;
            }

            foreach (var slot in slots)
            {
                if (slot.Item == 0) continue;

                switch (slot.Stamp)
                {
                    case 0:
                        ProcessedData.AddGoldRecord(slot.Hint, slot.Item);
                        goto case 5; // why is fall-through considered a compiler error??
                    case 5:
                        score -= IsLeftSide(slot.Id) ? 10u : 8u;
                        break;
                    default:
                        // Purposely ignoring all other stamp types
                        // _ => 9u - (slot.Stamp * 2),
                        score -= 2u;
                        break;
                }
            }

            foreach (var slot in slots)
            {
                if (slot.Item == 0) continue;

                if (!weightSlots.ContainsKey(slot.Id))
                    weightSlots[slot.Id] = new();
                var weightSlot = weightSlots[slot.Id];
                
                float weight = slot.IsDualDyed ? score / 2f : score;
                weightSlot.Update(slot.Dyes, weight);
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

                var total = dyes.Sum(dye => dye.Value.Confidence);
                foreach (var dye in dyes)
                {
                    var percentage = Math.Round(dye.Value.Confidence / total, 3);
                    dyeData.Add(dye.Key, new DyeData(dye.Value.Count, percentage));
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

    private static bool IsWeapon(uint slotId)
        => slotId is 1;

    private static bool IsLeftSide(uint slotId)
        => slotId >= 34 && slotId <= 38 || IsWeapon(slotId);
}

public record SlotInfo(uint Id, uint Hint, uint Stamp, uint Item, (uint, uint) Dyes)
{
    public bool IsDualDyed => Dyes.Item1 != 0 && Dyes.Item2 != 0;
}