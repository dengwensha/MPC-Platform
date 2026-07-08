namespace MPC.Geometry.Primitives;

public readonly record struct Arc3D(
    Point3D Center,
    Vector3D Normal,
    double Radius,
    double StartAngle,
    double EndAngle);