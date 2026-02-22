using Newtonsoft.Json;

namespace SupabaseExporter;

public static class ExportHandler
{
    private const string WebsitePath = "../../../../../../website";
    private const string AssetsPath = "static/data";
    private const string SheetsPath = "static/sheets";

    public static string ReadDataJson(string filename)
    {
        var file = new FileInfo(Path.Combine(WebsitePath, AssetsPath, filename));
        if (!file.Exists)
            return string.Empty;
        
        return File.ReadAllText(file.FullName);
    }
    
    public static void WriteTimestamp()
    {
        WriteDataJson("LastUpdate.json", DateTime.UtcNow.ToString("R"));
    }
    
    public static void WriteDataJson<T>(string filename, T data)
    {
        var file = new FileInfo(Path.Combine(WebsitePath, AssetsPath, filename));
        if (file.DirectoryName != null && !Directory.Exists(file.DirectoryName))
            Directory.CreateDirectory(file.DirectoryName);
        
        File.WriteAllText(file.FullName, JsonConvert.SerializeObject(data));
    }
    
    public static void WriteSheetJson<T>(string filename, T data)
    {
        File.WriteAllText(Path.Combine(WebsitePath, SheetsPath, filename), JsonConvert.SerializeObject(data));
    }
}