import {getUniqueHash} from "$lib/utils";
import {unpack} from "msgpackr";

type Fetch = typeof fetch;

interface CalculatedData {
    MaxSector: number;
    Maps: {[id: number]: Route[]};
}

interface Route {
    Distance: number;
    Sectors: number[];
}

export const CalculatedRoutes: CalculatedData = {MaxSector: 0, Maps: {}}
export const HashedRoutes: Record<number, Record<number, Route>> = {};

export async function importCalculatedData(fetch: Fetch) {
    let response = await fetch('/msgpack/CalculatedData.msgpack');
    let arrayBuffer  = await response.arrayBuffer();
    let data = unpack(arrayBuffer)

    CalculatedRoutes.MaxSector = data[0];
    for (const [key, item] of Object.entries(data[1])) {
        let mapId = parseInt(key);

        CalculatedRoutes.Maps[mapId] = [];
        for (const route of item) {
            CalculatedRoutes.Maps[mapId].push({
                Distance: route[0],
                Sectors: route[1],
            })
        }
    }

    for (const [map, routes] of Object.entries(CalculatedRoutes.Maps))
    {
        let dict: Record<number, Route> = {}
        for (const route of routes) {
            dict[getUniqueHash(route.Sectors)] = route;
        }

        HashedRoutes[parseInt(map)] = dict;
    }
}