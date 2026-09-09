using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepBSplineSurfaceWithKnotsGeometryMapper :
    IEntityMapper<StepBSplineSurfaceWithKnotsEntity, BSplineSurface3D>
{
    private const string InvalidControlPointDiagnosticCode =
        "MAP-STEP-BSPLINE-SURFACE-001";

    private const string InvalidSurfaceDiagnosticCode =
        "MAP-STEP-BSPLINE-SURFACE-002";

    public MappingResult<BSplineSurface3D> Map(
        StepBSplineSurfaceWithKnotsEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        int checkpoint = context.CaptureDiagnosticCheckpoint();
        string sourceIdentity = string.IsNullOrWhiteSpace(source.SourceEntityId)
            ? source.SourceEntityType
            : source.SourceEntityId;

        int uDegree;
        int vDegree;
        int[] uMultiplicities;
        int[] vMultiplicities;

        try
        {
            uDegree = checked((int)source.UDegree);
            vDegree = checked((int)source.VDegree);
            uMultiplicities = source.UMultiplicities
                .Select(value => checked((int)value)).ToArray();
            vMultiplicities = source.VMultiplicities
                .Select(value => checked((int)value)).ToArray();
        }
        catch (OverflowException exception)
        {
            return CreateFailure(
                context, checkpoint, InvalidSurfaceDiagnosticCode,
                $"STEP B_SPLINE_SURFACE_WITH_KNOTS '{sourceIdentity}' " +
                "contains a degree or multiplicity outside the canonical integer range. " +
                exception.Message);
        }

        var controlPointGrid = new List<Point3D[]>(
            source.ControlPointReferenceGrid.Count);

        for (int u = 0; u < source.ControlPointReferenceGrid.Count; u++)
        {
            IReadOnlyList<string> referenceRow = source.ControlPointReferenceGrid[u];
            var pointRow = new Point3D[referenceRow.Count];

            for (int v = 0; v < referenceRow.Count; v++)
            {
                string referenceId = referenceRow[v];
                ReferenceResolutionResult<StepCartesianPointEntity> resolution =
                    context.ReferenceResolver.Resolve<StepCartesianPointEntity>(referenceId);

                if (!resolution.IsResolved)
                {
                    context.AddDiagnostics(resolution.Diagnostics);
                    return MappingResult<BSplineSurface3D>.Failure(
                        context.GetDiagnosticsSince(checkpoint));
                }

                StepCartesianPointEntity point = resolution.Value
                    ?? throw new InvalidOperationException(
                        "A resolved CARTESIAN_POINT reference must contain an entity.");

                if (point.Coordinates.Count != 3)
                {
                    return CreateFailure(
                        context, checkpoint, InvalidControlPointDiagnosticCode,
                        $"STEP B_SPLINE_SURFACE_WITH_KNOTS '{sourceIdentity}' " +
                        $"control point '{referenceId}' at U[{u}], V[{v}] " +
                        "(zero-based indices) must contain exactly three coordinates " +
                        "for BSplineSurface3D mapping.");
                }

                pointRow[v] = new Point3D(
                    point.Coordinates[0],
                    point.Coordinates[1],
                    point.Coordinates[2]);
            }

            controlPointGrid.Add(pointRow);
        }

        BSplineSurface3D surface;

        try
        {
            surface = new BSplineSurface3D(
                uDegree,
                vDegree,
                controlPointGrid,
                uMultiplicities,
                vMultiplicities,
                source.UKnots,
                source.VKnots);
        }
        catch (ArgumentException exception)
        {
            return CreateFailure(
                context, checkpoint, InvalidSurfaceDiagnosticCode,
                $"STEP B_SPLINE_SURFACE_WITH_KNOTS '{sourceIdentity}' " +
                "could not be mapped to a valid canonical B-spline surface. " +
                exception.Message);
        }

        IReadOnlyList<MappingDiagnostic> diagnostics =
            context.GetDiagnosticsSince(checkpoint);

        if (diagnostics.Count > 0)
        {
            return MappingResult<BSplineSurface3D>.SuccessWithDiagnostics(
                surface, diagnostics);
        }

        return MappingResult<BSplineSurface3D>.Success(surface);
    }

    private static MappingResult<BSplineSurface3D> CreateFailure(
        MappingContext context,
        int checkpoint,
        string code,
        string message)
    {
        context.AddDiagnostic(new MappingDiagnostic(
            code, message, MappingDiagnosticSeverity.Error));

        return MappingResult<BSplineSurface3D>.Failure(
            context.GetDiagnosticsSince(checkpoint));
    }
}
