using Google.Apis.Sheets.v4.Data;

using static SupabaseExporter.Utils;

namespace SupabaseExporter;

public class GachaHandler(SheetHandler sheetHandler)
{
    private static readonly HashSet<uint> Fireworks = [38540, 39502, 40393, 41501];
    private static readonly HashSet<uint> Lockboxes = [31357, 33797, 22508, 23142, 23379, 24141, 24142, 24848, 24849, 31357, 33797];

    public void ReadCofferData(Models.Gacha[] data, string sheetName, uint target, int column = 0)
    {
        var total = 0.0;
        var dict = new Dictionary<uint, double>();
        foreach (var entry in data.Where(g => g.Coffer == target))
        {
            var amount = entry.Amount;
            if (target == 32161 && entry.ItemId != 8841)
                amount /= 2;
            else if (target == 41667 && Fireworks.Contains(entry.ItemId))
                amount /= 5;
            else if (Lockboxes.Contains(target) && amount > 1)
                amount = 1;

            total += amount;
            if (!dict.TryAdd(entry.ItemId, amount))
                dict[entry.ItemId] += amount;
        }

        var rows = sheetHandler.CreateRowDataList(dict, total);
        sheetHandler.SetDataAndFormat(sheetName, rows, column);
    }

    public void ReadFragmentData(Models.Gacha[] data) {
        Dictionary<uint, List<uint>> dict = [];
        foreach (var entry in data)
        {
            if (!dict.TryAdd(entry.Coffer, [entry.ItemId]))
                dict[entry.Coffer].Add(entry.ItemId);
        }

        List<RowData> rows = [new()
        {
            Values = new List<CellData>
            {
                new() { UserEnteredValue = StringValue("Logogram") },
                new() { UserEnteredValue = StringValue("Item") },
                new() { UserEnteredValue = StringValue("Obtained") },
                new() { UserEnteredValue = StringValue("Percentage") },
            }
        }];

        foreach (var (fragment, entries) in dict.OrderBy(pair => pair.Key))
        {
            var cofferData = new Dictionary<uint, double>();

            foreach (var entry in entries)
            {
                if (!cofferData.TryAdd(entry, 1))
                    cofferData[entry]++;
            }

            foreach (var (item, amount) in cofferData)
            {
                rows.Add(new RowData
                {
                    Values = new List<CellData>
                    {
                        new() { UserEnteredValue = StringValue(Sheets.ItemSheet.GetRow(fragment).Name.ExtractText()) },
                        new() { UserEnteredValue = StringValue(Sheets.ItemSheet.GetRow(item).Name.ExtractText()) },
                        new() { UserEnteredValue = NumberValue(amount) },
                        new() { UserEnteredValue = NumberValue(amount / entries.Count), UserEnteredFormat = PercentageFormat },
                    }
                });
            }
        }

        var sheet = sheetHandler.Spreadsheet.Sheets.First(s => s.Properties.Title == "Fragment");
        var batch = new BatchUpdateSpreadsheetRequest
        {
            Requests = new List<Request>
            {
                new()
                {
                    UpdateCells = new UpdateCellsRequest
                    {
                        Rows = rows,
                        Fields = "*",

                        Range = new GridRange
                        {
                            SheetId = sheet.Properties.SheetId,
                            StartColumnIndex = 0,
                            EndColumnIndex = 4,
                            StartRowIndex = 0,
                            EndRowIndex = rows.Count,
                        },
                    }
                }
            }
        };

        var batchRequest = sheetHandler.Service.Spreadsheets.BatchUpdate(batch, SheetHandler.SpreadsheetId);
        batchRequest.Execute();
    }
}
