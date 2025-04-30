namespace SupabaseExporter;

public static class Utils
{
    public static readonly int CurrentPatchNumber = VersionToNumber("1.5.8.1");
    public static readonly string[] KnownPatches = ["All", "7.20", "7.10"];
    
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
        if (version >= CurrentPatchNumber)
            return "7.20";
        
        return "7.10";
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
}