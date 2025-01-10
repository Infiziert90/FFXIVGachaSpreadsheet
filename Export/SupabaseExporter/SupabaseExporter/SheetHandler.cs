using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using static SupabaseExporter.Utils;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;

namespace SupabaseExporter;

public class SheetHandler
{
    internal const string SpreadsheetId = "1VfncSL5gf9E7ehgND5nZgguUyUAmZiAMbQllLKcoxTQ";

    public readonly GachaHandler GachaHandler;
    public readonly BunnyHandler BunnyHandler;

    internal readonly SheetsService Service;
    internal readonly Spreadsheet Spreadsheet;

    public SheetHandler()
    {
        var credential = GoogleCredential.FromJson(Environment.GetEnvironmentVariable("google_auth")).CreateScoped(SheetsService.Scope.Spreadsheets);
        Service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "FFXIV Gacha Spreadsheet"
        });

        Spreadsheet = Service.Spreadsheets.Get(SpreadsheetId).Execute();

        GachaHandler = new GachaHandler(this);
        BunnyHandler = new BunnyHandler(this);
    }

    public void SetTime()
    {
        var request = Service.Spreadsheets.Values.Update(SimpleValueRange(DateTime.UtcNow.ToString("R")), SpreadsheetId, "Info!F3");
        request.ValueInputOption = ValueInputOptionEnum.USERENTERED;
        request.Execute();
    }

    internal List<RowData> CreateRowDataList(Dictionary<uint, double> dict, double total)
    {
        List<RowData> rows = [new()
        {
            Values = new List<CellData>
            {
                new() { UserEnteredValue = StringValue("Name") },
                new() { UserEnteredValue = StringValue("Obtained") },
                new() { UserEnteredValue = StringValue("Percentage") },
            }
        }];

        foreach (var (key, value) in dict.OrderBy(pair => pair.Value))
        {
            rows.Add(new RowData
            {
                Values = new List<CellData>
                {
                    new() { UserEnteredValue = StringValue(Sheets.ItemSheet.GetRow(key).Name.ExtractText()) },
                    new() { UserEnteredValue = NumberValue(value) },
                    new() { UserEnteredValue = NumberValue(value / total), UserEnteredFormat = PercentageFormat },
                }
            });
        }

        return rows;
    }

    internal void SetDataAndFormat(string sheetName, List<RowData> rows, int column)
    {
        var sheet = Spreadsheet.Sheets.First(s => s.Properties.Title == sheetName);
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
                            StartColumnIndex = column,
                            EndColumnIndex = column + 3,
                            StartRowIndex = 0,
                            EndRowIndex = rows.Count,
                        },
                    }
                }
            }
        };

        var batchRequest = Service.Spreadsheets.BatchUpdate(batch, SpreadsheetId);
        batchRequest.Execute();
    }
}