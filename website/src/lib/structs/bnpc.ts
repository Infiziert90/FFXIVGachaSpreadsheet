export interface BnpcPairing {
    BPairs: Record<number, Pairing>;
}

export interface Pairing {
    R: number;
    B: number;
    N: number;
    M: number;

    L: Record<number, Location>;
}

interface Location {
    T: number;
    M: number;
    L: number;
    FL: number;
    FE: number;

    P: Position[];
}

interface Position {
    P: { X: number, Y: number, Z: number };
    N: boolean;
}