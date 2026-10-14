using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepBSplineCurveWithKnotsGeometryMapper :
    IEntityMapper<StepBSplineCurveWithKnotsEntity, BSplineCurve3D>
{
    public MappingResult<BSplineCurve3D> Map(StepBSplineCurveWithKnotsEntity source, MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);
        int checkpoint = context.CaptureDiagnosticCheckpoint();
        var points = new List<Point3D>();
        for (int index = 0; index < source.ControlPointReferences.Count; index++)
        {
            string reference = source.ControlPointReferences[index];
            var resolution = context.ReferenceResolver.Resolve<StepCartesianPointEntity>(reference);
            if (!resolution.IsResolved)
            {
                context.AddDiagnostics(resolution.Diagnostics);
                return MappingResult<BSplineCurve3D>.Failure(context.GetDiagnosticsSince(checkpoint));
            }
            var point = resolution.Value ?? throw new InvalidOperationException("Resolved point is missing.");
            if (point.Coordinates.Count != 3)
                return Fail(context, checkpoint, "MAP-STEP-BSPLINE-CURVE-001",
                    $"STEP B-spline curve '{source.SourceEntityId}' control point '{reference}' at index {index} must have three coordinates.");
            points.Add(new Point3D(point.Coordinates[0], point.Coordinates[1], point.Coordinates[2]));
        }
        BSplineCurve3D curve;
        try
        {
            curve = new BSplineCurve3D(checked((int)source.Degree), points,
                source.Multiplicities.Select(x => checked((int)x)), source.Knots);
        }
        catch (Exception exception) when (exception is ArgumentException or OverflowException)
        {
            return Fail(context, checkpoint, "MAP-STEP-BSPLINE-CURVE-002",
                $"STEP B-spline curve '{source.SourceEntityId}' cannot form a canonical curve: {exception.Message}");
        }
        var diagnostics = context.GetDiagnosticsSince(checkpoint);
        return diagnostics.Count == 0 ? MappingResult<BSplineCurve3D>.Success(curve)
            : MappingResult<BSplineCurve3D>.SuccessWithDiagnostics(curve, diagnostics);
    }

    private static MappingResult<BSplineCurve3D> Fail(MappingContext context, int checkpoint, string code, string message)
    {
        context.AddDiagnostic(new MappingDiagnostic(code, message, MappingDiagnosticSeverity.Error));
        return MappingResult<BSplineCurve3D>.Failure(context.GetDiagnosticsSince(checkpoint));
    }
}
