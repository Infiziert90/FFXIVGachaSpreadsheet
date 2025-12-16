using Newtonsoft.Json;

namespace SupabaseExporter;

public static class ExportHandler
{
    private const string WebsitePath = "../../../../../../website";
    private const string AssetsPath = "static/data";
    
    public static void WriteTimestamp()
    {
        WriteDataJson("LastUpdate.json", DateTime.UtcNow.ToString("R"));
    }
    
    public static void WriteDataJson<T>(string filename, T data)
    {
        File.WriteAllText($"{WebsitePath}/{AssetsPath}/{filename}", JsonConvert.SerializeObject(data));
    }
}