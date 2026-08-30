using Lumina.Extensions;
using Microsoft.EntityFrameworkCore;
using SupabaseExporter.Structures.Exports;

namespace SupabaseExporter.Processing.GuildleveAssignments;

public class GuildleveAssignmentsProcessor : IDisposable
{
    private readonly Dictionary<uint, LeveIssuer> ProcessedData = new();
    private readonly Dictionary<uint, (uint ENpcBaseId, uint LevelId)> ENpcCache = [];

    public async Task ProcessAllData(DatabaseContext context)
    {
        Logger.Information("Processing leve data");
        await Process(context);
        Export();
    }

    public void Dispose()
    {
        ProcessedData.Clear();
        ENpcCache.Clear();
        GC.Collect();
    }

    private async Task Process(DatabaseContext context)
    {
        LeveIssuer? currentIssuer = null;

        var stream = context.GuildleveAssignments
            .OrderBy(m => m.RowId)
            .ThenBy(m => m.CategoryRowId)
            .ThenBy(m => m.CategoryIndex)
            .AsAsyncEnumerable();

        await foreach (var row in stream)
        {
            if (currentIssuer == null || currentIssuer.GuildleveAssignmentId != row.RowId)
            {
                currentIssuer = new LeveIssuer { GuildleveAssignmentId = row.RowId };

                if (TryFindENpcByDataId(row.RowId, out var enpcBaseId, out var levelId))
                {
                    currentIssuer.ENpcBaseId = enpcBaseId;
                    currentIssuer.LevelId = levelId;
                }

                ProcessedData.Add(currentIssuer.GuildleveAssignmentId, currentIssuer);
            }

            if (!currentIssuer.Categories.TryGetValue(row.CategoryRowId, out var categoryEntry))
            {
                categoryEntry = new LeveAssignmentCategory { CategoryId = row.CategoryRowId };
                currentIssuer.Categories.Add(row.CategoryRowId, categoryEntry);
            }

            if (!categoryEntry.Types.TryGetValue(row.CategoryIndex, out var typeEntry))
            {
                typeEntry = new LeveAssignmentCategoryType { CategoryIndex = row.CategoryIndex };
                categoryEntry.Types.Add(row.CategoryIndex, typeEntry);
            }

            foreach (var leveId in row.LeveIds)
            {
                typeEntry.LeveIds.Add(leveId);
            }
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
