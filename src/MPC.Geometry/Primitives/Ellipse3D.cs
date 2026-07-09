namespace MPC.Geometry.Primitives;

public readonly record struct Ellipse3D(
    Point3D Center,
    Vector3D Normal,
    double MajorRadius,
    double MinorRadius);