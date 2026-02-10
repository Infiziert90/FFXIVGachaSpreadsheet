using Newtonsoft.Json;
using SupabaseExporter.Structures.Exports;

namespace SupabaseExporter.Processing.Submarines;

public class Submarines : IDisposable
{
    public readonly SubLoot CollectedData = new();
    private readonly HashSet<string> DeduplicationCache = [];

    public Submarines()
    {
        // Try to read internal cache to save up lots of processing
        var data = ExportHandler.ReadDataJson("Submarines.json");
        if (data == string.Empty)
            return;
        
        CollectedData = JsonConvert.DeserializeObject<SubLoot>(data) ?? new SubLoot();
    }
    
    public void ProcessAllData()
    {
        Logger.Information("Processing submarine loot data");
        Export();
        Dispose();
    }
    
    public void Dispose()
    {
        DeduplicationCache.Clear();
        CollectedData.Sectors.Clear();
        GC.Collect();
    }

    public void Fetch(IEnumerable<Models.SubmarineLootModel> data)
    {
        foreach (var record in data)
        {
            if (record.Id <= CollectedData.ProcessedId)
                continue;
            
            CollectedData.ProcessedId = record.Id;

            if (DeduplicationCache.Count > 5_000_000)
            {
                var last = DeduplicationCache.Last();
                DeduplicationCache.Clear();
                DeduplicationCache.Add(last);
            }
            
            if (!DeduplicationCache.Add(record.Hash))
            {
                Logger.Warning($"Duplicated hash found: {record.Id}");
                continue;
            }

            if (!CollectedData.Sectors.ContainsKey(record.Sector))
                CollectedData.Sectors[record.Sector] = new SubLoot.Sector(Sheets.ExplorationSheet.GetRow(record.Sector));

            var sectorData = CollectedData.Sectors[record.Sector];

            CollectedData.Total += 1;
            sectorData.Records += 1;

            var capable = GetCapableTier(record.PrimarySurvProc);
            if (capable == SurvTier.Tier3)
                sectorData.T3Capable += 1;
            
            var survTier = GetSurvTier(record.PrimarySurvProc);
            if (survTier == SurvTier.Invalid)
            {
                Logger.Error($"Invalid primary tier found: {record.Id}");
                continue;
            }
            
            var lootPool = sectorData.Pools[survTier];
            lootPool.AddRecord(record.Primary, record.PrimaryCount, GetRetTier(record.PrimaryRetProc), capable);
            
            lootPool.Stats.IncreaseSurveillance(record.PrimarySurvProc);
            lootPool.Stats.IncreaseRetrieval(record.PrimaryRetProc);
            lootPool.Stats.IncreaseFavor(record.FavProc);

            // TODO - Think about including additional proc data
            // tier = GetTier((SurveillanceProc)record.AdditionalSurvProc);
            // if (tier == Tier.Invalid)
            // {
            //     Logger.Error($"Invalid additional tier found: {record.Id}");
            //     continue;
            // }
            //
            // stats.IncreaseRetrieval(record.AdditionalSurvProc);
            
            // Add the unlocked from state if present
            if (record.Unlocked > 0)
            {
                if (!CollectedData.Sectors.ContainsKey(record.Unlocked))
                    CollectedData.Sectors[record.Unlocked] = new SubLoot.Sector(Sheets.ExplorationSheet.GetRow(record.Unlocked));

                CollectedData.Sectors[record.Unlocked].UnlockedFrom = record.Sector;
            }
        }
    }
    
    private void Export()
    {
        // The loop over fetch isn't guaranteed to contain all item IDs, so we add them here after everything has been processed
        foreach (var itemId in CollectedData.Sectors.Values.SelectMany(s => s.Pools.Values).SelectMany(s => s.Rewards.Keys))
            MappingHelper.AddItem(itemId);
        
        ExportHandler.WriteDataJson("Submarines.json", CollectedData);
        Logger.Information("Done exporting data ...");
    }
    
    private SurvTier GetSurvTier(uint proc) => (SurveillanceProc)proc switch
    {
        SurveillanceProc.T1Low or SurveillanceProc.T1Mid or SurveillanceProc.T1High => SurvTier.Tier1,
        SurveillanceProc.T2Mid or SurveillanceProc.T2High => SurvTier.Tier2,
        SurveillanceProc.T3High => SurvTier.Tier3,
        _ => SurvTier.Invalid
    };
    
    private SurvTier GetCapableTier(uint proc) => (SurveillanceProc)proc switch
    {
        SurveillanceProc.T1Low => SurvTier.Tier1,
        SurveillanceProc.T1Mid or SurveillanceProc.T2Mid => SurvTier.Tier2,
        SurveillanceProc.T1High or SurveillanceProc.T2High or SurveillanceProc.T3High => SurvTier.Tier3,
        _ => SurvTier.Invalid
    };

    private RetTier GetRetTier(uint proc) => (RetrievalProc)proc switch
    {
        RetrievalProc.Poor => RetTier.Poor,
        RetrievalProc.Normal => RetTier.Normal,
        RetrievalProc.Optimal => RetTier.Optimal,
        _ => RetTier.Invalid
    };
}