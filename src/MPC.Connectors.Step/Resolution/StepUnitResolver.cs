using System.Text.RegularExpressions;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;
using MPC.Connectors.Snapshots.Units;
using MPC.Connectors.Step.Contracts;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Resolution;

public sealed class StepUnitResolver : IStepUnitResolver
{
    private static readonly Regex SiUnitRegex =
        new(
            @"SI_UNIT\s*\(\s*([^,]+)\s*,\s*([^)]+)\s*\)",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

    public CadConnectorResult<CadUnitInfo> Resolve(
        StepDataInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        var lengthUnit =
            ResolveLengthUnit(inventory);

        var angleUnit =
            ResolveAngleUnit(inventory);

        var diagnostics =
            new List<CadDiagnostic>();

        if (lengthUnit == CadLengthUnit.Unknown)
        {
            diagnostics.Add(
                new CadDiagnostic(
                    code: "STEP_LENGTH_UNIT_NOT_RESOLVED",
                    severity: CadDiagnosticSeverity.Warning,
                    message:
                        "STEP source length unit could not be resolved."));
        }

        if (angleUnit == CadAngleUnit.Unknown)
        {
            diagnostics.Add(
                new CadDiagnostic(
                    code: "STEP_ANGLE_UNIT_NOT_RESOLVED",
                    severity: CadDiagnosticSeverity.Warning,
                    message:
                        "STEP source plane angle unit could not be resolved."));
        }

        var unitInfo =
            new CadUnitInfo(
                sourceLengthUnit: lengthUnit,
                sourceAngleUnit: angleUnit);

        if (diagnostics.Count > 0)
        {
            return CadConnectorResult<CadUnitInfo>
                .SuccessWithDiagnostics(
                    unitInfo,
                    diagnostics);
        }

        return CadConnectorResult<CadUnitInfo>
            .Success(unitInfo);
    }

    private static CadLengthUnit ResolveLengthUnit(
        StepDataInventory inventory)
    {
        var candidates =
            inventory.Entities
                .Where(IsLengthUnitEntity)
                .Select(ParseSiUnit)
                .Where(unit => unit is not null)
                .Cast<StepSiUnit>()
                .Select(MapLengthUnit)
                .Where(unit =>
                    unit != CadLengthUnit.Unknown)
                .Distinct()
                .ToList();

        return candidates.Count == 1
            ? candidates[0]
            : CadLengthUnit.Unknown;
    }

    private static CadAngleUnit ResolveAngleUnit(
        StepDataInventory inventory)
    {
        var candidates =
            inventory.Entities
                .Where(IsPlaneAngleUnitEntity)
                .Select(ParseSiUnit)
                .Where(unit => unit is not null)
                .Cast<StepSiUnit>()
                .Select(MapAngleUnit)
                .Where(unit =>
                    unit != CadAngleUnit.Unknown)
                .Distinct()
                .ToList();

        return candidates.Count == 1
            ? candidates[0]
            : CadAngleUnit.Unknown;
    }

    private static bool IsLengthUnitEntity(
        StepDataEntityRecord entity)
    {
        return entity.IsComplexEntity &&
               entity.ComponentTypes.Contains(
                   "LENGTH_UNIT",
                   StringComparer.OrdinalIgnoreCase) &&
               entity.ComponentTypes.Contains(
                   "SI_UNIT",
                   StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsPlaneAngleUnitEntity(
        StepDataEntityRecord entity)
    {
        return entity.IsComplexEntity &&
               entity.ComponentTypes.Contains(
                   "PLANE_ANGLE_UNIT",
                   StringComparer.OrdinalIgnoreCase) &&
               entity.ComponentTypes.Contains(
                   "SI_UNIT",
                   StringComparer.OrdinalIgnoreCase);
    }

    private static StepSiUnit? ParseSiUnit(
        StepDataEntityRecord entity)
    {
        var match =
            SiUnitRegex.Match(
                entity.RawStatement);

        if (!match.Success)
        {
            return null;
        }

        return new StepSiUnit(
            Prefix: NormalizeStepToken(
                match.Groups[1].Value),
            UnitName: NormalizeStepToken(
                match.Groups[2].Value));
    }

    private static CadLengthUnit MapLengthUnit(
        StepSiUnit unit)
    {
        if (!unit.UnitName.Equals(
                "METRE",
                StringComparison.OrdinalIgnoreCase))
        {
            return CadLengthUnit.Unknown;
        }

        return unit.Prefix.ToUpperInvariant() switch
        {
            "$" => CadLengthUnit.Metre,
            "MILLI" => CadLengthUnit.Millimetre,
            "CENTI" => CadLengthUnit.Centimetre,
            _ => CadLengthUnit.Unknown
        };
    }

    private static CadAngleUnit MapAngleUnit(
        StepSiUnit unit)
    {
        return unit.UnitName.Equals(
            "RADIAN",
            StringComparison.OrdinalIgnoreCase)
                ? CadAngleUnit.Radian
                : CadAngleUnit.Unknown;
    }

    private static string NormalizeStepToken(
        string value)
    {
        return value
            .Trim()
            .Trim('.');
    }

    private sealed record StepSiUnit(
        string Prefix,
        string UnitName);
}