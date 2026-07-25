import {getUniqueHash} from "$lib/utils";
import {CalcTime, type SubExplorationRow} from "$lib/sheets/structure/submarines/subExploration";
import {SimpleSubExplorationSheet} from "$lib/sheets/simplifiedSheets";
import {findMapFromSector, findVoyageStart, findVoyageStartPretty, numToLetter} from "$lib/submarines/utils";
import type {RouteBuild} from "$lib/submarines/build";
import {CalculateExpForSectors} from "$lib/submarines/sector";
import {CalculatedRoutes, HashedRoutes} from "$lib/submarines/calculated";

const FixedVoyageTime: number = 43200; // 12h

export interface BestRoute {
    Distance: number;
    Path: number[];
    PathPretty: SubExplorationRow[];
}

export function CreateBestRoute(distance: number, path: number[]): BestRoute {
    return {
        Distance: distance,
        Path: path,
        PathPretty: path.map(s => SimpleSubExplorationSheet[s])
    }
}

export function EmptyBestRoute(): BestRoute {
    return {
        Distance: 0,
        Path: [],
        PathPretty: []
    }
}

export function ToExplorationArray(sectors: number[]): SubExplorationRow[] {
    return sectors.map(s => SimpleSubExplorationSheet[s]);
}

export function SectorsToPath(separator: string, sectors: number[]): string {
    if (sectors.length === 0) {
        return "No Voyage";
    }

    const start = findVoyageStart(sectors[0]);
    if (start === undefined)
        return "No Voyage";

    return sectors.map(p => numToLetter(p - start, false)).join(separator);
}

export function CalculateDuration(sectors: SubExplorationRow[], speed: number): number
{
    if (sectors.length === 0 || sectors.length > 5)
        return 0;

    let start = findVoyageStartPretty(sectors[0].RowId);
    if (start === undefined)
        return 0;

    if (sectors.length === 1)
        return CalcTime(start, sectors[0], speed) + FixedVoyageTime;

    let durations = CalcTime(start, sectors[0], speed);
    for (let i = 1; i < sectors.length; i++)
        durations += CalcTime(sectors[i - 1], sectors[i], speed);

    return durations + FixedVoyageTime;
}

export function FindBestRoute(build: RouteBuild, unlocked: number[], mustInclude: number[], allowed: number[], ignoreUnlocks: boolean,avgExpBonus: boolean): BestRoute {
    let valid = Object.values(SimpleSubExplorationSheet)
        .filter(s => s.Map == build.MapRowId && !s.StartingPoint && s.RankReq <= build.Rank)
        .filter(s => allowed.length !== 0 ? allowed.includes(s.RowId) : ignoreUnlocks || unlocked.includes(s.RowId))
        .map(r => r.RowId);

    let subBuild = build.GetSubmarineBuild;
    let bestPaths = CalculatedRoutes.Maps[build.MapRowId]
        .filter(r => r.Distance <= subBuild.Range)            // distance sort
        .filter(r => valid.every(s => r.Sectors.includes(s)))       // only valid routes
        .filter(r => mustInclude.every(s => r.Sectors.includes(s))) // must include
        .map(r =>
        {
            let sectors = ToExplorationArray(r.Sectors);
            return {
                Path: r.Sectors,
                Distance: r.Distance,
                Duration: CalculateDuration(sectors, subBuild.Speed),
                Exp: CalculateExpForSectors(sectors, subBuild, avgExpBonus)
            }
        })
        // .Where(t => t.Duration < Plugin.Configuration.DurationLimit.ToSeconds())
        // .OrderByDescending(t => Plugin.Configuration.MaximizeDuration ? t.Exp : t.Exp / (t.Duration / 60))
        // .ThenBy(t => t.Duration)
        .sort((a, b) => (a.Exp > b.Exp ? -1 : 1))
        .sort((a, b) => (a.Duration < b.Duration ? -1 : 1));

    if (bestPaths.length === 0)
        return EmptyBestRoute();

    let bestPath = bestPaths[0];
    return CreateBestRoute(bestPath.Distance, bestPath.Path);
}

export function FindCalculatedRoute(sectors: number[]): BestRoute {
    if (sectors.length === 0)
        return CreateBestRoute(0, []);

    let hash = getUniqueHash(sectors);
    let map = findMapFromSector(sectors[0]);
    if (!(hash in HashedRoutes[map]))
        return CreateBestRoute(0, []);

    console.log(`Hash: ${hash} for map ${map}`)
    let optimizedRoute = HashedRoutes[map][hash];
    console.log(optimizedRoute);
    return CreateBestRoute(optimizedRoute.Distance, optimizedRoute.Sectors);
}