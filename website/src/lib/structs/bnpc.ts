export interface BnpcPairing {
    BPairs: Record<number, Pairing>;
}

export interface Pairing {
    R: number;
    B: number;
    N: number;
    L: Record<number, Location>;
}

interface Location {
    T: number;
    M: number;
    L: number;

    P: { X: number, Y: number, Z: number }[];
}