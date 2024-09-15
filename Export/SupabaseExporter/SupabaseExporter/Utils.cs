using Google.Apis.Sheets.v4.Data;

using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;

namespace SupabaseExporter;

public static class Utils
{
    public static ValueInputOptionEnum InputOption => ValueInputOptionEnum.USERENTERED;
    public static ValueRange SimpleValueRange(object content) => new() { Values = [[content]] };

    public static ExtendedValue StringValue(string value) => new() { StringValue = value };
    public static ExtendedValue NumberValue(double value) => new() { NumberValue = value };
    public static CellFormat PercentageFormat => new() { NumberFormat = new NumberFormat { Type = "NUMBER", Pattern = "##0.00%" } };
}