using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepBSplineSurfaceWithKnotsEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "B_SPLINE_SURFACE_WITH_KNOTS";

    public StepSemanticEntity Read(
        StepDataEntityRecord record,
        IReadOnlyList<StepParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(
            record);

        ArgumentNullException.ThrowIfNull(
            parameters);

        if (!string.Equals(
                record.SourceEntityType,
                SupportedEntityType,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Handler for {SupportedEntityType} cannot process entity type '{record.SourceEntityType}'.");
        }

        if (parameters.Count != 13)
        {
            throw new FormatException(
                "B_SPLINE_SURFACE_WITH_KNOTS must contain exactly thirteen parameters.");
        }

        if (parameters[0] is not
            StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first B_SPLINE_SURFACE_WITH_KNOTS parameter, name, must be a STEP string.");
        }

        long uDegree =
            ReadInteger(
                parameters[1],
                "The second B_SPLINE_SURFACE_WITH_KNOTS parameter, U degree, must be a STEP integer.");

        long vDegree =
            ReadInteger(
                parameters[2],
                "The third B_SPLINE_SURFACE_WITH_KNOTS parameter, V degree, must be a STEP integer.");

        var controlPointReferenceGrid =
            ReadControlPointReferenceGrid(
                parameters[3]);

        string surfaceForm =
            ReadEnumeration(
                parameters[4],
                "The fifth B_SPLINE_SURFACE_WITH_KNOTS parameter, surface form, must be a STEP enumeration.");

        StepLogicalValue uClosed =
            ReadLogical(
                parameters[5],
                "U closed");

        StepLogicalValue vClosed =
            ReadLogical(
                parameters[6],
                "V closed");

        StepLogicalValue selfIntersect =
            ReadLogical(
                parameters[7],
                "self intersect");

        var uMultiplicities =
            ReadIntegerAggregate(
                parameters[8],
                "The ninth B_SPLINE_SURFACE_WITH_KNOTS parameter, U multiplicities, must be an aggregate of STEP integers.");

        var vMultiplicities =
            ReadIntegerAggregate(
                parameters[9],
                "The tenth B_SPLINE_SURFACE_WITH_KNOTS parameter, V multiplicities, must be an aggregate of STEP integers.");

        var uKnots =
            ReadNumericAggregate(
                parameters[10],
                "The eleventh B_SPLINE_SURFACE_WITH_KNOTS parameter, U knots, must be an aggregate of numeric STEP values.");

        var vKnots =
            ReadNumericAggregate(
                parameters[11],
                "The twelfth B_SPLINE_SURFACE_WITH_KNOTS parameter, V knots, must be an aggregate of numeric STEP values.");

        string knotSpecification =
            ReadEnumeration(
                parameters[12],
                "The thirteenth B_SPLINE_SURFACE_WITH_KNOTS parameter, knot specification, must be a STEP enumeration.");

        try
        {
            return new StepBSplineSurfaceWithKnotsEntity(
                record.SourceEntityId,
                nameParameter.Value,
                uDegree,
                vDegree,
                controlPointReferenceGrid,
                surfaceForm,
                uClosed,
                vClosed,
                selfIntersect,
                uMultiplicities,
                vMultiplicities,
                uKnots,
                vKnots,
                knotSpecification);
        }
        catch (ArgumentException exception)
            when (!string.IsNullOrWhiteSpace(
                exception.ParamName))
        {
            string rejectedValue =
                GetRejectedValue(
                    exception.ParamName!,
                    parameters);

            throw new StepSemanticValueException(
                record.SourceEntityId,
                SupportedEntityType,
                exception.ParamName!,
                rejectedValue,
                exception);
        }
    }

    private static IReadOnlyList<IReadOnlyList<string>>
        ReadControlPointReferenceGrid(
            StepParameter parameter)
    {
        if (parameter is not
            StepAggregateParameter gridParameter)
        {
            throw new FormatException(
                "The fourth B_SPLINE_SURFACE_WITH_KNOTS parameter, control points, must be a nested STEP aggregate.");
        }

        var rows =
            new List<IReadOnlyList<string>>();

        foreach (StepParameter rowParameter in
                 gridParameter.Items)
        {
            if (rowParameter is not
                StepAggregateParameter rowAggregate)
            {
                throw new FormatException(
                    "Every B_SPLINE_SURFACE_WITH_KNOTS control-point row must be a STEP aggregate.");
            }

            var references =
                new List<string>();

            foreach (StepParameter item in
                     rowAggregate.Items)
            {
                if (item is not
                    StepEntityReferenceParameter
                    referenceParameter)
                {
                    throw new FormatException(
                        "Every B_SPLINE_SURFACE_WITH_KNOTS control point must be a STEP entity reference.");
                }

                references.Add(
                    referenceParameter.EntityId);
            }

            rows.Add(
                references.AsReadOnly());
        }

        return rows.AsReadOnly();
    }

    private static IReadOnlyList<long>
        ReadIntegerAggregate(
            StepParameter parameter,
            string errorMessage)
    {
        if (parameter is not
            StepAggregateParameter aggregate)
        {
            throw new FormatException(
                errorMessage);
        }

        var values =
            new List<long>();

        foreach (StepParameter item in
                 aggregate.Items)
        {
            if (item is not
                StepIntegerParameter integerParameter)
            {
                throw new FormatException(
                    errorMessage);
            }

            values.Add(
                integerParameter.Value);
        }

        return values.AsReadOnly();
    }

    private static IReadOnlyList<double>
        ReadNumericAggregate(
            StepParameter parameter,
            string errorMessage)
    {
        if (parameter is not
            StepAggregateParameter aggregate)
        {
            throw new FormatException(
                errorMessage);
        }

        var values =
            new List<double>();

        foreach (StepParameter item in
                 aggregate.Items)
        {
            values.Add(
                ReadNumeric(
                    item,
                    errorMessage));
        }

        return values.AsReadOnly();
    }

    private static long ReadInteger(
        StepParameter parameter,
        string errorMessage)
    {
        return parameter is
            StepIntegerParameter integerParameter
            ? integerParameter.Value
            : throw new FormatException(
                errorMessage);
    }

    private static double ReadNumeric(
        StepParameter parameter,
        string errorMessage)
    {
        return parameter switch
        {
            StepRealParameter realParameter =>
                realParameter.Value,

            StepIntegerParameter integerParameter =>
                integerParameter.Value,

            _ =>
                throw new FormatException(
                    errorMessage)
        };
    }

    private static string ReadEnumeration(
        StepParameter parameter,
        string errorMessage)
    {
        return parameter is
            StepEnumerationParameter enumerationParameter
            ? enumerationParameter.Name
            : throw new FormatException(
                errorMessage);
    }

    private StepLogicalValue ReadLogical(
        StepParameter parameter,
        string parameterDisplayName)
    {
        if (parameter is not
            StepEnumerationParameter enumerationParameter)
        {
            throw new FormatException(
                $"{SupportedEntityType} {parameterDisplayName} must be a STEP logical enumeration.");
        }

        return enumerationParameter.Name switch
        {
            "F" =>
                StepLogicalValue.False,

            "T" =>
                StepLogicalValue.True,

            "U" =>
                StepLogicalValue.Unknown,

            _ =>
                throw new FormatException(
                    $"{SupportedEntityType} {parameterDisplayName} must be .F., .T., or .U., but '{enumerationParameter.RawText}' was provided.")
        };
    }

    private static string GetRejectedValue(
        string parameterName,
        IReadOnlyList<StepParameter> parameters)
    {
        return parameterName switch
        {
            "uDegree" =>
                parameters[1].RawText,

            "vDegree" =>
                parameters[2].RawText,

            "controlPointReferenceGrid" =>
                parameters[3].RawText,

            "surfaceForm" =>
                parameters[4].RawText,

            "uClosed" =>
                parameters[5].RawText,

            "vClosed" =>
                parameters[6].RawText,

            "selfIntersect" =>
                parameters[7].RawText,

            "uMultiplicities" =>
                parameters[8].RawText,

            "vMultiplicities" =>
                parameters[9].RawText,

            "uKnots" =>
                parameters[10].RawText,

            "vKnots" =>
                parameters[11].RawText,

            "knotSpecification" =>
                parameters[12].RawText,

            _ =>
                throw new InvalidOperationException(
                    $"No source parameter is registered for semantic parameter '{parameterName}'.")
        };
    }
}
