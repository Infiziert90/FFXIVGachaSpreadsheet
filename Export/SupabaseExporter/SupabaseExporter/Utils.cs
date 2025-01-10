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

    public enum Territory : uint
    {
        Anemos = 732,
        Pagos = 763,
        Pyros = 795,
        Hydatos = 827,

        Bozja = 920,
    }

    public enum CofferRarity : uint
    {
        Gold = 2009530,
        Silver = 2009531,
        Bronze = 2009532
    }

    public enum DeepDungeon : uint
    {
        PotD = 1,
        HoH = 2,
        Eo = 3,
    }

    public enum Coffer : uint
    {
        Any = 1,
    }

    public enum LockboxTypes : uint
    {
        // Eureka
        Anemos = 22508,
        Pagos = 23142,
        ColdWarped = 23379,
        Pyros = 24141,
        HeatWarped = 24142,
        Hydatos = 24848,
        MoistureWarped = 24849,

        // Bozja
        SouthernFront = 31357,
        Zadnor = 33797,
    }

    public static string ToName(this Territory territory)
    {
        return territory switch
        {
            Territory.Anemos => "Anemos",
            Territory.Pagos => "Pagos",
            Territory.Pyros => "Pyros",
            Territory.Hydatos => "Hydatos",
            Territory.Bozja => "Bozja",
            _ => "Unknown"
        };
    }

    public static string ToName(this CofferRarity rarity)
    {
        return rarity switch
        {
            CofferRarity.Bronze => "Bronze",
            CofferRarity.Silver => "Silver",
            CofferRarity.Gold => "Gold",
            _ => "Unknown"
        };
    }

    public static string ToName(this DeepDungeon territory)
    {
        return territory switch
        {
            DeepDungeon.PotD => "Palace of the Dead",
            DeepDungeon.HoH => "Heaven-on-High",
            DeepDungeon.Eo => "Eureka Orthos",
            _ => "Unknown"
        };
    }

    public static string ToName(this Coffer territory)
    {
        return territory switch
        {
            Coffer.Any => "Coffer",
            _ => "Unknown"
        };
    }

    public static DeepDungeon ToDeepDungeon(uint coffer)
    {
        return coffer switch
        {
            16170 or 16171 or 16172 or 16173 => DeepDungeon.PotD,
            23223 or 23224 or 23225 => DeepDungeon.HoH,
            38945 or 38946 or 38947 => DeepDungeon.Eo,
            _ => throw new ArgumentOutOfRangeException(nameof(coffer), coffer, null)
        };
    }

    public static Territory ToTerritory(this LockboxTypes lockboxType)
    {
        return lockboxType switch
        {
            LockboxTypes.Anemos => Territory.Anemos,
            LockboxTypes.Pagos or LockboxTypes.ColdWarped => Territory.Pagos,
            LockboxTypes.Pyros or LockboxTypes.HeatWarped => Territory.Pyros,
            LockboxTypes.Hydatos or LockboxTypes.MoistureWarped => Territory.Hydatos,
            LockboxTypes.SouthernFront or LockboxTypes.Zadnor => Territory.Bozja,
            _ => throw new ArgumentOutOfRangeException(nameof(lockboxType), lockboxType, null)
        };
    }
}