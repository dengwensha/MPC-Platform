namespace MPC.Geometry.Primitives;

public readonly record struct BoundingBox3D(
    Point3D Min,
    Point3D Max);