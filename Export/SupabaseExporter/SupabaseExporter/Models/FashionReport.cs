using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

public enum UploadSourcePlugin : uint
{
    Tracky = 0,
    AvantGarde = 1
}

[Table("FashionReport")]
public class FashionReportModel : BaseModel
{
    [Name("id")]
    [Column("id")]
    public uint Id { get; set; }

    [Name("plugin")]
    [Column("plugin")]
    public UploadSourcePlugin Plugin { get; set; } = UploadSourcePlugin.Tracky; // TODO: Version check unique to AvantGarde

    [Name("week_num")]
    [Column("week_num")]
    public ushort WeekNum { get; set; }

    [Name("score")]
    [Column("score")]
    public uint Score { get; set; }

    [Name("hints")]
    [NotMapped]
    public string Hints { get; set; } = string.Empty;

    [Column("hints")]
    [Ignore]
    public uint[] HintsArray { get; set; } = new uint[22]; // 11 for hint ids, 11 for their respective stamp ids

    [Name("items")]
    [NotMapped]
    public string Items { get; set; } = string.Empty;
    
    [Column("items")]
    [Ignore]
    public uint[] ItemsArray { get; set; } = new uint[11]; 

    [Name("dyes")]
    [NotMapped]
    public string Dyes { get; set; } = string.Empty;
    
    [Column("dyes")]
    [Ignore]
    public uint[] DyesArray { get; set; } = new uint[12]; // Enough for double dyes for weapon + left side

    public IEnumerable<uint> GetHints()
        => Utils.PairIter(ProcessHints(), false).Select(x => x.Item1);

    public IEnumerable<uint> GetStamps()
        => Utils.PairIter(ProcessHints(), false).Select(x => x.Item2);

    public IEnumerable<(uint, uint)> GetCategories()
        => Utils.PairIter(ProcessHints(), false);

    public uint[] ProcessHints() // TODO: Add guards for values?
    {
        if (Hints == string.Empty)
            return HintsArray;
 
        var span = Hints.Trim(['{', '}']).AsSpan();

        var counter = 0;
        foreach (var range in span.Split(','))
        {
            HintsArray[counter] = uint.Parse(span[range]);
            counter++;
        }

        Hints = string.Empty;
        return HintsArray;
    }

    public ReadOnlySpan<uint> GetItems()
    {
        if (Items == string.Empty)
            return ItemsArray;
 
        var span = Items.Trim(['{', '}']).AsSpan();

        var counter = 0;
        foreach (var range in span.Split(','))
        {
            ItemsArray[counter] = uint.Parse(span[range]);
            counter++;
        }

        Items = string.Empty;
        return ItemsArray;
    }

    public IEnumerable<(uint, uint)> GetDyes()
       => Utils.PairIter(ProcessDyes(), false);

    public uint[] ProcessDyes()
    {
        if (Dyes == string.Empty)
            return DyesArray;
 
        var span = Dyes.Trim(['{', '}']).AsSpan();

        var counter = 0;
        foreach (var range in span.Split(','))
        {
            DyesArray[counter] = uint.Parse(span[range]);
            counter++;
        }

        Dyes = string.Empty;
        return DyesArray;
    }
}

public sealed class FashionReportExportMap : ClassMap<FashionReportModel>
{
    public FashionReportExportMap()
    {
        Map(m => m.Version).Name("version");

        Map(m => m.Id).Name("id");
        Map(m => m.Plugin).Name("plugin").Convert(row => ((int)row.Value.Plugin).ToString());
        Map(m => m.WeekNum).Name("week_num");
        Map(m => m.Score).Name("score");
        Map(m => m.HintsArray).Name("hints").Convert(l =>
        {
            l.Value.GetCategories();
            return $"{{{string.Join(",", l.Value.HintsArray)}}}";
        });
        Map(m => m.ItemsArray).Name("items").Convert(l =>
        {
            l.Value.GetItems();
            return $"{{{string.Join(",", l.Value.ItemsArray)}}}";
        });
        Map(m => m.DyesArray).Name("dyes").Convert(l =>
        {
            l.Value.GetDyes();
            return $"{{{string.Join(",", l.Value.DyesArray)}}}";
        });
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}

public sealed class FashionReportImportMap : ClassMap<FashionReportModel>
{
    public FashionReportImportMap()
    {
        Map(m => m.Version).Name("version");

        Map(m => m.Id).Name("id");
        Map(m => m.Plugin).Name("plugin");
        Map(m => m.WeekNum).Name("week_num");
        Map(m => m.Score).Name("score");
        Map(m => m.Hints).Name("hints");
        Map(m => m.Items).Name("items");
        Map(m => m.Dyes).Name("dyes");
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}