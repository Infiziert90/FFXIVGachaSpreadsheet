using static SupabaseExporter.Utils;

namespace SupabaseExporter;

public class BunnyHandler(SheetHandler sheetHandler)
{
    public void ReadBunnyData(List<Models.Bnuuy> data, string sheetName, uint terri, uint target, int column = 3) {
        var total = 0.0;
        Dictionary<uint, double> dict = [];
        foreach (var entry in data.Where(b => b.Territory == terri && b.Coffer == target))
        {
            total++;
            foreach (var item in entry.GetItems())
                if (!dict.TryAdd(item, 1))
                    dict[item]++;
        }

        sheetHandler.SetDataAndFormat(sheetName, sheetHandler.CreateRowDataList(dict, total), column);

        var request = sheetHandler.Service.Spreadsheets.Values.Update(SimpleValueRange(total), SheetHandler.SpreadsheetId, $"{sheetName}!B{2009534 - target}");
        request.ValueInputOption = InputOption;
        request.Execute();
    }
}