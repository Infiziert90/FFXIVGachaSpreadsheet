using SupabaseExporter.Structures.Exports;
using SupabaseExporter.Structures.Temps;

namespace SupabaseExporter.Processing.Coffers;

public abstract class CofferBase : IDisposable
{
    public readonly List<Coffer> ProcessedData = [];
    public readonly Dictionary<uint, Dictionary<uint, Dictionary<string, CofferTemp>>> CollectedData = [];
    
    public void Export(string name)
    {
        ExportHandler.WriteDataJson(name, ProcessedData.OrderBy(l => l.TerritoryId));
        Logger.Information("Done exporting data ...");
    }

    public void Dispose()
    {
        ProcessedData.Clear();
        CollectedData.Clear();
        GC.Collect();
    }
}