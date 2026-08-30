using Lumina.Extensions;
using SupabaseExporter.Models;
using SupabaseExporter.Structures.Exports;

namespace SupabaseExporter.Processing.GuildleveAssignments;

public class GuildleveAssignmentsProcessor : IDisposable
{
    private readonly Dictionary<uint, LeveIssuer> ProcessedData = new();
    private readonly Dictionary<uint, (uint ENpcBaseId, uint LevelId)> ENpcCache = [];

    public void ProcessAllData(GuildleveAssignmentsModel[] data)
    {
        Logger.Information("Processing leve data");
        Process(data);
        Export();
    }

    public void Dispose()
    {
        ProcessedData.Clear();
        ENpcCache.Clear();
        GC.Collect();
    }

    private void Process(GuildleveAssignmentsModel[] data)
    {
        foreach (var assignmentGrouping in data.GroupBy(m => m.RowId))
        {
            var result = new LeveIssuer { GuildleveAssignmentId = assignmentGrouping.Key, };

            if (TryFindENpcByDataId(assignmentGrouping.Key, out var enpcBaseId, out var levelId))
            {
                result.ENpcBaseId = enpcBaseId;
                result.LevelId = levelId;
            }

            foreach (var categoryGrouping in assignmentGrouping.GroupBy(m => m.CategoryRowId))
            {
                if (!result.Categories.TryGetValue(categoryGrouping.Key, out var categoryEntry))
                {
                    categoryEntry = new LeveAssignmentCategory { CategoryId = categoryGrouping.Key, };
                    
                    result.Categories.Add(categoryGrouping.Key, categoryEntry);
                }

                foreach (var indexGrouping in categoryGrouping.GroupBy(m => m.CategoryIndex))
                {
                    if (!categoryEntry.Types.TryGetValue(indexGrouping.Key, out var typeEntry))
                    {
                        typeEntry = new LeveAssignmentCategoryType { CategoryIndex = indexGrouping.Key, };

                        categoryEntry.Types.Add(indexGrouping.Key, typeEntry);
                    }

                    foreach (var leveId in indexGrouping.SelectMany(model => model.LeveIds))
                        typeEntry.LeveIds.Add(leveId);
                }
            }

            ProcessedData.Add(result.GuildleveAssignmentId, result);
        }
    }

    private void Export()
    {
        ExportHandler.WriteDataJson("LeveIssuers.json", ProcessedData);
        Logger.Information("Done exporting data ...");
    }

    private bool TryFindENpcByDataId(uint dataId, out uint enpcId, out uint levelId)
    {
        enpcId = 0;
        levelId = 0;

        if (ENpcCache.TryGetValue(dataId, out var tuple))
        {
            (enpcId, levelId) = tuple;
            return true;
        }

        if (Sheets.ENpcBaseSheet.TryGetFirst(row => row.ENpcData.Any(rowRef => rowRef.RowId == dataId), out var enpcBaseRow) &&
            Sheets.LevelSheet.TryGetFirst(row => row.Type == 8 && row.Object.RowId == enpcBaseRow.RowId, out var levelRow))
        {
            ENpcCache.Add(dataId, (enpcId, levelId) = (enpcBaseRow.RowId, levelRow.RowId));
            return true;
        }

        return false;
    }
}
