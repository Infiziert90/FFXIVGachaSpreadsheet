using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

public abstract class BaseModel
{
    [Name("version")]
    [Column("version")]
    public string Version { get; set; } = string.Empty;

    [Ignore]
    private bool IsCalculated;
    
    [Ignore]
    private int NumberVersion;
    
    [Ignore]
    private string Patch = string.Empty;
        
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