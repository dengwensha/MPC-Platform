namespace MPC.Connectors.Snapshots.Geometry;

public readonly record struct CadAxis3(
    CadPoint3 Origin,
    CadVector3 Direction);