using Lumina.Excel.Sheets;
using static SupabaseExporter.Sheets;

namespace SupabaseExporter.Structures.Sheets;

public class SimplifiedSheets
{
    private readonly Dictionary<uint, MapRow> SimpleMap;
    private readonly Dictionary<uint, TerritoryRow> SimpleTerritory;
    
    private readonly Dictionary<uint, SubMapRow> SimpleSubMap;
    private readonly Dictionary<uint, SubExplorationRow> SimpleSubExploration;
    
    private readonly Dictionary<uint, Dictionary<uint, MapMarkerRow>> SimpleMapMarker;
    private readonly Dictionary<uint, HousingLandSetRow> SimpleHousingLandSet;
    private readonly Dictionary<uint, Dictionary<uint, HousingMapMarkerRow>> SimpleHousingMapMarker;
    
    private readonly Dictionary<uint, WorldRow> SimpleWorld;
    private readonly Dictionary<uint, WorldDCGroupRow> SimpleWorldDcGroup;

    public SimplifiedSheets()
    {
        SimpleMap = MapSheet.Select(MapRow.From).ToDictionary(r => r.RowId, r => r);
        SimpleTerritory = TerritoryTypeSheet.Select(TerritoryRow.From).ToDictionary(r => r.RowId, r => r);
        
        SimpleSubMap = SubmarineMapSheet.Select(SubMapRow.From).ToDictionary(r => r.RowId, r => r);
        SimpleSubExploration = SubmarineExplorationSheet.Select(SubExplorationRow.From).ToDictionary(r => r.RowId, r => r);
        
        SimpleMapMarker = MapMarkerSheet.ToDictionary(baseRow => baseRow.RowId, baseRow => baseRow.Select(MapMarkerRow.From).ToDictionary(subRow => subRow.RowId, subRow => subRow));
        SimpleHousingLandSet = HousingLandSetSheet.Select(HousingLandSetRow.From).ToDictionary(r => r.RowId, r => r);
        SimpleHousingMapMarker = HousingMapMarkerSheet.ToDictionary(baseRow => baseRow.RowId, baseRow => baseRow.Select(HousingMapMarkerRow.From).ToDictionary(subRow => subRow.RowId, subRow => subRow));
        
        SimpleWorld = WorldSheet.Select(WorldRow.From).ToDictionary(r => r.RowId, r => r);
        SimpleWorldDcGroup = WorldDCGroupSheet.Select(WorldDCGroupRow.From).ToDictionary(r => r.RowId, r => r);
    }

    public void Export()
    {
        ExportHandler.WriteSheetJson("map.json", SimpleMap);
        ExportHandler.WriteSheetJson("territory.json", SimpleTerritory);
        
        ExportHandler.WriteSheetJson("subMap.json", SimpleSubMap);
        ExportHandler.WriteSheetJson("subExploration.json", SimpleSubExploration);
        
        ExportHandler.WriteSheetJson("mapMarker.json", SimpleMapMarker);
        ExportHandler.WriteSheetJson("housingLandSet.json", SimpleHousingLandSet);
        ExportHandler.WriteSheetJson("housingMapMarker.json", SimpleHousingMapMarker);
        
        ExportHandler.WriteSheetJson("world.json", SimpleWorld);
        ExportHandler.WriteSheetJson("worldDCGroup.json", SimpleWorldDcGroup);
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
    
    public static TerritoryRow From(TerritoryType territoryType) => new(territoryType);
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
    public ushort MapMarkerRange = map.MapMarkerRange;
    public short OffsetX = map.OffsetX;
    public short OffsetY = map.OffsetY;
    public ushort SizeFactor = map.SizeFactor;
    public uint Territory = map.TerritoryType.RowId;
    public PlaceRow PlaceName = new(map.PlaceName.Value);
    public PlaceRow PlaceNameSub = new(map.PlaceNameSub.Value);
    public PlaceRow PlaceNameRegion = new(map.PlaceNameRegion.Value);
    
    public static MapRow From(Map map) => new(map);
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
    
    public static SubExplorationRow From(SubmarineExploration exploration) => new(exploration);
}

[Serializable]
public struct SubMapRow(SubmarineMap map)
{
    public uint RowId = map.RowId;

    public string Name = map.Name.ToString();
    
    public static SubMapRow From(SubmarineMap map) => new(map);
}

[Serializable]
public struct HousingMapMarkerRow(HousingMapMarkerInfo markerInfo)
{
    public uint RowId = markerInfo.SubrowId;

    public float X = markerInfo.X;
    public float Y = markerInfo.Y;
    public float Z = markerInfo.Z;
    public uint Map = markerInfo.Map.RowId;
    
    public static HousingMapMarkerRow From(HousingMapMarkerInfo markerInfo) => new(markerInfo);
}

[Serializable]
public struct HousingLandSetRow(HousingLandSet landSet)
{
    public uint RowId = landSet.RowId;

    public LandSetEntry[] Sets = landSet.LandSet.Select(LandSetEntry.From).ToArray();
    
    public static HousingLandSetRow From(HousingLandSet landSet) => new(landSet);
}

[Serializable]
public struct LandSetEntry(HousingLandSet.LandSetStruct landSetStruct)
{
    public uint PlacardId = landSetStruct.PlacardId;
    public uint InitialPrice = landSetStruct.InitialPrice;
    public byte PlotSize = landSetStruct.PlotSize;
    
    public static LandSetEntry From(HousingLandSet.LandSetStruct landSetStruct) => new(landSetStruct);
}

[Serializable]
public struct WorldRow(World world)
{
    public uint RowId = world.RowId;
    
    public string Name = world.Name.ToString();
    public uint DataCenter = world.DataCenter.RowId;
    public byte Region = world.Region;
    public bool IsPublic = world.IsPublic;
    
    public static WorldRow From(World world) => new(world);
}

[Serializable]
public struct WorldDCGroupRow(WorldDCGroupType dc)
{
    public uint RowId = dc.RowId;
    
    public string Name = dc.Name.ToString();
    public byte NeolobbyId = dc.NeolobbyId;
    public byte Region = dc.Region;
    
    public static WorldDCGroupRow From(WorldDCGroupType dc) => new(dc);
}

[Serializable]
public struct MapMarkerRow(MapMarker mapMarker)
{
    public uint RowId = mapMarker.SubrowId;
    
    public PlaceRow PlaceNameSubtext = new(mapMarker.PlaceNameSubtext.Value);
    public short X = mapMarker.X;
    public short Y = mapMarker.Y;
    public ushort Icon = mapMarker.Icon;
    public byte SubtextOrientation = mapMarker.SubtextOrientation;
    public byte DataType = mapMarker.DataType;
    
    public static MapMarkerRow From(MapMarker mapMarker) => new(mapMarker);
}