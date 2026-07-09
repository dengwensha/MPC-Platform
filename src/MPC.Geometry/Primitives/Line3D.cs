namespace MPC.Geometry.Primitives;

using MPC.Geometry.Contracts;

public readonly record struct Line3D(
    Point3D Start,
    Point3D End) : ICurve3D;