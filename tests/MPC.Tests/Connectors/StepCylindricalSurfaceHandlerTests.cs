using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepCylindricalSurfaceHandlerTests
{
    [Fact]
    public void Read_WithValidCylindricalSurfaceRecord_ReturnsSemanticCylindricalSurface()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#80",
                sourceEntityType:
                    "CYLINDRICAL_SURFACE",
                rawStatement:
                    "#80=CYLINDRICAL_SURFACE('Main Cylinder',#50,25.0);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepCylindricalSurfaceHandler()
                    });

        var result =
            reader.Read(
                record);

        var cylindricalSurface =
            Assert.IsType<StepCylindricalSurface>(
                result);

        Assert.Equal(
            "#80",
            cylindricalSurface.SourceEntityId);

        Assert.Equal(
            "CYLINDRICAL_SURFACE",
            cylindricalSurface.SourceEntityType);

        Assert.Equal(
            "Main Cylinder",
            cylindricalSurface.Name);

        Assert.Equal(
            "#50",
            cylindricalSurface.PositionReferenceId);

        Assert.Equal(
            25.0,
            cylindricalSurface.Radius,
            precision:
                10);
    }
}