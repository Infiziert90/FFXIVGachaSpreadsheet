using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Lumina.Excel.Sheets;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

using static SupabaseExporter.Utils;

namespace SupabaseExporter;

public class DatabaseContext : DbContext
{
    public DbSet<Models.Loot> Loot { get; set; }
    public DbSet<Models.Gacha> Gacha { get; set; }
    public DbSet<Models.Bnuuy> Bunny { get; set; }
    public DbSet<Models.Venture> Ventures { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString: $"Server=aws-0-eu-west-2.pooler.supabase.com;Port=5432;User Id={Environment.GetEnvironmentVariable("username")};Password={Environment.GetEnvironmentVariable("password")};Database=postgres;Command Timeout=0");
        base.OnConfiguring(optionsBuilder);
    }
}

public static class EntryPoint
{
    // public static async Task Main()
    // {
    //     var exporter = new Exporter();
    //
    //     await using var context = new DatabaseContext();
    //     var success = await exporter.ExportSubmarineData(context);
    //     if (!success)
    //         return;
    //
    //     var result = await exporter.LoadGachaData(context);
    //     if (!result.Success)
    //         return;
    //
    //     await exporter.ExportEurekaData(result.Data);
    //     await exporter.ExportBozjaData(result.Data);
    //     await exporter.ExportCofferData(result.Data);
    //     await exporter.ExportDeepDungeonData(result.Data);
    //     await exporter.ExportFragmentData(result.Data);
    //
    //     await exporter.ExportBunnyData(context);
    //
    //     exporter.SheetHandler.SetTime();
    // }

    public enum VentureTypes : uint
    {
        QuickVenture = 395,
        QuickVentureMaxLevel = 100_000,
        QuickVentureMinLevel = 200_000
    }

    public static async Task Main()
    {
        var exporter = new Exporter();

        await using var context = new DatabaseContext();
        Console.WriteLine("Exporting venture data");
        var result = await context.Ventures.Where(l => l.Version != "1.5.6.0").OrderBy(l => l.Id).ToListAsync();

        Console.WriteLine($"Ventures found {result.Count:N0}");
        if (result.Count == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        var quickHistory = result.Where(v => v.QuickVenture).ToArray();

        // Coffers only drop from max level retainers
        var cofferVentures = quickHistory.Count(v => v.MaxLevel);
        var totalCoffers = quickHistory.Count(v => v.PrimaryId == 32161);

        // All valid gear is rarity green or higher
        (Item Item, bool HQ)[] validGear = quickHistory.Select(v => (Sheets.ItemSheet.GetRow(v.PrimaryId), v.PrimaryHq)).Where(i => i.Item1.Rarity > 1).ToArray();
        var totalLvl = validGear.Sum(i => i.Item.LevelItem.RowId);
        var totalSeals = validGear.Sum(i => Sheets.GCSupplySheet.GetRow(i.Item.LevelItem.RowId).SealsExpertDelivery);
        var totalFCPoints = validGear.Sum(i =>
        {
            var iLvL = i.Item.LevelItem.RowId;
            if ((iLvL & 1) == 1)
                iLvL += 1;

            return (i.HQ ? 3.0 : 1.5) * iLvL;
        });

        Console.WriteLine($"Total quick ventures: {quickHistory.Length:N0}");
        Console.WriteLine("\n");

        Console.WriteLine($"= Gear Average =");
        Console.WriteLine($"iLvL: {totalLvl / quickHistory.Length:F2} | {totalLvl / validGear.Length:F2}");
        Console.WriteLine($"FC Points: {totalFCPoints / quickHistory.Length:F2} | {totalFCPoints / validGear.Length:F2}");
        Console.WriteLine($"GC Seals: {totalSeals / quickHistory.Length:F2} | {totalSeals / validGear.Length:F2}");

        var history = quickHistory.Where(v => v.MaxLevel).ToArray();
        validGear = history.Where(v => v.MaxLevel).Select(v => (Sheets.ItemSheet.GetRow(v.PrimaryId), v.PrimaryHq)).Where(i => i.Item1.Rarity > 1).ToArray();
        totalLvl = validGear.Sum(i => i.Item.LevelItem.RowId);
        totalSeals = validGear.Sum(i => Sheets.GCSupplySheet.GetRow(i.Item.LevelItem.RowId).SealsExpertDelivery);
        totalFCPoints = validGear.Sum(i =>
        {
            var iLvL = i.Item.LevelItem.RowId;
            if ((iLvL & 1) == 1)
                iLvL += 1;

            return (i.HQ ? 3.0 : 1.5) * iLvL;
        });
        Console.WriteLine("\n");
        Console.WriteLine($"= Gear Average (Retainer Level = 100) [{history.Length:N0}] =");
        Console.WriteLine($"iLvL: {totalLvl / history.Length:F2}");
        Console.WriteLine($"FC Points: {totalFCPoints / history.Length:F2}");
        Console.WriteLine($"GC Seals: {totalSeals / history.Length:F2}");

        history = quickHistory.Where(v => !v.MaxLevel).ToArray();
        validGear = history.Select(v => (Sheets.ItemSheet.GetRow(v.PrimaryId), v.PrimaryHq)).Where(i => i.Item1.Rarity > 1).ToArray();
        totalLvl = validGear.Sum(i => i.Item.LevelItem.RowId);
        totalSeals = validGear.Sum(i => Sheets.GCSupplySheet.GetRow(i.Item.LevelItem.RowId).SealsExpertDelivery);
        totalFCPoints = validGear.Sum(i =>
        {
            var iLvL = i.Item.LevelItem.RowId;
            if ((iLvL & 1) == 1)
                iLvL += 1;

            return (i.HQ ? 3.0 : 1.5) * iLvL;
        });
        Console.WriteLine("\n");
        Console.WriteLine($"= Gear Average (Retainer Level < 100) [{history.Length:N0}] =");
        Console.WriteLine($"iLvL: {totalLvl / history.Length:F2}");
        Console.WriteLine($"FC Points: {totalFCPoints / history.Length:F2}");
        Console.WriteLine($"GC Seals: {totalSeals / history.Length:F2}");

        Console.WriteLine("\n");
        Console.WriteLine("= Venture Coffers =");
        Console.WriteLine($"Coffer valid ventures: {cofferVentures:N0}");
        Console.WriteLine($"Coffers: {totalCoffers:N0} (Chance in 100 = {totalCoffers / (double)cofferVentures * 100:F2}%)");

        var ventures = new Dictionary<VentureTypes, VentureTemp>();
        foreach (var venture in result)
        {
            foreach (var ventureType in Enum.GetValues<VentureTypes>())
            {
                var type = ventureType is VentureTypes.QuickVenture ? (VentureTypes)venture.VentureType : ventureType;
                ventures.TryAdd(type, new VentureTemp());

                switch (ventureType)
                {
                    case VentureTypes.QuickVentureMaxLevel when !venture.MaxLevel:
                    case VentureTypes.QuickVentureMinLevel when venture.MaxLevel:
                        continue;
                }

                var tmp = ventures[type];

                tmp.Type = venture.VentureType;
                tmp.Total += 1;
                tmp.Items.TryAdd(venture.PrimaryId, (new VentureTemp.ItemResult(), new VentureTemp.ItemResult()));

                var (primary, additional) =  tmp.Items[venture.PrimaryId];
                primary.Amount += 1;
                primary.Total += venture.PrimaryCount;
                primary.Min = Math.Min(primary.Min, venture.PrimaryCount);
                primary.Max = Math.Max(primary.Max, venture.PrimaryCount);
                tmp.Items[venture.PrimaryId] = (primary, additional);

                tmp.Items.TryAdd(venture.AdditionalId, (new VentureTemp.ItemResult(), new VentureTemp.ItemResult()));
                (primary, additional) = tmp.Items[venture.AdditionalId];
                additional.Amount += 1;
                additional.Total += venture.AdditionalCount;
                additional.Min = Math.Min(additional.Min, venture.AdditionalCount);
                additional.Max = Math.Max(additional.Max, venture.AdditionalCount);
                tmp.Items[venture.AdditionalId] = (primary, additional);

                ventures[type] = tmp;

                // Skip the foreach if we aren't in a Quick Venture
                if (venture.VentureType != 395)
                    break;
            }
        }

        var dict = new Dictionary<uint, VentureData>();
        foreach (var (key, venture) in ventures)
        {
            var task = Sheets.RetainerTaskSheet.GetRow(venture.Type);
            if (!task.IsRandom)
                continue;

            if (!task.Task.TryGetValue<RetainerTaskRandom>(out var retainerTask))
                continue;

            var primaryList = new List<ResultItem>();
            var additionalList = new List<ResultItem>();
            foreach (var (itemId, (primary, additional)) in venture.Items)
            {
                if (itemId == 0)
                    continue;

                var item = Sheets.ItemSheet.GetRow(itemId);
                if (primary.Amount > 0)
                {
                    primaryList.Add(new ResultItem(
                        item.Name.ExtractText(),
                        item.Icon,
                        (uint)primary.Amount,
                        primary.Amount / (double)venture.Total,
                        (uint)primary.Total,
                        (uint)primary.Min,
                        (uint)primary.Max));
                }

                if (additional.Amount > 0)
                {
                    additionalList.Add(new ResultItem(
                        item.Name.ExtractText(),
                        item.Icon,
                        (uint)additional.Amount,
                        additional.Amount / (double)venture.Total,
                        (uint)additional.Total,
                        (uint)additional.Min,
                        (uint)additional.Max));
                }

                await IconHelper.CreateIcon(item);
            }

            primaryList = primaryList.OrderBy(l => l.Amount).ToList();
            additionalList = additionalList.OrderBy(l => l.Amount).ToList();

            dict.TryAdd(task.ClassJobCategory.RowId, new VentureData(task));

            var type = venture.Type;
            var taskName = retainerTask.Name.ExtractText();
            if (key is VentureTypes.QuickVentureMaxLevel)
            {
                type = (uint)key;
                taskName += " (Only Max Level)";
            }
            else if (key is VentureTypes.QuickVentureMinLevel)
            {
                type = (uint)key;
                taskName += " (< Level 100)";
            }

            dict[task.ClassJobCategory.RowId].Tasks.Add(new VentureData.VentureTask(taskName, type, (uint)venture.Total, primaryList, additionalList));
        }

        var ventureList = new List<VentureData>();
        foreach (var ventureData in dict.Values)
        {
            ventureData.Tasks = ventureData.Tasks.OrderBy(t => t.TaskType).ToList();
            ventureList.Add(ventureData);
        }

        ventureList = ventureList.OrderBy(l => l.Category).ToList();

        await File.WriteAllTextAsync("VentureData.json", JsonConvert.SerializeObject(ventureList));

        await ReadGachaShit(exporter, context);
        await ReadBunnyData(context);
    }

    private static async Task ReadBunnyData(DatabaseContext context)
    {
        Console.WriteLine("Exporting bunny data");
        var result = await context.Bunny.OrderBy(l => l.Id).ToArrayAsync();

        Console.WriteLine($"Rows found {result.Length}");
        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        var bunnies = new Dictionary<Territory, CofferTemp>();
        foreach (var bunny in result)
        {
            bunnies.TryAdd((Territory)bunny.Territory, new CofferTemp());

            var tmp = bunnies[(Territory)bunny.Territory];

            tmp.Total += 1;
            tmp.Coffers.TryAdd(bunny.Coffer, new CofferTemp.Coffer());

            var cofferRarity =  tmp.Coffers[bunny.Coffer];

            cofferRarity.Total += 1;
            foreach (var item in bunny.Items)
                if (!cofferRarity.Items.TryAdd(item, 1))
                    cofferRarity.Items[item] += 1;

            tmp.Coffers[bunny.Coffer] = cofferRarity;
            bunnies[(Territory)bunny.Territory] = tmp;
        }

        var bunnyList = new List<MultiCofferData>();
        foreach (var (territory, bunnyTemp) in bunnies)
        {
            var coffers = new List<MultiCofferData.Coffer>();
            foreach (var (coffer, cofferTemp) in bunnyTemp.Coffers)
            {
                var primaryList = new List<ResultItem>();

                foreach (var (itemId, amount) in cofferTemp.Items)
                {
                    var item = Sheets.ItemSheet.GetRow(itemId);
                    primaryList.Add(new ResultItem(item.Name.ExtractText(), item.Icon, (uint)amount, amount / cofferTemp.Total));

                    await IconHelper.CreateIcon(item);
                }

                primaryList = primaryList.OrderBy(l => l.Amount).ToList();
                coffers.Add(new MultiCofferData.Coffer(coffer, ((CofferRarity)coffer).ToName(), cofferTemp.Total, primaryList));
            }

            coffers = coffers.OrderByDescending(c => c.CofferId).ToList();
            bunnyList.Add(new MultiCofferData((uint)territory, territory.ToName(), bunnyTemp.Total, coffers));
        }

        bunnyList = bunnyList.OrderBy(l => l.Territory).ToList();

        await File.WriteAllTextAsync("BunnyData.json", JsonConvert.SerializeObject(bunnyList));
        Console.WriteLine("Done exporting bunny data...");
    }

    public struct VentureTemp
    {
        public uint Type = 0;
        public long Total = 0;
        public Dictionary<uint, (ItemResult Primary, ItemResult Additional)> Items = new();

        public VentureTemp() { }

        public struct ItemResult
        {
            public long Amount = 0;
            public long Total = 0;
            public long Min = long.MaxValue;
            public long Max = long.MinValue;

            public ItemResult() { }
        };
    }

    public struct CofferTemp
    {
        public long Total = 0;
        public Dictionary<uint, Coffer> Coffers = new();

        public CofferTemp() { }

        public struct Coffer
        {
            public long Total = 0;
            public Dictionary<uint, double> Items = new();

            public Coffer() { }
        }
    }

    public record VentureData
    {
        public uint Category;
        public string Name;
        public List<VentureTask> Tasks = [];

        [JsonIgnore]
        public RetainerTask Task;

        public VentureData(RetainerTask task)
        {
            Task = task;

            Category = Task.ClassJobCategory.RowId;

            Name = Category switch
            {
                0 => "All",
                34 => "DoW/DoM",
                _ => Task.ClassJobCategory.Value.Name.ExtractText()
            };
        }

        public record VentureTask
        {
            public string TaskName;
            public uint TaskType;
            public uint TaskTotal;
            public List<ResultItem> Items;
            public List<ResultItem> Additionals;

            public VentureTask(string name, uint type, uint total, List<ResultItem> items, List<ResultItem> additionals)
            {
                TaskName = name;
                TaskType = type;
                TaskTotal = total;
                Items = items;
                Additionals = additionals;
            }
        }
    };

    public record MultiCofferData
    {
        public string Name;
        public uint Territory;
        public long Total;
        public List<Coffer> Coffers = [];

        public MultiCofferData(uint territory, string name, long total, List<Coffer> coffers)
        {
            Territory = territory;
            Name = name;
            Total = total;
            Coffers = coffers;
        }

        public record Coffer
        {
            public uint CofferId;
            public string CofferName;
            public long CofferTotal;
            public List<ResultItem> Items;

            public Coffer(uint cofferId, string cofferName, long cofferTotal, List<ResultItem> items)
            {
                CofferId = cofferId;
                CofferName = cofferName;
                CofferTotal = cofferTotal;
                Items = items;
            }
        };
    };

    private static async Task ReadGachaShit(Exporter exporter, DatabaseContext context)
    {
        Console.WriteLine("Loading gacha data");
        var previous = exporter.ReadCsv<Models.Gacha>("Gacha");
        var gachaResult = await context.Gacha.ToListAsync();

        gachaResult = previous.Concat(gachaResult).ToList();

        Console.WriteLine($"Rows found {gachaResult.Count}");
        if (gachaResult.Count == previous.Length)
        {
            Console.WriteLine("No new records found");
            return;
        }

        Console.WriteLine("Loading gacha data finished...");

        await ReadCofferData(gachaResult);
        await ReadDeepDungeonData(gachaResult);
        await ReadLockboxData(gachaResult);
    }

    private static async Task ReadCofferData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting coffer data");
        uint[] validCoffers = [32161, 36635, 36636, 41667];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        var tmpData = new Dictionary<Coffer, CofferTemp>();
        foreach (var coffer in result)
        {
            var type = Coffer.Any;
            tmpData.TryAdd(type, new CofferTemp());

            var tmp = tmpData[type];

            tmp.Total += 1;
            tmp.Coffers.TryAdd(coffer.Coffer, new CofferTemp.Coffer());

            var cofferRarity =  tmp.Coffers[coffer.Coffer];

            cofferRarity.Total += 1;
            if (!cofferRarity.Items.TryAdd(coffer.ItemId, 1))
                cofferRarity.Items[coffer.ItemId] += 1;

            tmp.Coffers[coffer.Coffer] = cofferRarity;
            tmpData[type] = tmp;
        }

        var cofferList = new List<MultiCofferData>();
        foreach (var (territory, deepDungeonTemp) in tmpData)
        {
            var coffers = new List<MultiCofferData.Coffer>();
            foreach (var (coffer, cofferTemp) in deepDungeonTemp.Coffers)
            {
                var primaryList = new List<ResultItem>();

                foreach (var (itemId, amount) in cofferTemp.Items)
                {
                    var item = Sheets.ItemSheet.GetRow(itemId);
                    primaryList.Add(new ResultItem(item.Name.ExtractText(), item.Icon, (uint)amount, amount / cofferTemp.Total));

                    await IconHelper.CreateIcon(item);
                }

                var cofferItem = Sheets.ItemSheet.GetRow(coffer);

                primaryList = primaryList.OrderBy(l => l.Amount).ToList();
                coffers.Add(new MultiCofferData.Coffer(coffer, cofferItem.Name.ExtractText(), cofferTemp.Total, primaryList));
            }

            coffers = coffers.OrderBy(c => c.CofferId).ToList();
            cofferList.Add(new MultiCofferData((uint)territory, territory.ToName(), deepDungeonTemp.Total, coffers));
        }

        cofferList = cofferList.OrderBy(l => l.Territory).ToList();

        await File.WriteAllTextAsync("CofferData.json", JsonConvert.SerializeObject(cofferList));
        Console.WriteLine("Done exporting coffer data...");
    }

    private static async Task ReadDeepDungeonData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting deep dungeon data");
        uint[] validCoffers = [16170, 16171, 16172, 16173, 23223, 23224, 23225, 38945, 38946, 38947];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        var deepDungeons = new Dictionary<Utils.DeepDungeon, CofferTemp>();
        foreach (var coffer in result)
        {
            var type = ToDeepDungeon(coffer.Coffer);
            deepDungeons.TryAdd(type, new CofferTemp());

            var tmp = deepDungeons[type];

            tmp.Total += 1;
            tmp.Coffers.TryAdd(coffer.Coffer, new CofferTemp.Coffer());

            var cofferRarity =  tmp.Coffers[coffer.Coffer];

            cofferRarity.Total += 1;
            if (!cofferRarity.Items.TryAdd(coffer.ItemId, 1))
                cofferRarity.Items[coffer.ItemId] += 1;

            tmp.Coffers[coffer.Coffer] = cofferRarity;
            deepDungeons[type] = tmp;
        }

        var deepDungeonlist = new List<MultiCofferData>();
        foreach (var (territory, deepDungeonTemp) in deepDungeons)
        {
            var coffers = new List<MultiCofferData.Coffer>();
            foreach (var (coffer, cofferTemp) in deepDungeonTemp.Coffers)
            {
                var primaryList = new List<ResultItem>();

                foreach (var (itemId, amount) in cofferTemp.Items)
                {
                    var item = Sheets.ItemSheet.GetRow(itemId);
                    primaryList.Add(new ResultItem(item.Name.ExtractText(), item.Icon, (uint)amount, amount / cofferTemp.Total));

                    await IconHelper.CreateIcon(item);
                }

                var cofferItem = Sheets.ItemSheet.GetRow(coffer);

                primaryList = primaryList.OrderBy(l => l.Amount).ToList();
                coffers.Add(new MultiCofferData.Coffer(coffer, cofferItem.Name.ExtractText(), cofferTemp.Total, primaryList));
            }

            coffers = coffers.OrderBy(c => c.CofferId).ToList();
            deepDungeonlist.Add(new MultiCofferData((uint)territory, territory.ToName(), deepDungeonTemp.Total, coffers));
        }

        deepDungeonlist = deepDungeonlist.OrderBy(l => l.Territory).ToList();

        await File.WriteAllTextAsync("DeepDungeonData.json", JsonConvert.SerializeObject(deepDungeonlist));
        Console.WriteLine("Done exporting deep dungeon data...");
    }

    private static async Task ReadLockboxData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting lockbox data");
        uint[] validCoffers = [22508, 23142, 23379, 24141, 24142, 24848, 24849, 31357, 33797];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        var deepDungeons = new Dictionary<Territory, CofferTemp>();
        foreach (var coffer in result)
        {
            var type = (LockboxTypes)coffer.Coffer;
            deepDungeons.TryAdd(type.ToTerritory(), new CofferTemp());

            var tmp = deepDungeons[type.ToTerritory()];

            tmp.Total += 1;
            tmp.Coffers.TryAdd(coffer.Coffer, new CofferTemp.Coffer());

            var cofferRarity =  tmp.Coffers[coffer.Coffer];

            cofferRarity.Total += 1;
            if (!cofferRarity.Items.TryAdd(coffer.ItemId, 1))
                cofferRarity.Items[coffer.ItemId] += 1;

            tmp.Coffers[coffer.Coffer] = cofferRarity;
            deepDungeons[type.ToTerritory()] = tmp;
        }

        var deepDungeonlist = new List<MultiCofferData>();
        foreach (var (territory, deepDungeonTemp) in deepDungeons)
        {
            var coffers = new List<MultiCofferData.Coffer>();
            foreach (var (coffer, cofferTemp) in deepDungeonTemp.Coffers)
            {
                var primaryList = new List<ResultItem>();

                foreach (var (itemId, amount) in cofferTemp.Items)
                {
                    var item = Sheets.ItemSheet.GetRow(itemId);
                    primaryList.Add(new ResultItem(item.Name.ExtractText(), item.Icon, (uint)amount, amount / cofferTemp.Total));

                    await IconHelper.CreateIcon(item);
                }

                var cofferItem = Sheets.ItemSheet.GetRow(coffer);

                primaryList = primaryList.OrderBy(l => l.Amount).ToList();
                coffers.Add(new MultiCofferData.Coffer(coffer, cofferItem.Name.ExtractText(), cofferTemp.Total, primaryList));
            }

            coffers = coffers.OrderBy(c => c.CofferId).ToList();
            deepDungeonlist.Add(new MultiCofferData((uint)territory, territory.ToName(), deepDungeonTemp.Total, coffers));
        }

        deepDungeonlist = deepDungeonlist.OrderBy(l => l.Territory).ToList();

        await File.WriteAllTextAsync("LockboxData.json", JsonConvert.SerializeObject(deepDungeonlist));
        Console.WriteLine("Done exporting lockbox data...");
    }

    public record ResultItem(string Name, uint Icon, uint Amount, double Percentage, uint Total = 0, uint Min = 0, uint Max = 0);
}

public class Exporter
{
    private readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
    private readonly CsvConfiguration CsvReaderConfig = new(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

    public SheetHandler SheetHandler;
    private DirectoryInfo? Directory;

    public Exporter()
    {
        SheetHandler = new SheetHandler();
    }

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

        SheetHandler.GachaHandler.ReadCofferData(result, "Anemos", 22508);
        SheetHandler.GachaHandler.ReadCofferData(result, "Pagos", 23142);
        SheetHandler.GachaHandler.ReadCofferData(result, "Pagos", 23379, 7);
        SheetHandler.GachaHandler.ReadCofferData(result, "Pyros", 24141);
        SheetHandler.GachaHandler.ReadCofferData(result, "Pyros", 24142, 7);
        SheetHandler.GachaHandler.ReadCofferData(result, "Hydatos", 24848);
        SheetHandler.GachaHandler.ReadCofferData(result, "Hydatos", 24849, 7);
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

        SheetHandler.GachaHandler.ReadCofferData(result, "Bozja", 31357);
        SheetHandler.GachaHandler.ReadCofferData(result, "Bozja", 33797, 7);
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

        SheetHandler.GachaHandler.ReadCofferData(result, "Grand Company", 36635);
        SheetHandler.GachaHandler.ReadCofferData(result, "Grand Company", 36636, 7);
        SheetHandler.GachaHandler.ReadCofferData(result, "Venture", 32161);
        SheetHandler.GachaHandler.ReadCofferData(result, "Sanctuary", 41667);
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

        SheetHandler.GachaHandler.ReadCofferData(result, "PotD", 16170, 3);
        SheetHandler.GachaHandler.ReadCofferData(result, "PotD", 16171, 7);
        SheetHandler.GachaHandler.ReadCofferData(result, "PotD", 16172, 11);
        SheetHandler.GachaHandler.ReadCofferData(result, "PotD", 16173, 15);
        SheetHandler.GachaHandler.ReadCofferData(result, "HoH", 23223, 3);
        SheetHandler.GachaHandler.ReadCofferData(result, "HoH", 23224, 7);
        SheetHandler.GachaHandler.ReadCofferData(result, "HoH", 23225, 11);
        SheetHandler.GachaHandler.ReadCofferData(result, "EO", 38945, 3);
        SheetHandler.GachaHandler.ReadCofferData(result, "EO", 38946, 7);
        SheetHandler.GachaHandler.ReadCofferData(result, "EO", 38947, 11);
        Console.WriteLine("Done exporting deep dungeon data...");
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

        SheetHandler.GachaHandler.ReadFragmentData(result);
        Console.WriteLine("Done exporting fragment data...");
    }

    public async Task ExportBunnyData(DatabaseContext context)
    {
        Console.WriteLine("Exporting bunny data");
        var result = await context.Bunny.OrderBy(l => l.Id).ToArrayAsync();

        Console.WriteLine($"Rows found {result.Length}");
        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        // Pagos
        SheetHandler.BunnyHandler.ReadBunnyData(result, "Bnuuy PA", 763, 2009532);
        SheetHandler.BunnyHandler.ReadBunnyData(result, "Bnuuy PA", 763, 2009531, 7);
        SheetHandler.BunnyHandler.ReadBunnyData(result, "Bnuuy PA", 763, 2009530, 11);

        // Pyros
        SheetHandler.BunnyHandler.ReadBunnyData(result, "Bnuuy PY", 795, 2009532);
        SheetHandler.BunnyHandler.ReadBunnyData(result, "Bnuuy PY", 795, 2009531, 7);
        SheetHandler.BunnyHandler.ReadBunnyData(result, "Bnuuy PY", 795, 2009530, 11);

        // Hydatos
        SheetHandler.BunnyHandler.ReadBunnyData(result, "Bnuuy HY", 827, 2009532);
        SheetHandler.BunnyHandler.ReadBunnyData(result, "Bnuuy HY", 827, 2009531, 7);
        SheetHandler.BunnyHandler.ReadBunnyData(result, "Bnuuy HY", 827, 2009530, 11);
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

    public T[] ReadCsv<T>(string fileName)
    {
        using var reader = new StreamReader($"{fileName}.csv");
        using var csv = new CsvReader(reader, CsvReaderConfig);

        return csv.GetRecords<T>().ToArray();
    }

    #region Done
    [Obsolete("Not used anymore, enough data collected")]
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
    #endregion
}