using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace SupabaseExporter;

public class DatabaseContext : DbContext
{
    public DbSet<Models.Loot> Loot { get; set; }
    public DbSet<Models.Gacha> Gacha { get; set; }
    public DbSet<Models.Bnuuy> Bunny { get; set; }
    public DbSet<Models.Venture> Ventures { get; set; }
    public DbSet<Models.Desynthesis> Desynthesis { get; set; }

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
        var dataHandler = new DataHandler();

        await using var context = new DatabaseContext();
        // await exporter.ExportSubmarineData(context);

        var gachaResult = await exporter.LoadGachaData(context);
        if (gachaResult.Success)
        {
            await dataHandler.ReadCofferData(gachaResult.Data);
            await dataHandler.ReadDeepDungeonData(gachaResult.Data);
            await dataHandler.ReadLockboxData(gachaResult.Data);
        }

        var ventureResult = await exporter.LoadVentureData(context);
        if (ventureResult.Success)
        {
            await dataHandler.PrintVentureStats(ventureResult.Data);
            await dataHandler.ReadVentureData(ventureResult.Data);
        }

        var bunnyResult = await exporter.LoadBunnyData(context);
        if (bunnyResult.Success)
        {
            await dataHandler.ReadBunnyData(bunnyResult.Data);
        }

        var desynthResult = await exporter.LoadDesynthData(context);
        if (desynthResult.Success)
        {
            await dataHandler.ReadDesynthData(desynthResult.Data);
        }

        // Generate json with all icon paths
        await IconHelper.CreateIconPaths();
        await dataHandler.WriteTimestamp();
    }
}

public class Exporter
{
    private readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
    private readonly CsvConfiguration CsvReaderConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

    public async Task ExportSubmarineData(DatabaseContext context)
    {
        Console.WriteLine("Exporting submarine data");
        var result = await context.Loot.Where(l => l.Version != "0").OrderBy(l => l.Id).ToListAsync();

        Console.WriteLine($"Rows found {result.Count:N0}");
        if (result.Count == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        var lastId = result.Last().Id.ToString();

        await WriteCsv(lastId, result);

        await context.Loot.Where(l => l.Id < result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");

        Console.WriteLine("Done exporting submarine data...");
    }

    public async Task<(bool Success, List<Models.Gacha> Data)> LoadGachaData(DatabaseContext context)
    {
        Console.WriteLine("Loading gacha data");
        var previous = ReadCsv<Models.Gacha>("LocalCache/Gacha");
        var result = await context.Gacha.ToListAsync();
        if (result.Count == 0)
        {
            Console.WriteLine("No new records found");
            return (false, []);
        }

        result = previous.Concat(result).ToList();

        Console.WriteLine($"Rows found {result.Count:N0}");
        Console.WriteLine("Loading gacha data finished...");
        return (true, result);
    }

    public async Task<(bool Success, List<Models.Venture> Data)> LoadVentureData(DatabaseContext context)
    {
        Console.WriteLine("Loading venture data");
        var previous = ReadCsv<Models.Venture>("LocalCache/Ventures");
        var result = await context.Ventures.OrderBy(l => l.Id).ToListAsync();
        if (result.Count == 0)
        {
            Console.WriteLine("No new records found");
            return (false, []);
        }
        
        result = previous.Concat(result).ToList();

        Console.WriteLine($"Ventures found {result.Count:N0}");
        Console.WriteLine("Loading venture data finished...");
        return (true, result);
    }

    public async Task<(bool Success, List<Models.Bnuuy> Data)> LoadBunnyData(DatabaseContext context)
    {
        Console.WriteLine("Loading bunny data");
        var previous = ReadCsv<Models.Bnuuy>("LocalCache/Bnuuy");
        var result = await context.Bunny.OrderBy(l => l.Id).ToListAsync();
        if (result.Count == 0)
        {
            Console.WriteLine("No new records found");
            return (false, []);
        }

        result = previous.Concat(result).ToList();

        Console.WriteLine($"Rows found {result.Count:N0}");
        Console.WriteLine("Loading gacha data finished...");
        return (true, result);
    }

    public async Task<(bool Success, List<Models.Desynthesis> Data)> LoadDesynthData(DatabaseContext context)
    {
        Console.WriteLine("Loading desynth data");
        var previous = ReadCsv<Models.Desynthesis>("LocalCache/Desynthesis");
        var result = await context.Desynthesis.ToListAsync();
        if (result.Count == 0)
        {
            Console.WriteLine("No new records found");
            return (false, []);
        }

        result = previous.Concat(result).ToList();

        Console.WriteLine($"Rows found {result.Count:N0}");
        Console.WriteLine("Loading desynth data finished...");
        return (true, result);
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

    private T[] ReadCsv<T>(string fileName)
    {
        using var reader = new StreamReader($"{fileName}.csv");
        using var csv = new CsvReader(reader, CsvReaderConfig);

        return csv.GetRecords<T>().ToArray();
    }
}