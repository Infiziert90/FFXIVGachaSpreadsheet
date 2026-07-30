using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SupabaseExporter.Models;

[Table("FateReward")]
public class FateRewardModel : BaseModel
{
    [Name("id")]
    [Column("id")]
    public uint Id { get; set; }    
    
    [Name("territory")]
    [Column("territory")]
    public uint Territory { get; set; }
    
    [Name("map")]
    [Column("map")]
    public uint Map { get; set; }

    [Name("client_language")]
    [Column("client_language")]
    public byte ClientLanguage { get; set; }

    [Name("type")]
    [Column("type")]
    public byte Type { get; set; }

    [Name("success")]
    [Column("success")]
    public byte Success { get; set; }
    
    [Name("name")]
    [Column("name")]
    public string Name { get; set; }
    
    [Name("icon")]
    [Column("icon")]
    public uint Icon { get; set; }

    [Name("medal")]
    [Column("medal")]
    public byte Medal { get; set; }

    [Name("fate_id")]
    [Column("fate_id")]
    public uint FateId { get; set; }

    [Name("eureka_fate")]
    [Column("eureka_fate")]
    public byte EurekaFate { get; set; }
    
    [Name("experience")]
    [Column("experience")]
    public uint Experience { get; set; }

    [Name("experience_flags")]
    [Column("experience_flags")]
    public byte ExperienceFlags { get; set; }

    [Name("currency_amount")]
    [Column("currency_amount")]
    public uint CurrencyAmount { get; set; }

    [Name("currency_flags")]
    [Column("currency_flags")]
    public byte CurrencyFlags { get; set; }

    [Name("fate_token_type_id")]
    [Column("fate_token_type_id")]
    public byte FateTokenTypeId { get; set; }

    [Name("fate_token_type_item_id")]
    [Column("fate_token_type_item_id")]
    public uint FateTokenTypeItemId { get; set; }

    [Name("fate_token_type_amount")]
    [Column("fate_token_type_amount")]
    public uint FateTokenTypeAmount { get; set; }

    [Name("fate_token_type_flags")]
    [Column("fate_token_type_flags")]
    public byte FateTokenTypeFlags { get; set; }

    [Name("grand_company")]
    [Column("grand_company")]
    public byte GrandCompany { get; set; }

    [Name("gc_seals_amount")]
    [Column("gc_seals_amount")]
    public uint GCSealsAmount { get; set; }

    [Name("item_processed_bits")]
    [Column("item_processed_bits")]
    public byte ItemProcessedBits { get; set; }

    [Name("item_processed_count")]
    [Column("item_processed_count")]
    public byte ItemProcessedCount { get; set; }

    [Name("rewards")]
    [NotMapped]
    public string Rewards { get; set; } = string.Empty;
    
    [Column("rewards")]
    [Ignore]
    public uint[] RewardsArray { get; set; } = new uint[20]; // There shouldn't be more than 10 items at any time, 20 is just safety

    [Name("additional_rewards")]
    [NotMapped]
    public string AdditionalRewards { get; set; } = string.Empty;
    
    [Column("additional_rewards")]
    [Ignore]
    public uint[] AdditionalRewardsArray { get; set; } = new uint[12]; // There shouldn't be more than 6 items at any time, 12 is just safety
        
    public FateRewardModel() {}

    public IEnumerable<(uint, uint)> GetRewards() 
        => Utils.PairIter(ProcessRewards());
    
    public IEnumerable<(uint, uint)> GetAdditionalRewards() 
        => Utils.PairIter(ProcessAdditionalRewards());
        
    private uint[] ProcessRewards()
    {
        if (Rewards == string.Empty)
        {
            if (RewardsArray.Length > 10)
            {
                Logger.Error($"Invalid length found, ID: {Id}");
                return [];
            }
            
            if (RewardsArray.Any(reward => reward > 1_000_000))
            {
                Logger.Error($"Invalid treasure result found, ID: {Id}");
                return [];
            }

            return RewardsArray;
        }

        var span = Rewards.Trim('{', '}').AsSpan();

        var counter = 0;
        foreach (var range in span.Split(','))
        {
            if (counter >= 6)
            {
                Logger.Error($"Invalid length found, ID: {Id}");
                return [];
            }
            
            var result = uint.Parse(span[range]);
            if (result > 1_000_000)
            {
                Logger.Error($"Invalid treasure result found, ID: {Id}");
                return [];
            }
                
            RewardsArray[counter] = result;
            counter++;
        }

        Rewards = string.Empty;
        return RewardsArray;
    }
    
    private uint[] ProcessAdditionalRewards()
    {
        if (AdditionalRewards == string.Empty)
        {
            if (AdditionalRewardsArray.Length > 6)
            {
                Logger.Error($"Invalid length found, ID: {Id}");
                return [];
            }
            
            if (AdditionalRewardsArray.Any(reward => reward > 1_000_000))
            {
                Logger.Error($"Invalid treasure result found, ID: {Id}");
                return [];
            }

            return AdditionalRewardsArray;
        }

        var span = AdditionalRewards.Trim('{', '}').AsSpan();

        var counter = 0;
        foreach (var range in span.Split(','))
        {
            if (counter >= 6)
            {
                Logger.Error($"Invalid length found, ID: {Id}");
                return [];
            }
            
            var result = uint.Parse(span[range]);
            if (result > 1_000_000)
            {
                Logger.Error($"Invalid treasure result found, ID: {Id}");
                return [];
            }
                
            AdditionalRewardsArray[counter] = result;
            counter++;
        }

        AdditionalRewards = string.Empty;
        return AdditionalRewardsArray;
    }
}

public sealed class FateRewardExportMap : ClassMap<FateRewardModel>
{
    public FateRewardExportMap()
    {
        
        Map(m => m.Version).Name("version");
        
        Map(m => m.Id).Name("id");
        Map(m => m.Territory).Name("territory");
        Map(m => m.Map).Name("map");
        Map(m => m.ClientLanguage).Name("client_language");
        Map(m => m.Type).Name("type");
        Map(m => m.Success).Name("success");
        Map(m => m.Name).Name("name");
        Map(m => m.Icon).Name("icon");
        Map(m => m.Medal).Name("medal");
        Map(m => m.FateId).Name("fate_id");
        Map(m => m.EurekaFate).Name("eureka_fate");
        Map(m => m.Experience).Name("experience");
        Map(m => m.ExperienceFlags).Name("experience_flags");
        Map(m => m.CurrencyAmount).Name("currency_amount");
        Map(m => m.CurrencyFlags).Name("currency_flags");
        Map(m => m.FateTokenTypeId).Name("fate_token_type_id");
        Map(m => m.FateTokenTypeItemId).Name("fate_token_type_item_id");
        Map(m => m.FateTokenTypeAmount).Name("fate_token_type_amount");
        Map(m => m.FateTokenTypeFlags).Name("fate_token_type_flags");
        Map(m => m.GrandCompany).Name("grand_company");
        Map(m => m.GCSealsAmount).Name("gc_seals_amount");
        Map(m => m.ItemProcessedBits).Name("item_processed_bits");
        Map(m => m.ItemProcessedCount).Name("item_processed_count");
        
        Map(m => m.RewardsArray).Name("rewards").Convert(l =>
        {
            l.Value.GetRewards();
            return $"{{{string.Join(",", l.Value.RewardsArray)}}}";
        });
        Map(m => m.AdditionalRewardsArray).Name("additional_rewards").Convert(l =>
        {
            l.Value.GetAdditionalRewards();
            return $"{{{string.Join(",", l.Value.AdditionalRewardsArray)}}}";
        });
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}

public sealed class FateRewardImportMap : ClassMap<FateRewardModel>
{
    public FateRewardImportMap()
    {
        Map(m => m.Version).Name("version");
        
        Map(m => m.Id).Name("id");
        Map(m => m.Territory).Name("territory");
        Map(m => m.Map).Name("map");
        Map(m => m.ClientLanguage).Name("client_language");
        Map(m => m.Type).Name("type");
        Map(m => m.Success).Name("success");
        Map(m => m.Name).Name("name");
        Map(m => m.Icon).Name("icon");
        Map(m => m.Medal).Name("medal");
        Map(m => m.FateId).Name("fate_id");
        Map(m => m.EurekaFate).Name("eureka_fate");
        Map(m => m.Experience).Name("experience");
        Map(m => m.ExperienceFlags).Name("experience_flags");
        Map(m => m.CurrencyAmount).Name("currency_amount");
        Map(m => m.CurrencyFlags).Name("currency_flags");
        Map(m => m.FateTokenTypeId).Name("fate_token_type_id");
        Map(m => m.FateTokenTypeItemId).Name("fate_token_type_item_id");
        Map(m => m.FateTokenTypeAmount).Name("fate_token_type_amount");
        Map(m => m.FateTokenTypeFlags).Name("fate_token_type_flags");
        Map(m => m.GrandCompany).Name("grand_company");
        Map(m => m.GCSealsAmount).Name("gc_seals_amount");
        Map(m => m.ItemProcessedBits).Name("item_processed_bits");
        Map(m => m.ItemProcessedCount).Name("item_processed_count");
        
        Map(m => m.Rewards).Name("rewards");
        Map(m => m.AdditionalRewards).Name("additional_rewards");
        
        Map(m => m.GetVersion).Ignore();
        Map(m => m.GetPatch).Ignore();
    }
}