namespace SupabaseExporter.Structures;

public class CofferBase : IDisposable
{
    internal readonly List<CofferData> ProcessedData = [];
    internal readonly Dictionary<uint, Dictionary<uint, Dictionary<string, CofferTemp>>> CollectedData = [];
    
    internal void Export(string name)
    {
        ExportHandler.WriteDataJson(name, ProcessedData.OrderBy(l => l.Territory));
        Console.WriteLine("Done exporting data ...");
    }

    public void Dispose()
    {
        ProcessedData.Clear();
        CollectedData.Clear();
        GC.Collect();
    }
}