namespace SupabaseExporter;

#region Submarine
public enum SurvTier : uint
{
    Invalid = 0,
    
    Tier1 = 1,
    Tier2 = 2,
    Tier3 = 3,
}

public enum RetTier : uint
{
    Invalid = 0,
    
    Poor = 1,
    Normal = 2,
    Optimal = 3,
}

public enum RetrievalProc : uint
{
    Optimal = 14,
    Normal = 15,
    Poor = 16
}

public enum SurveillanceProc : uint
{
    T3High = 4,
    T2High = 5,
    T1High = 6,
    T2Mid = 7,
    T1Mid = 8,
    T1Low = 9
}

public enum FavorProc : uint
{
    Yes = 18,
    StatsEnoughButFailed = 19,
    Low = 20
}
#endregion

public enum Territory : uint
{
    Anemos = 732,
    Pagos = 763,
    Pyros = 795,
    Hydatos = 827,

    Bozja = 920,
    
    SouthHorn = 1252
}

public enum OccultCategory : uint
{
    Treasure = 1,
    Pot = 2,
    Bunny = 3
}

public enum CofferRarity : uint
{
    Gold = 2009530,
    Silver = 2009531,
    Bronze = 2009532,
    
    OccultTreasureBronze = 1596,
    OccultTreasureSilver = 1597,
    
    OccultPotGold = 2014741,
    OccultPotSilver = 2014742,
    OccultPotBronze = 2014743,
    
    BunnyGold = 2012936,
}

public enum DeepDungeon : uint
{
    PotD = 1,
    HoH = 2,
    Eo = 3,
    Pt = 4,
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
    QuickVentureMaxLevel = 100_000,
    QuickVentureMinLevel = 200_000
}

public enum LogoFrag : uint
{
    Logogram = 1,
    Fragment = 2,
}


public static class EnumExtensions
{
    public static readonly VentureTypes[] ExtraQuickVentureTypes = [VentureTypes.QuickVentureMaxLevel, VentureTypes.QuickVentureMinLevel];
    
    public static string ToName(this Territory territory)
    {
        return territory switch
        {
            Territory.Anemos => "Anemos",
            Territory.Pagos => "Pagos",
            Territory.Pyros => "Pyros",
            Territory.Hydatos => "Hydatos",
            Territory.Bozja => "Bozja",
            Territory.SouthHorn => "South Horn",
            _ => "Unknown"
        };
    }

    public static string ToName(this CofferRarity rarity)
    {
        return rarity switch
        {
            CofferRarity.Bronze or CofferRarity.OccultTreasureBronze or CofferRarity.OccultPotBronze => "Bronze",
            CofferRarity.Silver or CofferRarity.OccultTreasureSilver or CofferRarity.OccultPotSilver => "Silver",
            CofferRarity.Gold or CofferRarity.OccultPotGold or CofferRarity.BunnyGold => "Gold",
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
            DeepDungeon.Pt => "Pilgrim's Traverse",
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
            47104 or 47105 or 47106 or 47742 => DeepDungeon.Pt,
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
    
    public static OccultCategory ToCategory(this uint cofferRarity)
    {
        return cofferRarity switch
        {
            2014741 or 2014742 or 2014743 => OccultCategory.Pot,
            2012936 => OccultCategory.Bunny,
            _ => throw new ArgumentOutOfRangeException(nameof(cofferRarity), cofferRarity, null)
        };
    }
    
    public static string ToName(this OccultCategory category)
    {
        return category switch
        {
            OccultCategory.Treasure => "Treasure",
            OccultCategory.Pot => "Pot",
            OccultCategory.Bunny => "Bunny",
            _ => "Unknown"
        };
    }

    public static LogoFrag ToLogoFrag(uint coffer)
    {
        return coffer switch
        {
            >= 24007 and <= 24809 => LogoFrag.Logogram,
            >= 30884 and <= 33779 => LogoFrag.Fragment,
            _ => throw new ArgumentOutOfRangeException(nameof(coffer), coffer, null)
        };
    }
    
    public static string ToArea(this LogoFrag logoFrag)
    {
        return logoFrag switch
        {
            LogoFrag.Logogram => "Logograms",
            LogoFrag.Fragment => "Fragments",
            _ => throw new ArgumentOutOfRangeException(nameof(logoFrag), logoFrag, null)
        };
    }
}