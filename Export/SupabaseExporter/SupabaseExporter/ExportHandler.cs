using Newtonsoft.Json;

namespace SupabaseExporter;

public static class ExportHandler
{
    private const string WebsitePath = "../../../../../../Website";
    private const string AssetsPath = "assets/data";
    
    public static void WriteTimestamp()
    {
        WriteDataJson("LastUpdate.json", DateTime.UtcNow.ToString("R"));
    }
    
    public static void WriteDataJson<T>(string filename, T data)
    {
        File.WriteAllText($"{WebsitePath}/{AssetsPath}/{filename}", JsonConvert.SerializeObject(data));
    }
}