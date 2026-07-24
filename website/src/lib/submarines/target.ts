import type {SubmarineBuild} from "$lib/submarines/build";
import {CalculateBreakpoint} from "$lib/submarines/sector";

export class TargetValues {
    public MinSurveillance: number = 0;
    public MinRetrieval: number = 0;
    public MinSpeed: number = 0;
    public MinRange: number = 0;
    public MinFavor: number = 0;
    public MaxSurveillance: number = 0;
    public MaxRetrieval: number = 0;
    public MaxSpeed: number = 0;
    public MaxRange: number = 0;
    public MaxFavor: number = 0;

    public UseT1: boolean = false;
    public UseT2: boolean = false;
    public UsePoor: boolean = false;
    public UseNormal: boolean = false;
    public IgnoreFavor: boolean = false;
    public NoModded: boolean = false;

    constructor() {}

    static FromBuilds(allBuilds: SubmarineBuild[]): TargetValues {
        let newTargetValues: TargetValues = new TargetValues();

        newTargetValues.MinSurveillance = Math.min(...allBuilds.map(s => s.Surveillance));
        newTargetValues.MinRetrieval = Math.min(...allBuilds.map(s => s.Retrieval))
        newTargetValues.MinSpeed = Math.min(...allBuilds.map(s => s.Speed))
        newTargetValues.MinRange = Math.min(...allBuilds.map(s => s.Range))
        newTargetValues.MinFavor = Math.min(...allBuilds.map(s => s.Favor))

        newTargetValues.MaxSurveillance = Math.max(...allBuilds.map(s => s.Surveillance))
        newTargetValues.MaxRetrieval = Math.max(...allBuilds.map(s => s.Retrieval))
        newTargetValues.MaxSpeed = Math.max(...allBuilds.map(s => s.Speed))
        newTargetValues.MaxRange = Math.max(...allBuilds.map(s => s.Range))
        newTargetValues.MaxFavor = Math.max(...allBuilds.map(s => s.Favor))

        return newTargetValues;
    }

    static FromTarget(lockedTarget: TargetValues): TargetValues {
        let newTargetValues = new TargetValues();

        newTargetValues.MinSurveillance = lockedTarget.MinSurveillance;
        newTargetValues.MinRetrieval = lockedTarget.MinRetrieval;
        newTargetValues.MinSpeed = lockedTarget.MinSpeed;
        newTargetValues.MinRange = lockedTarget.MinRange;
        newTargetValues.MinFavor = lockedTarget.MinFavor;
        newTargetValues.MaxSurveillance = lockedTarget.MaxSurveillance;
        newTargetValues.MaxRetrieval = lockedTarget.MaxRetrieval;
        newTargetValues.MaxSpeed = lockedTarget.MaxSpeed;
        newTargetValues.MaxRange = lockedTarget.MaxRange;
        newTargetValues.MaxFavor = lockedTarget.MaxFavor;

        return newTargetValues;
    }

    Filter(build: SubmarineBuild): boolean {
        return build.Surveillance >= this.MinSurveillance &&
            build.Retrieval >= this.MinRetrieval &&
            build.Speed >= this.MinSpeed &&
            build.Range >= this.MinRange &&
            build.Favor >= this.MinFavor &&
            build.Surveillance <= this.MaxSurveillance &&
            build.Retrieval <= this.MaxRetrieval &&
            build.Speed <= this.MaxSpeed &&
            build.Range <= this.MaxRange &&
            build.Favor <= this.MaxFavor;
    }

    SectorFilter(build: SubmarineBuild, sectors: number[]): boolean {
        let breakpoints = CalculateBreakpoint(sectors);

        return build.Surveillance >= (this.UseT1 ? 0 : this.UseT2 ? breakpoints.T2 : breakpoints.T3) &&
            build.Retrieval >= (this.UsePoor ? 0 : this.UseNormal ? breakpoints.Normal : breakpoints.Optimal) &&
            (this.IgnoreFavor || build.Favor >= breakpoints.Favor) &&
            (!this.NoModded || build.HighestRankPart < 50);
    }
}