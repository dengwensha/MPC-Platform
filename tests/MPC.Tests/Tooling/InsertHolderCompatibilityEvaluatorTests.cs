using MPC.Tooling;
using MPC.Tooling.Compatibility;

namespace MPC.Tests.Tooling;

public sealed class InsertHolderCompatibilityEvaluatorTests
{
    private readonly InsertHolderCompatibilityEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_SandvikReferencePair_ReturnsCompatible()
    {
        ToolHolder holder = CreateSandvikHolder();
        CuttingInsert insert = CreateSandvikInsert();

        CompatibilityResult result = _evaluator.Evaluate(holder, insert);

        Assert.True(result.IsCompatible);
        Assert.Equal(CompatibilityCode.Compatible, result.Code);
    }

    [Fact]
    public void Evaluate_DifferentInsertShape_ReturnsDeterministicMismatch()
    {
        ToolHolder holder = CreateSandvikHolder();
        CuttingInsert insert = CreateSandvikInsert(
            new ToolInterface("D", "19", 19.05, 6.35, 80, 0, 2));

        CompatibilityResult result = _evaluator.Evaluate(holder, insert);

        Assert.False(result.IsCompatible);
        Assert.Equal(CompatibilityCode.InsertShapeMismatch, result.Code);
    }

    [Fact]
    public void Evaluate_DifferentThickness_ReturnsDeterministicMismatch()
    {
        ToolHolder holder = CreateSandvikHolder();
        CuttingInsert insert = CreateSandvikInsert(
            new ToolInterface("C", "19", 19.05, 7.35, 80, 0, 2));

        CompatibilityResult result = _evaluator.Evaluate(holder, insert);

        Assert.False(result.IsCompatible);
        Assert.Equal(CompatibilityCode.ThicknessMismatch, result.Code);
    }

    [Fact]
    public void Constructor_NonPositiveCriticalDimension_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ToolInterface("C", "19", 0, 6.35, 80, 0, 2));
    }

    private static ToolHolder CreateSandvikHolder() =>
        new(
            new ToolProductIdentity(
                "Sandvik Coromant",
                "5729638",
                "C8-DCLNR-55080-19",
                "11412575"),
            new ToolInterface("C", "19", 19.05, 6.35, 80, 0, 2),
            "CNMG 19 06 12",
            ToolHand.Right);

    private static CuttingInsert CreateSandvikInsert(ToolInterface? @interface = null) =>
        new(
            new ToolProductIdentity(
                "Sandvik Coromant",
                "7860702",
                "CNMG 19 06 12-PM 4425",
                "7323225046701"),
            @interface ?? new ToolInterface("C", "19", 19.05, 6.35, 80, 0, 2),
            new CuttingGrade("4425", "HC", "CVD", "TiCN+Al2O3+TiN"),
            "PM",
            ToolHand.Neutral);
}
