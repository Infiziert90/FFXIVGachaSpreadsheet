using System.Runtime.CompilerServices;
using Google.Apis.Sheets.v4.Data;
using Lumina.Data.Files;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;

namespace SupabaseExporter;

public static class Utils
{
    public static ValueInputOptionEnum InputOption => ValueInputOptionEnum.USERENTERED;
    public static ValueRange SimpleValueRange(object content) => new() { Values = [[content]] };

    public static ExtendedValue StringValue(string value) => new() { StringValue = value };
    public static ExtendedValue NumberValue(double value) => new() { NumberValue = value };
    public static CellFormat PercentageFormat => new() { NumberFormat = new NumberFormat { Type = "NUMBER", Pattern = "##0.00%" } };

    /// <summary>
    /// Returns the image data.
    /// </summary>
    /// <param name="texFile">The TexFile to format.</param>
    /// <returns>The formatted image data.</returns>
    public static byte[] GetRgbaImageData(this TexFile texFile)
    {
        var imageData = texFile.ImageData;
        var dst = new byte[imageData.Length];

        for (var i = 0; i < dst.Length; i += 4)
        {
            dst[i] = imageData[i + 2];
            dst[i + 1] = imageData[i + 1];
            dst[i + 2] = imageData[i];
            dst[i + 3] = imageData[i + 3];
        }

        return dst;
    }

    /// <summary> Iterate over enumerables with additional index. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static IEnumerable<(T Value, int Index)> WithIndex<T>(this IEnumerable<T> list)
        => list.Select((x, i) => (x, i));
}