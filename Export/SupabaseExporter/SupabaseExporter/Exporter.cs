using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SupabaseExporter.Models;
using SupabaseExporter.Processing.ChestDrops;
using SupabaseExporter.Processing.Coffers;
using SupabaseExporter.Processing.Desynthesis;
using SupabaseExporter.Processing.Submarines;
using SupabaseExporter.Processing.Ventures;

namespace SupabaseExporter;

public class DatabaseContext : DbContext
{
    public DbSet<Models.SubmarineLootModel> SubmarineLoot { get; set; }
    public DbSet<Models.RandomCofferModel> RandomCoffers { get; set; }
    public DbSet<Models.EurekaBunnyModel> EurekaBunnies { get; set; }
    public DbSet<Models.VentureModel> Ventures { get; set; }
    public DbSet<Models.ChestDropModel> ChestDrops { get; set; }
    public DbSet<Models.DesynthesisModel> Desynthesis { get; set; }
    public DbSet<Models.OccultBunnyModel> OccultBunny { get; set; }
    public DbSet<Models.OccultTreasureModel> OccultTreasures { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString: $"Server={Environment.GetEnvironmentVariable("ip")};Port=5432;User Id={Environment.GetEnvironmentVariable("username")};Password={Environment.GetEnvironmentVariable("password")};Database=postgres;Command Timeout=0");
        base.OnConfiguring(optionsBuilder);
    }
}

public static class EntryPoint
{
    public static async Task Main()
    {
        var exporter = new Exporter();

        await using var context = new DatabaseContext();
        var submarineProcessor = new Submarines();
        await exporter.ExportSubmarineData(context, submarineProcessor);
        submarineProcessor.ProcessAllData();
        
        var gachaResult = exporter.LoadGachaData(context);
        if (gachaResult.Success)
        {
            var randomProcessor = new RandomCoffers();
            var deepDungeonProcessor = new DeepDungeonSacks();
            var lockboxProcessor = new FieldOpLockboxes();
            var cardProcessor = new TripleTriadPacks();
            var logoFragProcessor = new FieldOpContainers();
            
            randomProcessor.ProcessAllData(gachaResult.Data);
            deepDungeonProcessor.ProcessAllData(gachaResult.Data);
            lockboxProcessor.ProcessAllData(gachaResult.Data);
            cardProcessor.ProcessAllData(gachaResult.Data);
            logoFragProcessor.ProcessAllData(gachaResult.Data);
        }
        
        var ventureResult = exporter.LoadVentureData(context);
        if (ventureResult.Success)
        {
            var ventureProcessor = new Ventures();
            ventureProcessor.ProcessAllData(ventureResult.Data);
        }
        
        var bunnyResult = exporter.LoadBunnyData(context);
        if (bunnyResult.Success)
        {
            var bunnyProcessor = new EurekaBunnies();
            bunnyProcessor.ProcessAllData(bunnyResult.Data);
        }
        
        var desynthResult = exporter.LoadDesynthData(context);
        if (desynthResult.Success)
        {
            var desynthesisProcessor = new Desynthesis();
            desynthesisProcessor.ProcessAllData(desynthResult.Data);
        }
        
        var dutyLootResult = exporter.LoadDutyLootData(context);
        if (dutyLootResult.Success)
        {
            var dutyLootProcessor = new ChestDrops();
            dutyLootProcessor.ProcessAllData(dutyLootResult.Data);
        }
        
        var occultTreasureResult = exporter.LoadOccultTreasureData(context);
        var occultBunnyResult = exporter.LoadOccultBunnyData(context);
        if (occultTreasureResult.Success && occultBunnyResult.Success)
        {
            var occultTreasureProcessor = new OccultTreasures();
            occultTreasureProcessor.ProcessAllData(occultTreasureResult.Data, occultBunnyResult.Data);
        }
        
        // Generate json with all icon paths
        MappingHelper.ExportMappingFile();
        ExportHandler.WriteTimestamp();
    }
}

public class Exporter
{
    private readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
    private readonly CsvConfiguration CsvReaderConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

    public async Task ExportSubmarineData(DatabaseContext context, Submarines processor)
    {
        // Logger.Information("Exporting submarine data");
        // var result = await context.SubmarineLoot.Where(l => l.Version != "0").OrderBy(l => l.Id).ToListAsync();
        //
        // Logger.Information($"Rows found {result.Count:N0}");
        // if (result.Count == 0)
        // {
        //     Logger.Warning("No records found");
        //     return;
        // }
        //
        // var lastId = result.Last().Id.ToString();
        //
        // await WriteCsv(lastId, result);
        //
        // await context.SubmarineLoot.Where(l => l.Id < result.Last().Id).ExecuteDeleteAsync();
        // await context.Database.ExecuteSqlAsync($"vacuum full;");


        var mapping = new SubmarineLootMap();
        foreach (var data in ReadCsvFolder<Models.SubmarineLootModel>("LocalCache/Submarine/", mapping))
            processor.Fetch(data);

        Logger.Information("Done exporting submarine data...");
    }

    public (bool Success, Models.RandomCofferModel[] Data) LoadGachaData(DatabaseContext context)
    {
        Logger.Information("Loading gacha data");
        var previous = ReadCsv<Models.RandomCofferModel>("LocalCache/Gacha");
        var result = context.RandomCoffers.OrderBy(l => l.Id).AsEnumerable();
        var data = previous.Concat(result).ToArray();

        Logger.Information($"Total records {data.Length:N0}");
        Logger.Information("Loading gacha data finished...");
        return (true, data);
    }

    public (bool Success, Models.VentureModel[] Data) LoadVentureData(DatabaseContext context)
    {
        Logger.Information("Loading venture data");
        var previous = ReadCsv<Models.VentureModel>("LocalCache/Ventures");
        var result = context.Ventures.OrderBy(l => l.Id).AsEnumerable();
        var data = previous.Concat(result).ToArray();

        Logger.Information($"Total records {data.Length:N0}");
        Logger.Information("Loading venture data finished...");
        return (true, data);
    }

    public (bool Success, Models.EurekaBunnyModel[] Data) LoadBunnyData(DatabaseContext context)
    {
        Logger.Information("Loading bunny data");
        var previous = ReadCsv<Models.EurekaBunnyModel>("LocalCache/Bnuuy");
        var result = context.EurekaBunnies.OrderBy(l => l.Id).AsEnumerable();
        var data = previous.Concat(result).ToArray();

        Logger.Information($"Total records {data.Length:N0}");
        Logger.Information("Loading bunny data finished...");
        return (true, data);
    }

    public (bool Success, Models.DesynthesisModel[] Data) LoadDesynthData(DatabaseContext context)
    {
        Logger.Information("Loading desynth data");
        var previous = ReadCsv<Models.DesynthesisModel>("LocalCache/Desynthesis");
        var result = context.Desynthesis.OrderBy(l => l.Id).AsEnumerable();
        var data = previous.Concat(result).ToArray();

        Logger.Information($"Total records {data.Length:N0}");
        Logger.Information("Loading desynth data finished...");
        return (true, data);
    }
    
    public (bool Success, Models.ChestDropModel[] Data) LoadDutyLootData(DatabaseContext context)
    {
        Logger.Information("Loading duty loot data");
        var previous = ReadCsv<Models.ChestDropModel>("LocalCache/DutyLoot");
        var result = context.ChestDrops.OrderBy(l => l.Id).AsEnumerable();
        var data = previous.Concat(result).ToArray();

        Logger.Information($"Total records {data.Length:N0}");
        Logger.Information("Loading duty loot data finished...");
        return (true, data);
    }
    
    public (bool Success, Models.OccultTreasureModel[] Data) LoadOccultTreasureData(DatabaseContext context)
    {
        Logger.Information("Loading occult treasure data");
        var previous = ReadCsv<Models.OccultTreasureModel>("LocalCache/OccultTreasure");
        var result = context.OccultTreasures.OrderBy(l => l.Id).AsEnumerable();
        var data = previous.Concat(result).ToArray();

        Logger.Information($"Total records {data.Length:N0}");
        Logger.Information("Loading occult treasure data finished...");
        return (true, data);
    }
    
    public (bool Success, Models.OccultBunnyModel[] Data) LoadOccultBunnyData(DatabaseContext context)
    {
        Logger.Information("Loading occult bunny data");
        var previous = ReadCsv<Models.OccultBunnyModel>("LocalCache/OccultBunny");
        var result = context.OccultBunny.OrderBy(l => l.Id).AsEnumerable();
        var data = previous.Concat(result).ToArray();

        Logger.Information($"Total records {data.Length:N0}");
        Logger.Information("Loading occult bunny data finished...");
        return (true, data);
    }

    private async Task WriteCsv<T>(string fileName, IEnumerable<T> result, ClassMap<T>? classMap = null)
    {
        await using var writer = new StreamWriter(Path.Combine($"{fileName}.csv"));
        await using var csv = new CsvWriter(writer, CsvConfig);

        csv.Context.UnregisterClassMap();
        if (classMap != null)
            csv.Context.RegisterClassMap(classMap);

        csv.WriteHeader<T>();
        await csv.NextRecordAsync();

        foreach (var entry in result)
        {
            csv.WriteRecord(entry);
            await csv.NextRecordAsync();
        }
    }

    private IEnumerable<T> ReadCsv<T>(string fileName)
    {
        using var reader = new StreamReader($"{fileName}.csv");
        using var csv = new CsvReader(reader, CsvReaderConfig);

        return csv.GetRecords<T>();
    }
    
    private IEnumerable<IEnumerable<T>> ReadCsvFolder<T>(string folder, ClassMap? map = null)
    {
        foreach (var file in new DirectoryInfo(folder).EnumerateFiles())
        {
            using var reader = file.OpenText();
            using var csv = new CsvReader(reader, CsvReaderConfig);
            
            if (map != null)
                csv.Context.RegisterClassMap(map);
            
            yield return csv.GetRecords<T>();
        }
    }
}