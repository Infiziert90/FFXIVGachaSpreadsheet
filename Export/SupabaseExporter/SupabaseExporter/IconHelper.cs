using Lumina.Excel.Sheets;
using Lumina.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace SupabaseExporter;

public static class IconHelper
{
    private const string ImagePath = "../../../../../../Website/static/images/";
    private static readonly Dictionary<string, bool> ExistingFiles = new();

    /// <summary>
    /// Checks the internal cache, if not exist, creates the icon in the website static folder.
    /// </summary>
    /// <param name="item">The item to get the icon from.</param>
    public static async Task CreateIcon(Item item)
    {
        // Fill the cache with existing icons
        if (ExistingFiles.Count == 0)
        {
            foreach (var file in new DirectoryInfo(ImagePath).GetFiles("*.png"))
                ExistingFiles[file.Name] = true;
        }

        if (ExistingFiles.ContainsKey($"{item.Icon}.png"))
            return;

        Console.WriteLine($"Creating icon? {item.Icon}");
        if (File.Exists($"{ImagePath}{item.Icon}.png"))
        {
            Console.WriteLine("Icon already existed?");
            return;
        }

        ExistingFiles[$"{item.Icon}.png"] = true;

        var icon = Sheets.Lumina.GetIcon(item.Icon);
        if (icon != null)
        {
            using var image = Image.LoadPixelData<Rgba32>(icon.GetRgbaImageData(), icon.Header.Width, icon.Header.Height);
            await image.SaveAsPngAsync($"{ImagePath}{item.Icon}.png", new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression });
        }
    }
}