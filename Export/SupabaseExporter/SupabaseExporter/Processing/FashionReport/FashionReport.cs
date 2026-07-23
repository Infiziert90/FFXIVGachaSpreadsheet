using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.FashionReport;

public class FashionReport : IDisposable
{
    private readonly Fashion ProcessedData = new();
    private readonly Dictionary<uint, FashionDyeTemp> CollectedSolutions = [];

    private const string OldDatabasePath = "../../../Processing/FashionReport";
    private const string OldDatabaseFilename = "AvantGardeDataOld.csv";

    public void ProcessAllData(Models.FashionReportModel[] data)
    {
        Logger.Information("Processing fashion report data");
        Fetch(data);
        MergeOld();
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
            var isValid = true;

            if (!CollectedSolutions.ContainsKey(record.WeekNum))
                CollectedSolutions[record.WeekNum] = new FashionDyeTemp();

            var weightSlots = CollectedSolutions[record.WeekNum].Slots;

            foreach (var slot in slots)
            {
                if (slot.Item == 0) 
                    continue;

                if ((slot.Stamp == 5 && slot.Hint != 0) || (slot.Hint == 0 && slot.Stamp != 5))
                {
                    Logger.Error($"Invalid record: Stamp ID 5 is reserved for inactive hints. ID: {record.Id}");
                    isValid = false;                    
                    break;
                }

                // Purposely ignoring all other stamp types
                // 9u - (slot.Stamp * 2),
                score -= slot.Stamp is 0 or 5 ? (IsLeftSide(slot.Id) ? 10u : 8u) : 2u;
            }

            if (!isValid)
                continue;

            foreach (var slot in slots)
            {
                if (slot.Item == 0) 
                    continue;

                if (slot.Stamp == 0)
                    ProcessedData.AddGoldRecord(slot.Hint, slot.Item);

                if (!weightSlots.ContainsKey(slot.Id))
                    weightSlots[slot.Id] = new FashionDyeTemp.Slot();
                
                var weightSlot = weightSlots[slot.Id];
                
                var weight = slot.IsDualDyed ? score / 2f : score;
                weightSlot.Update(slot.Dyes, weight);
            }
        }
    }
    
    private void MergeOld()
    {
        using var reader = new StreamReader(Path.Combine(OldDatabasePath, OldDatabaseFilename));
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

        csv.Context.RegisterClassMap<AvantGardeOldModelMap>();
        var records = csv.GetRecords<AvantGardeOldModel>();

        foreach (var record in records)
        {
            foreach (var item in record.ItemIds)
            {
                ProcessedData.AddGoldRecord(record.RowId, item);
            }
        }
    }

    private void Combine()
    {
        foreach (var data in CollectedSolutions)
        {
            var slotData = new List<Fashion.Slot>();
            foreach (var slot in data.Value.Slots)
            {
                var dyes = slot.Value.Dyes;
                var dyeData = new Dictionary<uint, DyeData>();

                var totalWeightVal = Math.Max(dyes.Sum(dye => dye.Value.Confidence), 0.01f);

                foreach (var dye in dyes)
                {
                    var percentage = Math.Round(dye.Value.Confidence / totalWeightVal, 3);
                    dyeData.Add(dye.Key, new DyeData(dye.Value.Count, percentage));
                }
                
                dyeData = dyeData.OrderByDescending(x => x.Value.Pct).ToDictionary();
                
                if (slot.Key <= 38)
                {
                    var slotName = slot.Key == 1 ? "Weapon" : Sheets.ItemUICategorySheet.GetRow(slot.Key).Name.ToString();
                    slotData.Add(new Fashion.Slot(slot.Key, slotName, dyeData));
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
        var slots = new List<SlotInfo>();
        using var enCats = record.GetCategories().GetEnumerator();
        using var enDyes = record.GetDyes().GetEnumerator();
        
        var items = record.GetItems();
        var advanceDyes = true;
        for (var i = 0; i < items.Length; i++)
        {
            if (advanceDyes && !enDyes.MoveNext())
                advanceDyes = false;

            if (!enCats.MoveNext())
                break;
            
            if (!Sheets.ItemSheet.TryGetRow(items[i], out var item))
            {
                Logger.Error($"Can't get item from rowId. ID: {record.Id}");
                return [];
            }

            var dyes = i is < 9 or > 12 ? enDyes.Current : (0, 0);
            var slotId = item.EquipSlotCategory.RowId is 1 or 2 or 13 ? 1 : item.ItemUICategory.RowId;
            slots.Add(new SlotInfo(slotId, enCats.Current.Item1, enCats.Current.Item2, items[i], dyes));
        }
        
        return slots;
    }

    private static bool IsWeapon(uint slotId)
        => slotId is 1;

    private static bool IsLeftSide(uint slotId)
        => slotId is >= 34 and <= 38 || IsWeapon(slotId);
}

public record SlotInfo(uint Id, uint Hint, uint Stamp, uint Item, (uint, uint) Dyes)
{
    public bool IsDualDyed => Dyes.Item1 != 0 && Dyes.Item2 != 0;
}