using Newtonsoft.Json;

namespace SupabaseExporter;

public static class ExportHandler
{
    private const string WebsitePath = "../../../../../../Website";
    private const string AssetsPath = "assets/data";
    
    public static async Task WriteTimestamp()
    {
        await WriteDataJson("LastUpdate.json", DateTime.UtcNow.ToString("R"));
    }
    
    public static async Task WriteDataJson<T>(string filename, T data)
    {
        await File.WriteAllTextAsync($"{WebsitePath}/{AssetsPath}/{filename}", JsonConvert.SerializeObject(data));
    }
}