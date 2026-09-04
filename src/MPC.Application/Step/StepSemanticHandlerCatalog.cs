using MPC.Connectors.Step.Semantics;

namespace MPC.Application.Step;

internal static class StepSemanticHandlerCatalog
{
    public static IReadOnlyList<IStepSemanticEntityHandler> Create()
    {
        IStepSemanticEntityHandler[] handlers =
        [
            new StepAdvancedFaceEntityHandler(),
            new StepAxis2Placement3DEntityHandler(),
            new StepCartesianPointEntityHandler(),
            new StepCircleEntityHandler(),
            new StepClosedShellEntityHandler(),
            new StepConicalSurfaceEntityHandler(),
            new StepCylindricalSurfaceEntityHandler(),
            new StepDegenerateToroidalSurfaceEntityHandler(),
            new StepDirectionEntityHandler(),
            new StepEdgeCurveEntityHandler(),
            new StepEdgeLoopEntityHandler(),
            new StepFaceBoundEntityHandler(),
            new StepFaceOuterBoundEntityHandler(),
            new StepLineEntityHandler(),
            new StepManifoldSolidBrepEntityHandler(),
            new StepOrientedEdgeEntityHandler(),
            new StepPlaneEntityHandler(),
            new StepSphericalSurfaceEntityHandler(),
            new StepToroidalSurfaceEntityHandler(),
            new StepVectorEntityHandler(),
            new StepVertexPointEntityHandler()
        ];

        return Array.AsReadOnly(handlers);
    }
}
