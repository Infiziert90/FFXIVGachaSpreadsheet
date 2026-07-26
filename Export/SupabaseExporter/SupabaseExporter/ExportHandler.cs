using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace SupabaseExporter;

public static class ExportHandler
{
    private const string WebsitePath = "../../../../../../website";
    private const string AssetsPath = "static/data";
    private const string SheetsPath = "static/sheets";
    private const string WebDataPath = "static/website";

    public static string ReadDataJson(string filename)
    {
        var file = new FileInfo(Path.Combine(WebsitePath, AssetsPath, filename));
        return file.Exists ? File.ReadAllText(file.FullName) : string.Empty;
    }
    
    public static void WriteTimestamp()
    {
        WriteDataJson("LastUpdate.json", DateTime.UtcNow.ToString("R"));
    }
    
    public static void WriteDataJson<T>(string filename, T data, bool withIndent = false)
    {
        var file = new FileInfo(Path.Combine(WebsitePath, AssetsPath, filename));
        if (file.DirectoryName != null && !Directory.Exists(file.DirectoryName))
            Directory.CreateDirectory(file.DirectoryName);
        
        File.WriteAllText(file.FullName, JsonConvert.SerializeObject(data, withIndent ? Formatting.Indented : Formatting.None));
    }
    
    public static void WriteWebJson<T>(string filename, T data)
    {
        var file = new FileInfo(Path.Combine(WebsitePath, WebDataPath, filename));
        if (file.DirectoryName != null && !Directory.Exists(file.DirectoryName))
            Directory.CreateDirectory(file.DirectoryName);
        
        var bDate = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        byte[] compressedData;

        using (var outStream = new MemoryStream())
        {
            using (var compressor = new GZipStream(outStream, CompressionLevel.Optimal, leaveOpen: true))
            {
                compressor.Write(bDate);
            }

            outStream.Seek(0, SeekOrigin.Begin);

            compressedData = outStream.ToArray();
        }
        
        File.WriteAllBytes($"{file.FullName}.gz", compressedData);
    }
    
    public static void WriteSheetJson<T>(string filename, T data)
    {
        File.WriteAllText(Path.Combine(WebsitePath, SheetsPath, filename), JsonConvert.SerializeObject(data));
    }
}