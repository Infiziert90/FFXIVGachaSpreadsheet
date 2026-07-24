import type {SubRankRow} from "$lib/sheets/structure/submarines/subRank";
import type {SubPartRow} from "$lib/sheets/structure/submarines/subPart";
import {SimpleSubPartSheet, SimpleSubRankSheet} from "$lib/sheets/simplifiedSheets";
import type {SubExplorationRow} from "$lib/sheets/structure/submarines/subExploration";
import type {BestRoute} from "$lib/submarines/voyage";

export class SubmarineBuild {
    public Bonus: SubRankRow;
    public readonly Hull: SubPartRow;
    public readonly Stern: SubPartRow;
    public readonly Bow: SubPartRow;
    public readonly Bridge: SubPartRow;

    constructor(rank: number, hull: number, stern: number, bow: number, bridge: number) {
        this.Bonus = SimpleSubRankSheet[rank];
        this.Hull = SimpleSubPartSheet[hull];
        this.Stern = SimpleSubPartSheet[stern];
        this.Bow = SimpleSubPartSheet[bow];
        this.Bridge = SimpleSubPartSheet[bridge];
    }

    get Range(): number {
        return this.Bonus.RangeBonus + this.Hull.Range + this.Stern.Range + this.Bow.Range + this.Bridge.Range;
    }

    get Speed(): number {
        return this.Bonus.SpeedBonus + this.Hull.Speed + this.Stern.Speed + this.Bow.Speed + this.Bridge.Speed;
    }

    get Surveillance(): number {
        return this.Bonus.SurveillanceBonus + this.Hull.Surveillance + this.Stern.Surveillance + this.Bow.Surveillance + this.Bridge.Surveillance;
    }

    get Retrieval(): number {
        return this.Bonus.RetrievalBonus + this.Hull.Retrieval + this.Stern.Retrieval + this.Bow.Retrieval + this.Bridge.Retrieval;
    }

    get Favor(): number {
        return this.Bonus.FavorBonus + this.Hull.Favor + this.Stern.Favor + this.Bow.Favor + this.Bridge.Favor;
    }

    get RepairCosts(): number {
        return this.Hull.RepairMaterials + this.Stern.RepairMaterials + this.Bow.RepairMaterials + this.Bridge.RepairMaterials;
    }

    get BuildCost(): number {
        return this.Hull.Components + this.Stern.Components + this.Bow.Components + this.Bridge.Components;
    }

    get HighestRankPart(): number {
        return Math.max(...this.GetPartRanks)
    }

    get GetPartRanks(): number[] {
        return [this.Hull.Rank, this.Stern.Rank, this.Bow.Rank, this.Bridge.Rank]
    }


    get HullIdentifier(): string {
        return ToIdentifier(this.Hull.RowId);
    }

    get SternIdentifier(): string {
        return ToIdentifier(this.Stern.RowId);
    }

    get BowIdentifier(): string {
        return ToIdentifier(this.Bow.RowId);
    }

    get BridgeIdentifier(): string {
        return ToIdentifier(this.Bridge.RowId);
    }

    public FullIdentifier(): string {
        let identifier = `${this.HullIdentifier}${this.SternIdentifier}${this.BowIdentifier}${this.BridgeIdentifier}`;

        if (identifier.split('+').length - 1 === 4)
            identifier = `${identifier.replaceAll('+', '')}++`;

        return identifier;
    }

    public static FromRouteBuild(routeBuild: RouteBuild): SubmarineBuild {
        return new SubmarineBuild(
            routeBuild.Rank,
            routeBuild.Hull,
            routeBuild.Stern,
            routeBuild.Bow,
            routeBuild.Bridge,
        )
    }

    public Print() {
        console.log(this)
        console.log(`${this.Bonus.SpeedBonus} + ${this.Hull.Speed} + ${this.Stern.Speed} + ${this.Bow.Speed} + ${this.Bridge.Speed}`)
    }
}

export class RouteBuild {
    public Rank: number = $state(1);
    public Hull: number = $state(3);
    public Stern: number = $state(4);
    public Bow: number = $state(1);
    public Bridge: number = $state(2);

    public Map: number = $state(0);
    public Sectors: number[] = $state([]);

    public OptimizedDistance: number = $state(0);
    public OptimizedRoute: SubExplorationRow[] = $state([]);


    constructor(rank: number, hull: number, stern: number, bow: number, bridge: number) {
        this.Rank = rank;

        this.Hull = hull;
        this.Stern = stern;
        this.Bow = bow;
        this.Bridge = bridge;
    }

    public static get Empty(): RouteBuild {
        return new RouteBuild(1, 3, 4, 1, 2);
    }


    public get MapRowId(): number {
        return this.Map + 1;
    }

    public get GetSubmarineBuild(): SubmarineBuild {
        return SubmarineBuild.FromRouteBuild(this);
    }

    public get FuelCost(): number {
        return this.OptimizedRoute.length !== 0
            ? this.OptimizedRoute.reduce((sum, p) => sum + p.CeruleumTankReq, 0)
            : 0;
    }

    get HullIdentifier(): string {
        return ToIdentifier(this.Hull);
    }
    get SternIdentifier(): string {
        return ToIdentifier(this.Stern);
    }
    get BowIdentifier(): string {
        return ToIdentifier(this.Bow);
    }
    get BridgeIdentifier(): string {
        return ToIdentifier(this.Bridge);
    }

    private get PartArray(): number[] {
        return [this.Bow, this.Bridge, this.Hull, this.Stern];
    }

    public FullIdentifier(): string {
        let identifier = `${this.HullIdentifier}${this.SternIdentifier}${this.BowIdentifier}${this.BridgeIdentifier}`;

        if (identifier.split('+').length - 1 === 4)
            identifier = `${identifier.replaceAll('+', '')}++`;

        return identifier;
    }

    public UpdateOptimized(route: BestRoute) {
        this.Sectors = route.Path;

        this.OptimizedRoute = route.PathPretty;
        this.OptimizedDistance = route.Distance;
    }

    public ChangeMap(newMap: number) {
        this.Map = newMap;

        this.Sectors = [];
        this.OptimizedRoute = [];
        this.OptimizedDistance = 0;
    }
}

function ToIdentifier(partId: number): string {
    switch(Math.floor((partId - 1) / 4)) {
        case 0: return 'S';
        case 1: return 'U';
        case 2: return 'W';
        case 3: return 'C';
        case 4: return 'Y';

        case 5:
        case 6:
        case 7:
        case 8:
        case 9:
            return `${ToIdentifier(partId - 20)}+`;

        default: return 'Unknown';
    }
}