using System.Numerics;

namespace SupabaseExporter.Structures.Exports;

public class BnpcPairing
{
    // Used for internal cache keeping
    public uint ProcessedId;
    
    public Dictionary<ulong, Pairing> BnpcPairings = [];

    public class Pairing(uint baseId, uint nameId, uint model, ushort kind, ushort battalion)
    {
        public uint Records;

        public uint Base = baseId;
        public uint Name = nameId;
        public uint Model = model;
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
        public byte ForayLevel;
        public byte ForayElement;

        public List<Position> Positions = [];
    }

    public class Position(Vector3 pos, bool noTarget, int count)
    {
        public Vector3 Pos = pos;
        public bool NoTarget = noTarget;
        public int Count = count;
    }
}

public class BnpcPairingWeb
{
    public Dictionary<ulong, PairingWeb> BPairs = [];

    public class PairingWeb(uint baseId, uint nameId, uint modelChara)
    {
        public uint R;

        public uint B = baseId;
        public uint N = nameId;
        public uint M = modelChara;
        
        public Dictionary<uint, LocationWeb> L = [];

        public static PairingWeb From(BnpcPairing.Pairing org)
        {
            var pairing = new PairingWeb(org.Base, org.Name, org.Model);
            foreach (var (key, value) in org.Locations)
                pairing.L[key] = LocationWeb.From(value);
            
            return pairing;
        }
    }

    public class LocationWeb(uint territory, uint map, uint level, byte forayLevel, byte forayElement)
    {
        public uint T = territory;
        public uint M = map;
        public uint L = level;
        public byte FL = forayLevel;
        public byte FE = forayElement;

        public List<PositionWeb> P = [];

        public static LocationWeb From(BnpcPairing.Location org)
        {
            var n = new LocationWeb(org.Territory, org.Map, org.Level, org.ForayLevel, org.ForayElement);
            foreach (var pos in org.Positions)
                n.P.Add(new PositionWeb(pos.Pos, pos.NoTarget));
            
            return n;
        }
    }
    
    public class PositionWeb(Vector3 pos, bool noTarget)
    {
        public Vector3 P = pos;
        public bool N = noTarget;
    }
}

public record BnpcSimple(uint Base, HashSet<uint> Names);