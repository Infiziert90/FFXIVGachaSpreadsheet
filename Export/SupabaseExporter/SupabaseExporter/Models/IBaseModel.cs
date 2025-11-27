using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

public abstract class BaseModel
{
    [Name("version")]
    [Column("version")]
    public string Version { get; set; } = string.Empty;

    private bool IsCalculated;
    private int NumberVersion;
    private string Patch;
        
    public int GetVersion
    {
        get
        {
            if (!IsCalculated)
                Calculate();
                
            return NumberVersion;
        }
    }
        
    public string GetPatch
    {
        get
        {
            if (!IsCalculated)
                Calculate();
                
            return Patch;
        }
    }

    private void Calculate()
    {
        IsCalculated = true;
            
        NumberVersion = Utils.VersionToNumber(Version);
        Patch = Utils.VersionToPatch(NumberVersion);
    }
}