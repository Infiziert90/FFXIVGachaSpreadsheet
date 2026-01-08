using System.Numerics;
using SupabaseExporter.Structures.Exports;

namespace SupabaseExporter.Processing.BnpcPairs;

public class BnpcPairs : IDisposable
{
    private readonly HashSet<string> DeduplicationSet = [];
    private readonly BnpcPairing ProcessedData = new();
    
    public void ProcessAllData(Models.BnpcPair[] data)
    {
        Logger.Information("Processing BnpcPair data");
        Fetch(data);
        Combine();
        Export();
        Dispose();
    }
    public void Dispose()
    {
        DeduplicationSet.Clear();
        ProcessedData.BnpcPairings.Clear();
        GC.Collect();
    }
    
    private void Fetch(Models.BnpcPair[] data)
    {
        foreach (var record in data)
        {
            if (!DeduplicationSet.Add(record.Hashed))
                continue;
            
            if (!ProcessedData.BnpcPairings.ContainsKey(record.BaseId))
                ProcessedData.BnpcPairings[record.BaseId] = new BnpcPairing.Pairing();
            
            var pairing = ProcessedData.BnpcPairings[record.BaseId];
            pairing.Records += 1;
            pairing.Names.Add(record.NameId);

            if (!pairing.LevelToLocations.ContainsKey(record.LevelId))
                pairing.LevelToLocations[record.LevelId] = new BnpcPairing.Location(record.TerritoryId, record.MapId);
            
            var location = pairing.LevelToLocations[record.LevelId];
            location.Records += 1;
            
            var position = new Vector3(record.X, record.Y, record.Z);

            var found = Vector3.Zero;
            foreach (var existingPosition in location.Positions.Keys)
            {
                var difV = existingPosition - position;
                var dis = Math.Sqrt(Math.Pow(difV.X, 2f) + Math.Pow(difV.Y, 2f) + Math.Pow(difV.Z, 2f));
                
                if (dis < 5.0)
                    found = existingPosition;
            }

            if (found != Vector3.Zero)
                location.Positions[found] += 1;
            else
                location.Positions[position] = 1;
        }
    }

    private void Combine() 
    {
        foreach (var (baseId, pairing) in ProcessedData.BnpcPairings)
        {
            if (pairing.Names.Count == 0)
                Logger.Error($"Found empty name list? {baseId}");
        }
    }
    
    public record BnpcSimple(uint Base, HashSet<uint> Names);
    
    private void Export()
    {
        ExportHandler.WriteDataJson("BnpcPairs.json", ProcessedData);
        ExportHandler.WriteDataJson("BnpcPairsSimple.json", ProcessedData.BnpcPairings.Select(pair => new BnpcSimple(pair.Key, pair.Value.Names)));
        Logger.Information("Done exporting data ...");
    }
}