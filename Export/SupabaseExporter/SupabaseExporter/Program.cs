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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString: $"Server=aws-0-eu-west-2.pooler.supabase.com;Port=5432;User Id={Environment.GetEnvironmentVariable("username")};Password={Environment.GetEnvironmentVariable("password")};Database=postgres;Command Timeout=120");
        base.OnConfiguring(optionsBuilder);
    }
}

public class Program
{
    public static async Task Main()
    {
        var exporter = new Exporter();

        await using var context = new DatabaseContext();
        var success = await exporter.ExportSubmarineData(context);
        if (!success)
            return;

        var result = await exporter.LoadGachaData(context);
        if (!result.Success)
            return;

        await exporter.ExportEurekaData(result.Data);
        await exporter.ExportBozjaData(result.Data);
        await exporter.ExportCofferData(result.Data);
        await exporter.ExportDeepDungeonData(result.Data);
        await exporter.ExportLogogramData(result.Data);
        await exporter.ExportFragmentData(result.Data);

        await exporter.ExportBunnyData(context);
    }
}

public class Exporter
{
    private readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
    private readonly CsvConfiguration CsvReaderConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

    private DirectoryInfo? Directory;

    public async Task<bool> ExportSubmarineData(DatabaseContext context)
    {
        Console.WriteLine("Exporting submarine data");
        var result = await context.Loot.Where(l => l.Version != "0").OrderBy(l => l.Id).ToListAsync();

        Console.WriteLine($"Rows found {result.Count}");
        if (result.Count == 0)
        {
            Console.WriteLine("No records found");
            return false;
        }

        var lastId = result.Last().Id.ToString();

        Directory = new DirectoryInfo(lastId);
        Directory.Create();

        await WriteCsv(lastId, result);

        await context.Loot.Where(l => l.Id < result.Last().Id).ExecuteDeleteAsync();
        await context.Database.ExecuteSqlAsync($"vacuum full;");

        Console.WriteLine("Done exporting submarine data...");
        return true;
    }

    public async Task<(bool Success, List<Models.Gacha> Data)> LoadGachaData(DatabaseContext context)
    {
        Console.WriteLine("Loading gacha data");
        var previous = ReadCsv<Models.Gacha>("Gacha");
        var result = await context.Gacha.ToListAsync();

        result = previous.Concat(result).ToList();

        Console.WriteLine($"Rows found {result.Count}");
        if (result.Count == previous.Length)
        {
            Console.WriteLine("No new records found");
            return (false, []);
        }

        Console.WriteLine("Loading gacha data finished...");
        return (true, result);
    }

    public async Task ExportEurekaData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting eureka data");
        uint[] validCoffers = [22508, 23142, 23379, 24141, 24142, 24848, 24849];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        await WriteCsv("Eureka", result);
        Console.WriteLine("Done exporting eureka data...");
    }

    public async Task ExportBozjaData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting Bozja data");
        uint[] validCoffers = [31357, 33797];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        await WriteCsv("Bozja", result);
        Console.WriteLine("Done exporting Bozja data...");
    }

    public async Task ExportCofferData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting coffer data");
        uint[] validCoffers = [32161, 36635, 36636, 41667];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        await WriteCsv("Coffer", result);
        Console.WriteLine("Done exporting coffer data...");
    }

    public async Task ExportDeepDungeonData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting deep dungeon data");
        uint[] validCoffers = [16170, 16171, 16172, 16173, 23223, 23224, 23225, 38945, 38946, 38947];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        await WriteCsv("DeepDungeon", result);
        Console.WriteLine("Done exporting deep dungeon data...");
    }

    public async Task ExportLogogramData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting logogram data");
        uint[] validCoffers = [24007, 24008, 24009, 24010, 24011, 24012, 24013, 24014, 24809];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        await WriteCsv("Logogram", result);
        Console.WriteLine("Done exporting logogram data...");
    }

    public async Task ExportFragmentData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting fragment data");
        uint[] validCoffers = [30884, 30885, 30886, 30887, 30888, 30889, 30890, 30891, 30892, 30893, 30894, 30895, 30896, 30897, 30898, 30899, 32162, 32163, 32164, 32165, 32831, 32832, 32833, 32834, 33768, 33769, 33770, 33771, 33772, 33773, 33774, 33775, 33776, 33777, 33778, 33779];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        await WriteCsv("Fragment", result);
        Console.WriteLine("Done exporting fragment data...");
    }

    public async Task ExportBunnyData(DatabaseContext context)
    {
        Console.WriteLine("Exporting bunny data");
        var result = await context.Bunny.OrderBy(l => l.Id).ToListAsync();

        Console.WriteLine($"Rows found {result.Count}");
        if (result.Count == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        await WriteCsv("Bunny", result, new Models.BnuuyMap());
        Console.WriteLine("Done exporting bunny data...");
    }

    private async Task WriteCsv<T>(string fileName, IEnumerable<T> result, ClassMap<T>? classMap = null)
    {
        await using var writer = new StreamWriter(Path.Combine(Directory!.FullName, $"{fileName}.csv"));
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