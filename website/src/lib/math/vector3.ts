export class Vector3 {
    public X: number;
    public Y: number;
    public Z: number;

    constructor(x: number, y: number, z: number) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    Add(other: Vector3): Vector3 {
        this.X += other.X;
        this.Y += other.Y;
        this.Z += other.Z;

        return this;
    }

    Subtract(other: Vector3): Vector3 {
        this.X -= other.X;
        this.Y -= other.Y;
        this.Z -= other.Z;

        return this;
    }

    Multiply(other: Vector3): Vector3 {
        this.X *= other.X;
        this.Y *= other.Y;
        this.Z *= other.Z;

        return this;
    }

    Divide(other: Vector3): Vector3 {
        this.X /= other.X;
        this.Y /= other.Y;
        this.Z /= other.Z;

        return this;
    }

    Dot(other: Vector3): number {
        return (this.X * other.X) + (this.Y * other.Y) + (this.Z * other.Z)
    }

    LengthSquared(): number {
        return this.Dot(this);
    }

    Distance(other: Vector3): number {
        return Math.sqrt(this.DistanceSquared(other))
    }

    DistanceSquared(other: Vector3): number {
        return (this.Clone().Subtract(other)).LengthSquared()
    }

    Clone(): Vector3 {
        return new Vector3(this.X, this.Y, this.Z)
    }
}