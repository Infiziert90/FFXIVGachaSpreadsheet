using System.Numerics;

namespace SupabaseExporter.Structures.Exports;

public class BnpcPairing
{
    public Dictionary<uint, Pairing> BnpcPairings = [];

    public class Pairing
    {
        public uint Records;
        
        public HashSet<uint> Names = [];
        public Dictionary<uint, Location> LevelToLocations = [];
    }

    public class Location(uint territory, uint map)
    {
        public uint Records;
        
        public uint Territory = territory;
        public uint Map = map;
        
        public Dictionary<Vector3, uint> Positions = [];
    }
}