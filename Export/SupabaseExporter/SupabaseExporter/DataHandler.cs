﻿using Lumina.Excel.Sheets;
using Newtonsoft.Json;

namespace SupabaseExporter;

public class DataHandler
{
    public const string WebsitePath = "../../../../../../Website";
    public const string AssetsPath = "assets/data";

    /// <summary>
    /// A simple helper record for named JSON keys.
    /// </summary>
    public record ResultItem(string Name, uint Icon, uint Amount, double Percentage, uint Total = 0, uint Min = 0, uint Max = 0);

    /// <summary>
    /// A simple helper record for named JSON keys.
    /// </summary>
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

    /// <summary>
    /// A simple helper record for named JSON keys.
    /// </summary>
    public record VentureData
    {
        public uint Category;
        public string Name;
        public List<VentureTask> Tasks = [];

        public VentureData(RetainerTask task)
        {
            Category = task.ClassJobCategory.RowId;

            Name = Category switch
            {
                0 => "All",
                34 => "DoW/DoM",
                _ => task.ClassJobCategory.Value.Name.ExtractText()
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

    /// <summary>
    /// A simple helper struct for data collection before processing.
    /// </summary>
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

    /// <summary>
    /// A simple helper struct for data collection before processing.
    /// </summary>
    public struct CofferTemp
    {
        public long Total = 0;
        public Dictionary<uint, Coffer> Coffers = [];

        public CofferTemp() { }

        public struct Coffer
        {
            public long Total = 0;
            public Dictionary<uint, double> Items = [];

            public Coffer() { }
        }
    }

    public async Task PrintVentureStats(List<Models.Venture> data)
    {
        var quickHistory = data.Where(v => v.QuickVenture).ToArray();

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

        Console.WriteLine("= Gear Average =");
        Console.WriteLine($"iLvL: {totalLvl / quickHistory.Length:F2} | {totalLvl / validGear.Length:F2}");
        Console.WriteLine($"FC Points: {totalFCPoints / quickHistory.Length:F2} | {totalFCPoints / validGear.Length:F2}");
        Console.WriteLine($"GC Seals: {totalSeals / quickHistory.Length:F2} | {totalSeals / validGear.Length:F2}");
    }

    public async Task ReadVentureData(List<Models.Venture> data)
    {
        Console.WriteLine("Exporting venture data");
        var ventures = new Dictionary<VentureTypes, VentureTemp>();
        foreach (var venture in data)
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

                IconHelper.AddIcon(item);
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

        await WriteDataJson("VentureData.json", ventureList.OrderBy(l => l.Category));
        Console.WriteLine("Done exporting venture data...");
    }

    public async Task ReadBunnyData(List<Models.Bnuuy> data)
    {
        Console.WriteLine("Exporting bunny data");
        var bunnies = new Dictionary<Territory, CofferTemp>();
        foreach (var bunny in data)
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

                    IconHelper.AddIcon(item);
                }

                primaryList = primaryList.OrderBy(l => l.Amount).ToList();
                coffers.Add(new MultiCofferData.Coffer(coffer, ((CofferRarity)coffer).ToName(), cofferTemp.Total, primaryList));
            }

            coffers = coffers.OrderByDescending(c => c.CofferId).ToList();
            bunnyList.Add(new MultiCofferData((uint)territory, territory.ToName(), bunnyTemp.Total, coffers));
        }

        await WriteDataJson("BunnyData.json", bunnyList.OrderBy(l => l.Territory));
        Console.WriteLine("Done exporting bunny data...");
    }

    public async Task ReadCofferData(List<Models.Gacha> data)
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

                    IconHelper.AddIcon(item);
                }

                var cofferItem = Sheets.ItemSheet.GetRow(coffer);

                primaryList = primaryList.OrderBy(l => l.Amount).ToList();
                coffers.Add(new MultiCofferData.Coffer(coffer, cofferItem.Name.ExtractText(), cofferTemp.Total, primaryList));
            }

            coffers = coffers.OrderBy(c => c.CofferId).ToList();
            cofferList.Add(new MultiCofferData((uint)territory, territory.ToName(), deepDungeonTemp.Total, coffers));
        }

        await WriteDataJson("CofferData.json", cofferList.OrderBy(l => l.Territory));
        Console.WriteLine("Done exporting coffer data...");
    }

    public async Task ReadDeepDungeonData(List<Models.Gacha> data)
    {
        Console.WriteLine("Exporting deep dungeon data");
        uint[] validCoffers = [16170, 16171, 16172, 16173, 23223, 23224, 23225, 38945, 38946, 38947];
        var result = data.Where(l => validCoffers.Contains(l.Coffer)).OrderBy(l => l.Id).ToArray();

        if (result.Length == 0)
        {
            Console.WriteLine("No records found");
            return;
        }

        var deepDungeons = new Dictionary<DeepDungeon, CofferTemp>();
        foreach (var coffer in result)
        {
            var type = EnumExtensions.ToDeepDungeon(coffer.Coffer);
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

                    IconHelper.AddIcon(item);
                }

                var cofferItem = Sheets.ItemSheet.GetRow(coffer);

                primaryList = primaryList.OrderBy(l => l.Amount).ToList();
                coffers.Add(new MultiCofferData.Coffer(coffer, cofferItem.Name.ExtractText(), cofferTemp.Total, primaryList));
            }

            coffers = coffers.OrderBy(c => c.CofferId).ToList();
            deepDungeonlist.Add(new MultiCofferData((uint)territory, territory.ToName(), deepDungeonTemp.Total, coffers));
        }

        await WriteDataJson("DeepDungeonData.json", deepDungeonlist.OrderBy(l => l.Territory));
        Console.WriteLine("Done exporting deep dungeon data...");
    }

    public async Task ReadLockboxData(List<Models.Gacha> data)
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

                    IconHelper.AddIcon(item);
                }

                var cofferItem = Sheets.ItemSheet.GetRow(coffer);

                primaryList = primaryList.OrderBy(l => l.Amount).ToList();
                coffers.Add(new MultiCofferData.Coffer(coffer, cofferItem.Name.ExtractText(), cofferTemp.Total, primaryList));
            }

            coffers = coffers.OrderBy(c => c.CofferId).ToList();
            deepDungeonlist.Add(new MultiCofferData((uint)territory, territory.ToName(), deepDungeonTemp.Total, coffers));
        }

        await WriteDataJson("LockboxData.json", deepDungeonlist.OrderBy(l => l.Territory));
        Console.WriteLine("Done exporting lockbox data...");
    }

    public async Task WriteTimeData()
    {
        var lastUpdate = DateTime.UtcNow.ToString("R");
        await WriteDataJson("LastUpdate.json", lastUpdate);
    }

    public static async Task WriteDataJson<T>(string filename, IEnumerable<T> data)
    {
        await File.WriteAllTextAsync($"{WebsitePath}/{AssetsPath}/{filename}", JsonConvert.SerializeObject(data));
    }
}