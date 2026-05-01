using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SupabaseExporter.Models;
using SupabaseExporter.Processing.BnpcPairs;
using SupabaseExporter.Processing.ChestDrops;
using SupabaseExporter.Processing.Coffers;
using SupabaseExporter.Processing.Desynthesis;
using SupabaseExporter.Processing.Submarines;
using SupabaseExporter.Processing.Ventures;
using SupabaseExporter.Structures.Sheets;

namespace SupabaseExporter;

public class DatabaseContext : DbContext
{
    public DbSet<SubmarineLootModel> SubmarineLoot { get; set; }
    public DbSet<RandomCofferModel> RandomCoffers { get; set; }
    public DbSet<EurekaBunnyModel> EurekaBunnies { get; set; }
    public DbSet<VentureModel> Ventures { get; set; }
    public DbSet<ChestDropModel> ChestDrops { get; set; }
    public DbSet<DesynthesisModel> Desynthesis { get; set; }
    public DbSet<OccultBunnyModel> OccultBunny { get; set; }
    public DbSet<OccultTreasureModel> OccultTreasures { get; set; }
    public DbSet<BnpcPairModel> BnpcPairs { get; set; }

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
        
        // TODO Rewrite for enumerator usage
        var gachaResult = await exporter.LoadGachaData(context);
        if (gachaResult.Length > 0)
        {
            var randomProcessor = new RandomCoffers();
            var deepDungeonProcessor = new DeepDungeonSacks();
            var lockboxProcessor = new FieldOpLockboxes();
            var cardProcessor = new TripleTriadPacks();
            var logoFragProcessor = new FieldOpContainers();
            
            randomProcessor.ProcessAllData(gachaResult);
            deepDungeonProcessor.ProcessAllData(gachaResult);
            lockboxProcessor.ProcessAllData(gachaResult);
            cardProcessor.ProcessAllData(gachaResult);
            logoFragProcessor.ProcessAllData(gachaResult);
        }
        
        var ventureResult = await exporter.LoadVentureData(context);
        if (ventureResult.Length > 0)
        {
            var ventureProcessor = new Ventures();
            ventureProcessor.ProcessAllData(ventureResult);
        }
        
        var bunnyResult = await exporter.LoadBunnyData(context);
        if (bunnyResult.Length > 0)
        {
            var bunnyProcessor = new EurekaBunnies();
            bunnyProcessor.ProcessAllData(bunnyResult);
        }
        
        var desynthResult = await exporter.LoadDesynthData(context);
        if (desynthResult.Length > 0)
        {
            var desynthesisProcessor = new Desynthesis();
            desynthesisProcessor.ProcessAllData(desynthResult);
        }
        
        var dutyLootResult = await exporter.LoadDutyLootData(context);
        if (dutyLootResult.Length > 0)
        {
            var dutyLootProcessor = new ChestDrops();
            dutyLootProcessor.ProcessAllData(dutyLootResult);
        }
        
        var occultTreasureResult = await exporter.LoadOccultTreasureData(context);
        var occultBunnyResult = await exporter.LoadOccultBunnyData(context);
        if (occultTreasureResult.Length > 0 && occultBunnyResult.Length > 0)
        {
            var occultTreasureProcessor = new OccultTreasures();
            occultTreasureProcessor.ProcessAllData(occultTreasureResult, occultBunnyResult);
        }
        
        var bnpcPairsProcessor = new BnpcPairs();
        await exporter.LoadBnpcPairData(context, bnpcPairsProcessor);
        bnpcPairsProcessor.ProcessAllData();
        
        // Generate json with all icon paths
        MappingHelper.ExportMappingFile();
        ExportHandler.WriteTimestamp();
        new SimplifiedSheets().Export();
    }
}

public class Exporter
{
    private readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
    private readonly CsvConfiguration CsvReaderConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

    public async Task ExportSubmarineData(DatabaseContext context, Submarines processor)
    {
        Logger.Information("Exporting submarine data");
        var result = await context.SubmarineLoot.Where(l => l.Version != "0").OrderBy(l => l.Id).ToListAsync();
        
        Logger.Information($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Logger.Warning("No records found");
            return;
        }
        
        var lastId = result.Last().Id.ToString();
        
        var mapping = new SubmarineLootMap();
        await WriteCsv("LocalCache/Submarine/", lastId, result, mapping);
        
        await context.SubmarineLoot.Where(l => l.Id <= result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");
        result.Clear();
        context.ChangeTracker.Clear();
        
        foreach (var data in ReadFolder<SubmarineLootModel>("LocalCache/Submarine/", processor.CollectedData.ProcessedId, mapping))
            processor.Fetch(data);

        Logger.Information("Done exporting submarine data...");
    }

    public async Task<RandomCofferModel[]> LoadGachaData(DatabaseContext context)
    {
        Logger.Information("Exporting gacha data");
        var result = await context.RandomCoffers.Where(l => l.Version != "0").OrderBy(l => l.Id).ToListAsync();
        
        Logger.Information($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Logger.Warning("No records found");
            return [];
        }
        
        var lastId = result.Last().Id.ToString();
        
        var path = "LocalCache/Gacha/";
        var mapping = new RandomCofferMap();
        await WriteCsv(path, lastId, result, mapping);
        
        await context.RandomCoffers.Where(l => l.Id <= result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");
        result.Clear();
        context.ChangeTracker.Clear();

        var data = ReadFolderStatic<RandomCofferModel>(path, 0, mapping).ToArray();
        Logger.Information("Done exporting gacha data...");
        return data;
    }

    public async Task<VentureModel[]> LoadVentureData(DatabaseContext context)
    {
        Logger.Information("Exporting venture data");
        var result = await context.Ventures.OrderBy(l => l.Id).ToListAsync();
        
        Logger.Information($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Logger.Warning("No records found");
            return [];
        }
        
        var lastId = result.Last().Id.ToString();
        
        var path = "LocalCache/Ventures/";
        var mapping = new VentureMap();
        await WriteCsv(path, lastId, result, mapping);
        
        await context.Ventures.Where(l => l.Id <= result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");
        result.Clear();
        context.ChangeTracker.Clear();

        var data = ReadFolderStatic<VentureModel>(path, 0, mapping).ToArray();
        Logger.Information("Done exporting venture data...");
        return data;
    }

    public async Task<EurekaBunnyModel[]> LoadBunnyData(DatabaseContext context)
    {
        Logger.Information("Exporting bunny data");
        var result = await context.EurekaBunnies.OrderBy(l => l.Id).ToListAsync();
        
        Logger.Information($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Logger.Warning("No records found");
            return [];
        }
        
        var lastId = result.Last().Id.ToString();
        
        var path = "LocalCache/Bnuuy/";
        var mapping = new EurekaBunnyExportMap();
        await WriteCsv(path, lastId, result, mapping);
        
        await context.EurekaBunnies.Where(l => l.Id <= result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");
        result.Clear();
        context.ChangeTracker.Clear();

        var importMapping = new EurekaBunnyImportMap();
        var data = ReadFolderStatic<EurekaBunnyModel>(path, 0, importMapping).ToArray();
        Logger.Information("Done exporting bunny data...");
        return data;
    }

    public async Task<DesynthesisModel[]> LoadDesynthData(DatabaseContext context)
    {
        Logger.Information("Exporting desynth data");
        var result = await context.Desynthesis.OrderBy(l => l.Id).ToListAsync();
        
        Logger.Information($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Logger.Warning("No records found");
            return [];
        }
        
        var lastId = result.Last().Id.ToString();
        
        var path = "LocalCache/Desynthesis/";
        var mapping = new DesynthesisExportMap();
        await WriteCsv(path, lastId, result, mapping);
        
        await context.Desynthesis.Where(l => l.Id <= result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");
        result.Clear();
        context.ChangeTracker.Clear();

        var importMapping = new DesynthesisImportMap();
        var data = ReadFolderStatic<DesynthesisModel>(path, 0, importMapping).ToArray();
        Logger.Information("Done exporting desynth data...");
        return data;
    }
    
    public async Task<ChestDropModel[]> LoadDutyLootData(DatabaseContext context)
    {
        Logger.Information("Exporting duty loot data");
        var result = await context.ChestDrops.OrderBy(l => l.Id).ToListAsync();
        
        Logger.Information($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Logger.Warning("No records found");
            return [];
        }
        
        var lastId = result.Last().Id.ToString();
        
        var path = "LocalCache/DutyLoot/";
        var mapping = new ChestDropExportMap();
        await WriteCsv(path, lastId, result, mapping);
        
        await context.ChestDrops.Where(l => l.Id <= result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");
        result.Clear();
        context.ChangeTracker.Clear();

        var importMapping = new ChestDropImportMap();
        var data = ReadFolderStatic<ChestDropModel>(path, 0, importMapping).ToArray();
        Logger.Information("Done exporting duty loot data...");
        return data;
    }
    
    public async Task<OccultTreasureModel[]> LoadOccultTreasureData(DatabaseContext context)
    {
        Logger.Information("Exporting occult treasure data");
        var result = await context.OccultTreasures.OrderBy(l => l.Id).ToListAsync();
        
        Logger.Information($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Logger.Warning("No records found");
            return [];
        }
        
        var lastId = result.Last().Id.ToString();
        
        var path = "LocalCache/OccultTreasure/";
        var mapping = new OccultTreasureExportMap();
        await WriteCsv(path, lastId, result, mapping);
        
        await context.OccultTreasures.Where(l => l.Id <= result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");
        result.Clear();
        context.ChangeTracker.Clear();

        var importMapping = new OccultTreasureImportMap();
        var data = ReadFolderStatic<OccultTreasureModel>(path, 0, importMapping).ToArray();
        Logger.Information("Done exporting occult treasure data...");
        return data;
    }
    
    public async Task<OccultBunnyModel[]> LoadOccultBunnyData(DatabaseContext context)
    {
        Logger.Information("Exporting occult bunny data");
        var result = await context.OccultBunny.OrderBy(l => l.Id).ToListAsync();
        
        Logger.Information($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Logger.Warning("No records found");
            return [];
        }
        
        var lastId = result.Last().Id.ToString();
        
        var path = "LocalCache/OccultBunny/";
        var mapping = new OccultBunnyExportMap();
        await WriteCsv(path, lastId, result, mapping);
        
        await context.OccultBunny.Where(l => l.Id <= result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");
        result.Clear();
        context.ChangeTracker.Clear();

        var importMapping = new OccultBunnyImportMap();
        var data = ReadFolderStatic<OccultBunnyModel>(path, 0, importMapping).ToArray();
        Logger.Information("Done exporting occult bunny data...");
        return data;
    }
    
    public async Task LoadBnpcPairData(DatabaseContext context, BnpcPairs processor)
    {
        Logger.Information("Exporting BnpcPair data");
        var result = await context.BnpcPairs.Where(l => l.Version != "0").OrderBy(l => l.Id).ToListAsync();
        
        Logger.Information($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Logger.Warning("No records found");
            return;
        }
        
        var lastId = result.Last().Id.ToString();
        
        var mapping = new BnpcPairMap();
        await WriteCsv("LocalCache/Bnpc/", lastId, result, mapping);
        
        await context.BnpcPairs.Where(l => l.Id <= result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");
        result.Clear();
        context.ChangeTracker.Clear();
        
        foreach (var data in ReadFolder<BnpcPairModel>("LocalCache/Bnpc/", processor.CollectedData.ProcessedId, mapping))
            processor.Fetch(data);

        Logger.Information("Done exporting BnpcPair data...");
    }

    private async Task WriteCsv<T>(string path, string fileName, IEnumerable<T> result, ClassMap<T>? classMap = null)
    {
        await using var writer = new StreamWriter(Path.Combine($"{path}{fileName}.csv"));
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

    private T[] ReadCsv<T>(string fileName)
    {
        using var reader = new StreamReader($"{fileName}.csv");
        using var csv = new CsvReader(reader, CsvReaderConfig);

        return csv.GetRecords<T>().ToArray();
    }
    
    private IEnumerable<IEnumerable<T>> ReadFolder<T>(string folder, uint lastId, ClassMap? map = null)
    {
        foreach (var pair in new DirectoryInfo(folder).EnumerateFiles().Select(f => (f, uint.Parse(Path.GetFileNameWithoutExtension(f.Name)))).Where(pair => lastId < pair.Item2).OrderBy(pair => pair.Item2))
        {
            using var reader = pair.f.OpenText();
            using var csv = new CsvReader(reader, CsvReaderConfig);
            
            if (map != null)
                csv.Context.RegisterClassMap(map);
            
            yield return csv.GetRecords<T>();
        }
    }    
    
    private List<T> ReadFolderStatic<T>(string folder, uint lastId, ClassMap? map = null)
    {
        var results = new List<T>();
        foreach (var pair in new DirectoryInfo(folder).EnumerateFiles().Select(f => (f, uint.Parse(Path.GetFileNameWithoutExtension(f.Name)))).Where(pair => lastId < pair.Item2).OrderBy(pair => pair.Item2))
        {
            using var reader = pair.f.OpenText();
            using var csv = new CsvReader(reader, CsvReaderConfig);
            
            if (map != null)
                csv.Context.RegisterClassMap(map);
            
            results.AddRange(csv.GetRecords<T>());
        }

        return results;
    }
}