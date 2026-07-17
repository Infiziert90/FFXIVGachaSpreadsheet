export interface BnpcPairing {
    BnpcPairings: Record<number, Pairing>;
}

export interface Pairing {
    Records: number;
    Base: number;
    Name: number;
    Locations: Record<number, Location>;
}

interface Location {
    Territory: number;
    Map: number;
    Level: number;

    Positions: { X: number, Y: number, Z: number }[];
}