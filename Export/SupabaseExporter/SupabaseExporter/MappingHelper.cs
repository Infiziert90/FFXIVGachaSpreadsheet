namespace SupabaseExporter;

/// <summary>
/// Mapping for item id to name and XIVAPI icon path.
/// </summary>
[Serializable]
public record Mappings(string Name, string Icon);

public static class MappingHelper
{
    /// <summary>
    /// All item ids used on the website.
    /// </summary>
    private static readonly HashSet<uint> ItemSet = [];

    /// <summary>
    /// Build a JSON containing all mappings that are used.
    /// </summary>
    public static void CreateIconPaths()
    {
        var mappings = new Dictionary<uint, Mappings>();
        foreach (var itemId in ItemSet)
        {
            var item = Sheets.ItemSheet.GetRow(itemId);
            mappings[itemId] = new Mappings(item.Name.ToString(), Utils.GetIconPath(Utils.CheckItemAction(item)));
        }
        
        ExportHandler.WriteDataJson("Mappings.json", mappings);
    }

    /// <summary>
    /// Adds the item id to the mapping set for later deduplication.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    public static void AddItem(uint itemId) =>
        ItemSet.Add(itemId);
}