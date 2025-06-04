using Lumina.Excel.Sheets;

namespace SupabaseExporter;

public static class IconHelper
{
    /// <summary>
    /// All item and icon IDs used by the website.
    /// </summary>
    private static readonly HashSet<(uint, uint)> UsedItems = [];

    /// <summary>
    /// Build a JSON containing all icon paths that are used.
    /// </summary>
    public static void CreateIconPaths()
    {
        var iconPaths = new Dictionary<uint, string>();
        foreach (var (itemId, iconId) in UsedItems)
            iconPaths[itemId] = Utils.GetIconPath(iconId);

        ExportHandler.WriteDataJson("IconPaths.json", iconPaths);
    }

    /// <summary>
    /// Adds the item and icon IDs to a hashset for deduplication.
    /// </summary>
    /// <param name="item">The item to get the IDs from.</param>
    public static void AddItem(Item item) => 
        UsedItems.Add((item.RowId, Utils.CheckItemAction(item)));
}