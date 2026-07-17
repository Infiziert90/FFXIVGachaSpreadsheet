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
            {
                var type = GetType();
                if (type.GetProperty("Plugin")?.GetValue(this) is UploadSourcePlugin sourcePlugin)
                {
                    Calculate(sourcePlugin);
                }
                else
                {
                    Calculate();
                }
            }
                
            return Patch;
        }
    }

    private void Calculate(UploadSourcePlugin src = UploadSourcePlugin.Tracky)
    {
        IsCalculated = true;
            
        NumberVersion = Utils.VersionToNumber(Version);
        Patch = Utils.VersionToPatch(NumberVersion, src);
    }
}