using System.Numerics;

namespace SupabaseExporter.Structures.Exports;

public class BnpcPairing
{
    // Used for internal cache keeping
    public uint ProcessedId;
    
    public Dictionary<ulong, Pairing> BnpcPairings = [];

    public class Pairing(uint baseId, uint nameId)
    {
        public uint Records;

        public uint Base = baseId;
        public uint Name = nameId;
        
        public Dictionary<uint, Location> Locations = [];
    }

    public class Location(uint territory, uint map)
    {
        public uint Records;
        
        public uint Territory = territory;
        public uint Map = map;

        public List<Vector3> Positions = [];
        public Dictionary<int, uint> PositionCounts = []; // index to count
    }
}

public record BnpcSimple(uint Base, HashSet<uint> Names);