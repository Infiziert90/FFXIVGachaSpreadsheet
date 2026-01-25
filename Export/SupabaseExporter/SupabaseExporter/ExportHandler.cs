using Newtonsoft.Json;

namespace SupabaseExporter;

public static class ExportHandler
{
    private const string WebsitePath = "../../../../../../website";
    private const string AssetsPath = "static/data";
    private const string SheetsPath = "static/sheets";

    public static string ReadDataJson(string filename)
    {
        var fileInfo = new FileInfo($"{WebsitePath}/{AssetsPath}/{filename}");
        if (!fileInfo.Exists)
            return string.Empty;
        
        return File.ReadAllText(fileInfo.FullName);
    }
    
    public static void WriteTimestamp()
    {
        WriteDataJson("LastUpdate.json", DateTime.UtcNow.ToString("R"));
    }
    
    public static void WriteDataJson<T>(string filename, T data)
    {
        File.WriteAllText($"{WebsitePath}/{AssetsPath}/{filename}", JsonConvert.SerializeObject(data));
    }
    
    public static void WriteSheetJson<T>(string filename, T data)
    {
        File.WriteAllText($"{WebsitePath}/{SheetsPath}/{filename}", JsonConvert.SerializeObject(data));
    }
}