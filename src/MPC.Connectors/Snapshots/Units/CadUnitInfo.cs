namespace MPC.Connectors.Snapshots.Units;

public sealed class CadUnitInfo
{
    public CadLengthUnit SourceLengthUnit { get; }

    public CadAngleUnit SourceAngleUnit { get; }

    public CadLengthUnit InternalLengthUnit =>
        CadLengthUnit.Metre;

    public CadAngleUnit InternalAngleUnit =>
        CadAngleUnit.Radian;

    public CadUnitInfo(
        CadLengthUnit sourceLengthUnit,
        CadAngleUnit sourceAngleUnit)
    {
        EnsureDefined(
            sourceLengthUnit,
            nameof(sourceLengthUnit));

        EnsureDefined(
            sourceAngleUnit,
            nameof(sourceAngleUnit));

        SourceLengthUnit = sourceLengthUnit;
        SourceAngleUnit = sourceAngleUnit;
    }

    private static void EnsureDefined<TEnum>(
        TEnum value,
        string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Unit value is not defined.");
        }
    }
}
