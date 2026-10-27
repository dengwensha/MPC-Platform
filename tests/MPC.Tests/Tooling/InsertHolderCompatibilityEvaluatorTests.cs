using MPC.Tooling;
using MPC.Tooling.Compatibility;

namespace MPC.Tests.Tooling;

public sealed class InsertHolderCompatibilityEvaluatorTests
{
    private readonly InsertHolderCompatibilityEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_SyntheticReferencePair_ReturnsCompatible()
    {
        ToolHolder holder = CreateSyntheticHolder();
        CuttingInsert insert = CreateSyntheticInsert();

        CompatibilityResult result = _evaluator.Evaluate(holder, insert);

        Assert.True(result.IsCompatible);
        Assert.Equal(CompatibilityCode.Compatible, result.Code);
    }

    [Fact]
    public void Evaluate_DifferentInsertShape_ReturnsDeterministicMismatch()
    {
        ToolHolder holder = CreateSyntheticHolder();
        CuttingInsert insert = CreateSyntheticInsert(
            new ToolInterface("D", "19", 19.05, 6.35, 80, 0, 2));

        CompatibilityResult result = _evaluator.Evaluate(holder, insert);

        Assert.False(result.IsCompatible);
        Assert.Equal(CompatibilityCode.InsertShapeMismatch, result.Code);
    }

    [Fact]
    public void Evaluate_DifferentThickness_ReturnsDeterministicMismatch()
    {
        ToolHolder holder = CreateSyntheticHolder();
        CuttingInsert insert = CreateSyntheticInsert(
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

    // TOOL-01A: pin all remaining mismatch codes without changing the
    // production matcher or claiming real-world machine compatibility.
    [Theory]
    [InlineData("D", "19", 19.05, 6.35, 80, 0, 2, CompatibilityCode.InsertShapeMismatch)]
    [InlineData("C", "16", 19.05, 6.35, 80, 0, 2, CompatibilityCode.SeatSizeMismatch)]
    [InlineData("C", "19", 18.00, 6.35, 80, 0, 2, CompatibilityCode.InscribedCircleDiameterMismatch)]
    [InlineData("C", "19", 19.05, 7.35, 80, 0, 2, CompatibilityCode.ThicknessMismatch)]
    [InlineData("C", "19", 19.05, 6.35, 55, 0, 2, CompatibilityCode.IncludedAngleMismatch)]
    [InlineData("C", "19", 19.05, 6.35, 80, 7, 2, CompatibilityCode.ClearanceAngleMismatch)]
    [InlineData("C", "19", 19.05, 6.35, 80, 0, 3, CompatibilityCode.MountingStyleMismatch)]
    public void Evaluate_EachInterfaceMismatch_ReportsSpecificCode(
        string shape, string seat, double diameter, double thickness,
        double includedAngle, double clearanceAngle, int mount,
        CompatibilityCode expected)
    {
        var insert = CreateSyntheticInsert(new ToolInterface(shape,seat,diameter,
            thickness,includedAngle,clearanceAngle,mount));
        var result = _evaluator.Evaluate(CreateSyntheticHolder(),insert);
        Assert.False(result.IsCompatible);
        Assert.Equal(expected,result.Code);
    }

    [Fact]
    public void Evaluate_OpposingHands_RejectsButNeutralInsertIsPermitted()
    {
        var rightHolder = CreateSyntheticHolder();
        var neutralInsert = CreateSyntheticInsert();
        Assert.True(_evaluator.Evaluate(rightHolder,neutralInsert).IsCompatible);
        var leftInsert = new CuttingInsert(neutralInsert.Identity,
            neutralInsert.Interface,neutralInsert.Grade,
            neutralInsert.ChipBreakerDesignation,ToolHand.Left);
        Assert.Equal(CompatibilityCode.HandMismatch,
            _evaluator.Evaluate(rightHolder,leftInsert).Code);
    }

    [Fact]
    public void Evaluate_LinearTolerance_HasStrictBoundary()
    {
        var within = CreateSyntheticInsert(new ToolInterface("C","19",19.0505,
            6.35,80,0,2));
        var beyond = CreateSyntheticInsert(new ToolInterface("C","19",19.052,
            6.35,80,0,2));
        Assert.True(_evaluator.Evaluate(CreateSyntheticHolder(),within).IsCompatible);
        Assert.Equal(CompatibilityCode.InscribedCircleDiameterMismatch,
            _evaluator.Evaluate(CreateSyntheticHolder(),beyond).Code);
    }

    private static ToolHolder CreateSyntheticHolder() =>
        new(
            new ToolProductIdentity(
                "Sandvik Coromant",
                "5729638",
                "C8-DCLNR-55080-19",
                "11412575"),
            new ToolInterface("C", "19", 19.05, 6.35, 80, 0, 2),
            "CNMG 19 06 12",
            ToolHand.Right);

    private static CuttingInsert CreateSyntheticInsert(ToolInterface? @interface = null) =>
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
