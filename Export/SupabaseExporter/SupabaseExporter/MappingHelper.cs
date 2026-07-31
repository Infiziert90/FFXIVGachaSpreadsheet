namespace SupabaseExporter;

/// <summary>
/// Mapping for item id to name and XIVAPI icon path.
/// </summary>
[Serializable]
public record ItemEntry(string En, string Fr, string De, string Ja, string Icon);

public static class MappingHelper
{
    /// <summary>
    /// All item ids used on the website.
    /// </summary>
    private static readonly HashSet<uint> ItemSet = [];

    /// <summary>
    /// Build a JSON containing all mappings that are used.
    /// </summary>
    public static void ExportMappingFile()
    {
        var mappings = new Dictionary<uint, ItemEntry>();
        foreach (var itemId in ItemSet.Order())
        {
            var item = Sheets.ItemSheet.GetRow(itemId);
            var en = item.Name.ToString();
            var fr = Sheets.ItemSheetFrench.GetRow(itemId).Name.ToString();
            var de = Sheets.ItemSheetGerman.GetRow(itemId).Name.ToString();
            var ja = Sheets.ItemSheetJapanese.GetRow(itemId).Name.ToString();
            mappings[itemId] = new ItemEntry(en, fr, de, ja, Utils.GetIconPath(Utils.CheckItemAction(item)));
        }
        
        ExportHandler.WriteMappingJson("Items.json", mappings);
    } 

    /// <summary>
    /// Adds the item id to the mapping set for later deduplication.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    public static void AddItem(uint itemId) =>
        ItemSet.Add(itemId);
}