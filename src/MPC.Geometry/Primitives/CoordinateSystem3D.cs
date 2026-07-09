namespace MPC.Geometry.Primitives;

public readonly record struct CoordinateSystem3D(
    Point3D Origin,
    Vector3D XAxis,
    Vector3D YAxis,
    Vector3D ZAxis);