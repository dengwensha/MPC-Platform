namespace MPC.Tooling;

public sealed record ToolInterface
{
    public string InsertShapeCode { get; }

    public string SeatSizeCode { get; }

    public double InscribedCircleDiameterMillimeters { get; }

    public double ThicknessMillimeters { get; }

    public double IncludedAngleDegrees { get; }

    public double ClearanceAngleDegrees { get; }

    public int MountingStyleCode { get; }

    public ToolInterface(
        string insertShapeCode,
        string seatSizeCode,
        double inscribedCircleDiameterMillimeters,
        double thicknessMillimeters,
        double includedAngleDegrees,
        double clearanceAngleDegrees,
        int mountingStyleCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(insertShapeCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(seatSizeCode);

        EnsurePositiveFinite(
            inscribedCircleDiameterMillimeters,
            nameof(inscribedCircleDiameterMillimeters));
        EnsurePositiveFinite(thicknessMillimeters, nameof(thicknessMillimeters));

        if (!double.IsFinite(includedAngleDegrees) ||
            includedAngleDegrees <= 0 ||
            includedAngleDegrees >= 180)
        {
            throw new ArgumentOutOfRangeException(
                nameof(includedAngleDegrees),
                includedAngleDegrees,
                "Included angle must be finite and between 0 and 180 degrees.");
        }

        if (!double.IsFinite(clearanceAngleDegrees) ||
            clearanceAngleDegrees < 0 ||
            clearanceAngleDegrees >= 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(clearanceAngleDegrees),
                clearanceAngleDegrees,
                "Clearance angle must be finite and between 0 and 90 degrees.");
        }

        if (mountingStyleCode < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(mountingStyleCode),
                mountingStyleCode,
                "Mounting style code cannot be negative.");
        }

        InsertShapeCode = NormalizeCode(insertShapeCode);
        SeatSizeCode = NormalizeCode(seatSizeCode);
        InscribedCircleDiameterMillimeters = inscribedCircleDiameterMillimeters;
        ThicknessMillimeters = thicknessMillimeters;
        IncludedAngleDegrees = includedAngleDegrees;
        ClearanceAngleDegrees = clearanceAngleDegrees;
        MountingStyleCode = mountingStyleCode;
    }

    private static string NormalizeCode(string value) =>
        value.Trim().ToUpperInvariant();

    private static void EnsurePositiveFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Dimension must be a positive finite number.");
        }
    }
}
