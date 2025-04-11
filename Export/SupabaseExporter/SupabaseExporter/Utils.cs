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

    public static int VersionToNumber(string version)
    {
        if (string.IsNullOrEmpty(version))
            return 0;
        
        var splits = version.Split('.');
        return int.Parse(splits[0]) * 1000 + int.Parse(splits[1]) * 100 + int.Parse(splits[2]) * 10 + int.Parse(splits[3]);
    }

    public static string[] AllKnownPatches()
    {
        return ["All", "7.20", "7.10"];
    }
    
    public static string VersionToPatch(int version)
    {
        if (version >= VersionToNumber("1.5.8.1"))
            return "7.20";
        
        return "7.10";
    }
    
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