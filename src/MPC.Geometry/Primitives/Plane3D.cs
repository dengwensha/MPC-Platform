namespace MPC.Geometry.Primitives;

public readonly record struct Plane3D(
    Point3D Origin,
    Vector3D Normal);