using System.Text;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace SupabaseExporter;

public static class Utils
{
    public static readonly int Patch730 = VersionToNumber("1.6.1.0");
    public static readonly int Patch720 = VersionToNumber("1.5.8.1");
    
    /// <summary>
    /// Convert a version string to a number.
    /// </summary>
    /// <param name="version">Version as string, separated by dots.</param>
    /// <returns>Version as number</returns>
    public static int VersionToNumber(string version)
    {
        if (string.IsNullOrEmpty(version))
            return 0;

        var result = 0;
        var multiplier = 1000000;
        var splits = version.AsSpan();
        foreach (var numberRange in splits.Split('.'))
        {
            result += int.Parse(splits[numberRange]) * multiplier;
            multiplier /= 100;
        }
        
        return result;
    }
    
    /// <summary>
    /// Match a specific version number to patch name.
    /// </summary>
    /// <param name="version">Version as number</param>
    /// <returns>The patch name, with default 7.10</returns>
    public static string VersionToPatch(int version)
    {
        if (version >= Patch730)
            return "7.3X";
        
        if (version >= Patch720)
            return "7.2X";
        
        return "7.1X";
    }
    
    /// <summary>
    /// Return the ui path for usage with XIVAPI.
    /// </summary>
    /// <param name="iconId">The items icon id</param>
    /// <returns>Game path to the icon</returns>
    public static string GetIconPath(uint iconId){
        var iconGroup = iconId - (iconId % 1000);
        return $"{iconGroup:D6}/{iconId:D6}";
    }
    
    /// <summary>
    /// Check Item for resolving ItemAction.
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>Item icon, or resolved icon</returns>
    public static uint CheckItemAction(Item item)
    {
        if (item.ItemAction.RowId == 0)
            return item.Icon;

        var itemAction = item.ItemAction.Value;
        return itemAction.Type switch
        {
            1322 => Sheets.MountSheet.GetRow(itemAction.Data[0]).Icon, // Mount ID
            3357 => 87000 + (uint)itemAction.Data[0], // Triple Triad ID
            _ => item.Icon,
        };
    }
    
    /// <summary>
    /// Calculates the number of FC points a trade-in item would give.
    /// </summary>
    /// <param name="item">Item to be turned in</param>
    /// <returns>Number of FC points generated</returns>
    public static double CalculateFCPoints((Item Item, bool HQ) item)
    {
        var iLvL = item.Item.LevelItem.RowId;
        if ((iLvL & 1) == 1)
            iLvL += 1;

        return (item.HQ ? 3.0 : 1.5) * iLvL;
    }
    
    
    /// <summary>
    /// Correctly upper case a Lumina SeString without access to the game internal SeStringEvaluator.
    /// </summary>
    /// <param name="s">Text from the sheet</param>
    /// <param name="article">Article specified in the sheet</param>
    /// <returns></returns>
    public static string UpperCaseStr(ReadOnlySeString s, sbyte article = 0)
    {
        if (article == 1)
            return s.ExtractText();

        var sb = new StringBuilder(s.ExtractText());
        var lastSpace = true;
        for (var i = 0; i < sb.Length; ++i)
        {
            if (sb[i] == ' ')
            {
                lastSpace = true;
            }
            else if (lastSpace)
            {
                lastSpace = false;
                sb[i]     = char.ToUpperInvariant(sb[i]);
            }
        }

        return sb.ToString();
    }
}