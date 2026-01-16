using System.Numerics;
using SupabaseExporter.Structures.Exports;

namespace SupabaseExporter.Processing.BnpcPairs;

public class BnpcPairs : IDisposable
{
    public readonly BnpcPairing CollectedData = new();
    
    private readonly HashSet<string> DeduplicationSet = [];
    private readonly Dictionary<uint, BnpcSimple> SimplePairs = [];
    
    public void ProcessAllData()
    {
        Logger.Information("Processing BnpcPair data");
        Combine();
        Export();
        Dispose();
    }
    public void Dispose()
    {
        SimplePairs.Clear();
        DeduplicationSet.Clear();
        CollectedData.BnpcPairings.Clear();
        GC.Collect();
    }
    
    public void Fetch(IEnumerable<Models.BnpcPairModel> data)
    {
        foreach (var record in data)
        {
            if (record.Id <= CollectedData.ProcessedId)
                continue;
            
            CollectedData.ProcessedId = record.Id;

            if (Sheets.DisallowedBnpcNames.Contains(record.NameId) || Sheets.DisallowedBnpcBase.Contains(record.BaseId))
                continue;
            
            if (!DeduplicationSet.Add(record.Hashed))
                continue;

            // Both are sheet index, so a fixed uint number, but NameId has flag properties so it goes into the millions at times
            var combinedId = ((ulong)record.BaseId << 32) + record.NameId;
            if (!CollectedData.BnpcPairings.ContainsKey(combinedId))
                CollectedData.BnpcPairings[combinedId] = new BnpcPairing.Pairing(record.BaseId, record.NameId, record.ObjectKind, record.Battalion);
            
            var pairing = CollectedData.BnpcPairings[combinedId];
            pairing.Records += 1;
            
            pairing.Kind = record.ObjectKind; // We overwrite this every time because of old data having just 0 for it
            pairing.Battalion = record.Battalion;

            if (!pairing.Locations.ContainsKey(record.LevelId))
                pairing.Locations[record.LevelId] = new BnpcPairing.Location(record.TerritoryId, record.MapId, record.Level);
            
            var location = pairing.Locations[record.LevelId];
            location.Records += 1;
            
            var position = new Vector3(record.X, record.Y, record.Z);

            var found = -1;
            foreach (var (idx, existingPosition) in location.Positions.Index())
            {
                var difV = existingPosition - position;
                var dis = Math.Sqrt(Math.Pow(difV.X, 2f) + Math.Pow(difV.Y, 2f) + Math.Pow(difV.Z, 2f));
                
                if (dis < (Sheets.RankedBnpcBase.Contains(record.BaseId) ? 50.0 : 20.0))
                    found = idx;
            }

            if (found != -1)
            {
                location.PositionCounts[found] += 1;
            }
            else
            {
                var newIdx = location.Positions.Count;
                location.Positions.Add(position);
                
                location.PositionCounts[newIdx] = 1;
            }
        }
    }

    private void Combine() 
    {
        foreach (var pair in CollectedData.BnpcPairings.Values)
        {
            if (!SimplePairs.ContainsKey(pair.Base))
                SimplePairs[pair.Base] = new BnpcSimple(pair.Base, []);

            SimplePairs[pair.Base].Names.Add(pair.Name);
        }
    }
    
    private void Export()
    {
        ExportHandler.WriteDataJson("BnpcPairs.json", CollectedData);
        ExportHandler.WriteDataJson("BnpcPairsSimple.json", SimplePairs.Select(pair => pair.Value));
        Logger.Information("Done exporting data ...");
    }
}
