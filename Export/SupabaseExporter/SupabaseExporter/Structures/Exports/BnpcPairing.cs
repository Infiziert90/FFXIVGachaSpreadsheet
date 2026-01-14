using System.Numerics;

namespace SupabaseExporter.Structures.Exports;

public class BnpcPairing
{
    // Used for internal cache keeping
    public uint ProcessedId;
    
    public Dictionary<ulong, Pairing> BnpcPairings = [];

    public class Pairing(uint baseId, uint nameId, ushort kind, ushort battalion)
    {
        public uint Records;

        public uint Base = baseId;
        public uint Name = nameId;
        public ushort Kind = kind;
        public ushort Battalion = battalion;
        
        public Dictionary<uint, Location> Locations = [];
    }

    public class Location(uint territory, uint map, uint level)
    {
        public uint Records;
        
        public uint Territory = territory;
        public uint Map = map;
        public uint Level = level;

        public List<Vector3> Positions = [];
        public Dictionary<int, uint> PositionCounts = []; // index to count
    }
}

public record BnpcSimple(uint Base, HashSet<uint> Names);