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

public class BnpcPairingWeb
{
    public Dictionary<ulong, PairingWeb> BPairs = [];

    public class PairingWeb(uint baseId, uint nameId)
    {
        public uint R;

        public uint B = baseId;
        public uint N = nameId;
        
        public Dictionary<uint, LocationWeb> L = [];

        public static PairingWeb From(BnpcPairing.Pairing org)
        {
            var pairing = new PairingWeb(org.Base, org.Name);
            foreach (var (key, value) in org.Locations)
                pairing.L[key] = LocationWeb.From(value);
            
            return pairing;
        }
    }

    public class LocationWeb(uint territory, uint map, uint level)
    {
        public uint T = territory;
        public uint M = map;
        public uint L = level;

        public List<Vector3> P = [];

        public static LocationWeb From(BnpcPairing.Location org)
        {
            return new LocationWeb(org.Territory, org.Map, org.Level) { P = org.Positions };
        }
    }
}

public record BnpcSimple(uint Base, HashSet<uint> Names);