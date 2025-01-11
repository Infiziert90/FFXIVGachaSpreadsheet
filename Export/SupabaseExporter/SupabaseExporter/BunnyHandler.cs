using static SupabaseExporter.Utils;

namespace SupabaseExporter;

public class BunnyHandler(SheetHandler SheetHandler)
{
    public void ReadBunnyData(List<Models.Bnuuy> data, string sheetName, uint terri, uint target, int column = 3) {
        var total = 0.0;
        Dictionary<uint, double> dict = [];
        foreach (var entry in data)
        {
            if (entry.Territory != terri || entry.Coffer != target)
                continue;

            total++;
            foreach (var item in entry.Items)
                if (!dict.TryAdd(item, 1))
                    dict[item]++;
        }

        var rows = SheetHandler.CreateRowDataList(dict, total);
        SheetHandler.SetDataAndFormat(sheetName, rows, column);

        var request = SheetHandler.Service.Spreadsheets.Values.Update(SimpleValueRange(total), SheetHandler.SpreadsheetId, $"{sheetName}!B{2009534 - target}");
        request.ValueInputOption = InputOption;
        request.Execute();
    }
}