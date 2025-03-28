using System.Numerics;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SupabaseExporter;

public static class IconHelper
{
    /// <summary>
    /// Path to the website static folder.
    /// </summary>
    private const string ImagePath = "static/spritesheet";

    /// <summary>
    /// All icons used.
    /// </summary>
    private static readonly HashSet<ushort> UsedIcons = [];

    /// <summary>
    /// A simple helper for named JSON keys.
    /// </summary>
    private record Offset(int Width, int Height);

    /// <summary>
    /// Build a webp spritesheet for the website to use.
    /// </summary>
    public static async Task CreateSpriteSheet()
    {
        var existingOffsets = await DataHandler.ReadDataJson<Dictionary<ushort, Offset>>("SpritesheetOffsets.json");
        if (existingOffsets != null)
        {
            var existingIcons = existingOffsets.Select(pair => pair.Key).ToHashSet();
            if (UsedIcons.SetEquals(existingIcons))
            {
                Console.WriteLine("Icons equal, skip building spritesheet.");
                return;
            }

            // Print out new icons for debug purpose
            foreach (var icon in UsedIcons.Where(icon => !existingIcons.Contains(icon)))
                Console.WriteLine($"New icon found: {icon}");
        }

        var iconOffsets = new Dictionary<ushort, Offset>();

        // 2 pixel transparent padding for each item
        var width = 42 * 300; // 300 icons
        var height = 42 * (int)Math.Ceiling(UsedIcons.Count / 300.0);

        var offsetX = 0;
        var offsetY = 0;
        using var image = new Image<Rgba32>(width, height, new Rgba32(Vector4.Zero));
        foreach (var (icon, idx) in UsedIcons.WithIndex())
        {
            if (idx % 300 == 0 && idx != 0)
            {
                offsetX = 0;
                offsetY += 42;
            }

            var texFile = Sheets.Lumina.GetIcon(icon);
            if (texFile != null)
            {
                using var iconImage = Image.LoadPixelData<Rgba32>(texFile.GetRgbaImageData(), texFile.Header.Width, texFile.Header.Height);
                image.Mutate(i => i.DrawImage(iconImage, new Point(offsetX, offsetY), 1f));
                iconOffsets.Add(icon, new Offset(offsetX, offsetY));

                // move our draw cursor
                offsetX += texFile.Header.Width + 2; // 2 pixel padding
            }
        }

        await DataHandler.WriteDataJson("SpritesheetOffsets.json", iconOffsets);
        await image.SaveAsWebpAsync($"{DataHandler.WebsitePath}/{ImagePath}/spritesheet.webp", new WebpEncoder {Quality = 100});
    }

    /// <summary>
    /// Adds the icon to a hashset for later creation.
    /// </summary>
    /// <param name="item">The item to get the icon from.</param>
    public static void AddIcon(Item item) => 
        UsedIcons.Add(item.Icon);
}