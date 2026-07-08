namespace MPC.Geometry.Primitives;

public readonly record struct Circle3D(
    Point3D Center,
    Vector3D Normal,
    double Radius);