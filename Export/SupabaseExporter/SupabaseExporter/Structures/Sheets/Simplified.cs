using Lumina.Excel.Sheets;
using static SupabaseExporter.Sheets;

namespace SupabaseExporter.Structures.Sheets;

public class SimplifiedSheets
{
    private Dictionary<uint, MapRow> SimpleMap;
    private Dictionary<uint, TerritoryRow> SimpleTerritory;
    
    private Dictionary<uint, SubMapRow> SimpleSubMap;
    private Dictionary<uint, SubExplorationRow> SimpleSubExploration;

    public SimplifiedSheets()
    {
        SimpleMap = MapSheet.Select(r => new MapRow(r)).ToDictionary(r => r.RowId, r => r);
        SimpleTerritory = TerritoryTypeSheet.Select(r => new TerritoryRow(r)).ToDictionary(r => r.RowId, r => r);
        
        SimpleSubMap = SubmarineMapSheet.Select(r => new SubMapRow(r)).ToDictionary(r => r.RowId, r => r);
        SimpleSubExploration = SubmarineExplorationSheet.Select(r => new SubExplorationRow(r)).ToDictionary(r => r.RowId, r => r);
    }

    public void Export()
    {
        ExportHandler.WriteSheetJson("map.json", SimpleMap);
        ExportHandler.WriteSheetJson("territory.json", SimpleTerritory);
        
        ExportHandler.WriteSheetJson("subMap.json", SimpleSubMap);
        ExportHandler.WriteSheetJson("subExploration.json", SimpleSubExploration);
    }
}

[Serializable]
public struct TerritoryRow(TerritoryType territoryType)
{
    public uint RowId = territoryType.RowId;

    public uint Map = territoryType.Map.RowId;
    public uint QuestBattle = territoryType.QuestBattle.RowId;
    public uint ContentFinderCondition = territoryType.ContentFinderCondition.RowId;
    public PlaceRow PlaceName = new(territoryType.PlaceName.Value);
    public PlaceRow PlaceNameRegion = new(territoryType.PlaceNameRegion.Value);
    public PlaceRow PlaceNameZone = new(territoryType.PlaceNameZone.Value);
}

[Serializable]
public struct PlaceRow(PlaceName placeName)
{
    public uint RowId = placeName.RowId;
    
    public string Name = placeName.Name.ToString();
}

[Serializable]
public struct MapRow(Map map)
{
    public uint RowId = map.RowId;

    public string Id = map.Id.ToString();
    public short OffsetX = map.OffsetX;
    public short OffsetY = map.OffsetY;
    public ushort SizeFactor = map.SizeFactor;
    public PlaceRow PlaceName = new(map.PlaceName.Value);
    public PlaceRow PlaceNameSub = new(map.PlaceNameSub.Value);
    public PlaceRow PlaceNameRegion = new(map.PlaceNameRegion.Value);
}

[Serializable]
public struct SubExplorationRow(SubmarineExploration exploration)
{
    public uint RowId = exploration.RowId;
    
    public string Destination = exploration.Destination.ToString();
    public string Location = exploration.Location.ToString();
    public uint ExpReward = exploration.ExpReward;
    public ushort SurveyDurationmin = exploration.SurveyDurationmin;
    public float X = exploration.X;
    public float Y = exploration.Y;
    public float Z = exploration.Z;
    public uint Map = exploration.Map.RowId;
    public byte Stars = exploration.Stars;
    public byte RankReq = exploration.RankReq;
    public byte CeruleumTankReq = exploration.CeruleumTankReq;
    public byte SurveyDistance = exploration.SurveyDistance;
    public bool StartingPoint = exploration.StartingPoint;
}

[Serializable]
public struct SubMapRow(SubmarineMap map)
{
    public uint RowId = map.RowId;

    public string Name = map.Name.ToString();
}