namespace SupabaseExporter;

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

public enum VentureTypes : uint
{
    QuickVenture = 395,
    QuickVentureMaxLevel = 100_000,
    QuickVentureMinLevel = 200_000
}


public static class EnumExtensions
{
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