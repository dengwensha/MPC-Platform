namespace MPC.Tooling.Compatibility;

public enum CompatibilityCode
{
    Compatible = 0,
    InsertShapeMismatch = 1,
    SeatSizeMismatch = 2,
    InscribedCircleDiameterMismatch = 3,
    ThicknessMismatch = 4,
    IncludedAngleMismatch = 5,
    ClearanceAngleMismatch = 6,
    MountingStyleMismatch = 7,
    HandMismatch = 8
}

public sealed record CompatibilityResult
{
    public bool IsCompatible { get; }

    public CompatibilityCode Code { get; }

    private CompatibilityResult(bool isCompatible, CompatibilityCode code)
    {
        IsCompatible = isCompatible;
        Code = code;
    }

    public static CompatibilityResult Compatible { get; } =
        new(true, CompatibilityCode.Compatible);

    public static CompatibilityResult Incompatible(CompatibilityCode code)
    {
        if (code == CompatibilityCode.Compatible)
        {
            throw new ArgumentException(
                "An incompatible result requires a mismatch code.",
                nameof(code));
        }

        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "Compatibility code is not defined.");
        }

        return new CompatibilityResult(false, code);
    }
}

public sealed class InsertHolderCompatibilityEvaluator
{
    private const double LinearToleranceMillimeters = 0.001;
    private const double AngularToleranceDegrees = 0.001;

    public CompatibilityResult Evaluate(ToolHolder holder, CuttingInsert insert)
    {
        ArgumentNullException.ThrowIfNull(holder);
        ArgumentNullException.ThrowIfNull(insert);

        ToolInterface required = holder.RequiredInsertInterface;
        ToolInterface actual = insert.Interface;

        if (!StringComparer.Ordinal.Equals(
                required.InsertShapeCode,
                actual.InsertShapeCode))
        {
            return CompatibilityResult.Incompatible(
                CompatibilityCode.InsertShapeMismatch);
        }

        if (!StringComparer.Ordinal.Equals(
                required.SeatSizeCode,
                actual.SeatSizeCode))
        {
            return CompatibilityResult.Incompatible(
                CompatibilityCode.SeatSizeMismatch);
        }

        if (!AreEqual(
                required.InscribedCircleDiameterMillimeters,
                actual.InscribedCircleDiameterMillimeters,
                LinearToleranceMillimeters))
        {
            return CompatibilityResult.Incompatible(
                CompatibilityCode.InscribedCircleDiameterMismatch);
        }

        if (!AreEqual(
                required.ThicknessMillimeters,
                actual.ThicknessMillimeters,
                LinearToleranceMillimeters))
        {
            return CompatibilityResult.Incompatible(
                CompatibilityCode.ThicknessMismatch);
        }

        if (!AreEqual(
                required.IncludedAngleDegrees,
                actual.IncludedAngleDegrees,
                AngularToleranceDegrees))
        {
            return CompatibilityResult.Incompatible(
                CompatibilityCode.IncludedAngleMismatch);
        }

        if (!AreEqual(
                required.ClearanceAngleDegrees,
                actual.ClearanceAngleDegrees,
                AngularToleranceDegrees))
        {
            return CompatibilityResult.Incompatible(
                CompatibilityCode.ClearanceAngleMismatch);
        }

        if (required.MountingStyleCode != actual.MountingStyleCode)
        {
            return CompatibilityResult.Incompatible(
                CompatibilityCode.MountingStyleMismatch);
        }

        if (holder.Hand != ToolHand.Neutral &&
            insert.Hand != ToolHand.Neutral &&
            insert.Hand != holder.Hand)
        {
            return CompatibilityResult.Incompatible(
                CompatibilityCode.HandMismatch);
        }

        return CompatibilityResult.Compatible;
    }

    private static bool AreEqual(double expected, double actual, double tolerance) =>
        Math.Abs(expected - actual) <= tolerance;
}
