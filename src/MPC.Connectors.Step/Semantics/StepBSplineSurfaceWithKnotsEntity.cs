namespace MPC.Connectors.Step.Semantics;

public sealed class StepBSplineSurfaceWithKnotsEntity
    : StepSemanticEntity
{
    private static readonly IReadOnlySet<string>
        SupportedSurfaceForms =
            new HashSet<string>(
                StringComparer.Ordinal)
            {
                "PLANE_SURF",
                "CYLINDRICAL_SURF",
                "CONICAL_SURF",
                "SPHERICAL_SURF",
                "TOROIDAL_SURF",
                "SURF_OF_REVOLUTION",
                "RULED_SURF",
                "GENERALISED_CONE",
                "QUADRIC_SURF",
                "SURF_OF_LINEAR_EXTRUSION",
                "UNSPECIFIED"
            };

    private static readonly IReadOnlySet<string>
        SupportedKnotSpecifications =
            new HashSet<string>(
                StringComparer.Ordinal)
            {
                "UNIFORM_KNOTS",
                "QUASI_UNIFORM_KNOTS",
                "PIECEWISE_BEZIER_KNOTS",
                "UNSPECIFIED"
            };

    private readonly IReadOnlyList<
        IReadOnlyList<string>>
        _controlPointReferenceGrid;

    private readonly IReadOnlyList<long>
        _uMultiplicities;

    private readonly IReadOnlyList<long>
        _vMultiplicities;

    private readonly IReadOnlyList<double>
        _uKnots;

    private readonly IReadOnlyList<double>
        _vKnots;

    public string Name { get; }

    public long UDegree { get; }

    public long VDegree { get; }

    public IReadOnlyList<IReadOnlyList<string>>
        ControlPointReferenceGrid =>
            _controlPointReferenceGrid;

    public string SurfaceForm { get; }

    public StepLogicalValue UClosed { get; }

    public StepLogicalValue VClosed { get; }

    public StepLogicalValue SelfIntersect { get; }

    public IReadOnlyList<long> UMultiplicities =>
        _uMultiplicities;

    public IReadOnlyList<long> VMultiplicities =>
        _vMultiplicities;

    public IReadOnlyList<double> UKnots =>
        _uKnots;

    public IReadOnlyList<double> VKnots =>
        _vKnots;

    public string KnotSpecification { get; }

    public StepBSplineSurfaceWithKnotsEntity(
        string? sourceEntityId,
        string name,
        long uDegree,
        long vDegree,
        IEnumerable<IEnumerable<string>>
            controlPointReferenceGrid,
        string surfaceForm,
        StepLogicalValue uClosed,
        StepLogicalValue vClosed,
        StepLogicalValue selfIntersect,
        IEnumerable<long> uMultiplicities,
        IEnumerable<long> vMultiplicities,
        IEnumerable<double> uKnots,
        IEnumerable<double> vKnots,
        string knotSpecification)
        : base(
            sourceEntityId,
            "B_SPLINE_SURFACE_WITH_KNOTS")
    {
        ArgumentNullException.ThrowIfNull(
            name);

        if (uDegree < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(uDegree),
                uDegree,
                "U degree must be greater than zero.");
        }

        if (vDegree < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vDegree),
                vDegree,
                "V degree must be greater than zero.");
        }

        ValidateEnumeration(
            surfaceForm,
            SupportedSurfaceForms,
            nameof(surfaceForm),
            "surface form");

        ValidateLogicalValue(
            uClosed,
            nameof(uClosed));

        ValidateLogicalValue(
            vClosed,
            nameof(vClosed));

        ValidateLogicalValue(
            selfIntersect,
            nameof(selfIntersect));

        ValidateEnumeration(
            knotSpecification,
            SupportedKnotSpecifications,
            nameof(knotSpecification),
            "knot specification");

        var copiedControlPointReferenceGrid =
            CopyControlPointReferenceGrid(
                controlPointReferenceGrid);

        int uControlPointCount =
            copiedControlPointReferenceGrid.Count;

        int vControlPointCount =
            copiedControlPointReferenceGrid[0].Count;

        if (uControlPointCount <= uDegree)
        {
            throw new ArgumentException(
                "The control-point grid must contain at least U degree plus one rows.",
                nameof(controlPointReferenceGrid));
        }

        if (vControlPointCount <= vDegree)
        {
            throw new ArgumentException(
                "The control-point grid must contain at least V degree plus one columns.",
                nameof(controlPointReferenceGrid));
        }

        var copiedUMultiplicities =
            CopyMultiplicities(
                uMultiplicities,
                nameof(uMultiplicities));

        var copiedVMultiplicities =
            CopyMultiplicities(
                vMultiplicities,
                nameof(vMultiplicities));

        var copiedUKnots =
            CopyKnots(
                uKnots,
                nameof(uKnots));

        var copiedVKnots =
            CopyKnots(
                vKnots,
                nameof(vKnots));

        ValidateKnotDirection(
            degree:
                uDegree,
            controlPointCount:
                uControlPointCount,
            multiplicities:
                copiedUMultiplicities,
            knots:
                copiedUKnots,
            multiplicityParameterName:
                nameof(uMultiplicities),
            directionName:
                "U");

        ValidateKnotDirection(
            degree:
                vDegree,
            controlPointCount:
                vControlPointCount,
            multiplicities:
                copiedVMultiplicities,
            knots:
                copiedVKnots,
            multiplicityParameterName:
                nameof(vMultiplicities),
            directionName:
                "V");

        Name =
            name;

        UDegree =
            uDegree;

        VDegree =
            vDegree;

        _controlPointReferenceGrid =
            copiedControlPointReferenceGrid;

        SurfaceForm =
            surfaceForm;

        UClosed =
            uClosed;

        VClosed =
            vClosed;

        SelfIntersect =
            selfIntersect;

        _uMultiplicities =
            copiedUMultiplicities;

        _vMultiplicities =
            copiedVMultiplicities;

        _uKnots =
            copiedUKnots;

        _vKnots =
            copiedVKnots;

        KnotSpecification =
            knotSpecification;
    }

    private static IReadOnlyList<IReadOnlyList<string>>
        CopyControlPointReferenceGrid(
            IEnumerable<IEnumerable<string>>
                controlPointReferenceGrid)
    {
        ArgumentNullException.ThrowIfNull(
            controlPointReferenceGrid);

        var copiedRows =
            new List<IReadOnlyList<string>>();

        int? expectedColumnCount =
            null;

        foreach (var sourceRow in
                 controlPointReferenceGrid)
        {
            if (sourceRow is null)
            {
                throw new ArgumentException(
                    "The control-point grid cannot contain a null row.",
                    nameof(controlPointReferenceGrid));
            }

            var copiedRow =
                sourceRow.ToList();

            if (copiedRow.Count == 0)
            {
                throw new ArgumentException(
                    "The control-point grid cannot contain an empty row.",
                    nameof(controlPointReferenceGrid));
            }

            if (
                expectedColumnCount.HasValue &&
                copiedRow.Count !=
                expectedColumnCount.Value)
            {
                throw new ArgumentException(
                    "The control-point grid must be rectangular.",
                    nameof(controlPointReferenceGrid));
            }

            foreach (string referenceId in
                     copiedRow)
            {
                if (!IsValidEntityReferenceId(
                        referenceId))
                {
                    throw new ArgumentException(
                        "Every control point must be a valid STEP entity reference ID.",
                        nameof(controlPointReferenceGrid));
                }
            }

            expectedColumnCount ??=
                copiedRow.Count;

            copiedRows.Add(
                Array.AsReadOnly(
                    copiedRow.ToArray()));
        }

        if (copiedRows.Count == 0)
        {
            throw new ArgumentException(
                "The control-point grid cannot be empty.",
                nameof(controlPointReferenceGrid));
        }

        return Array.AsReadOnly(
            copiedRows.ToArray());
    }

    private static IReadOnlyList<long>
        CopyMultiplicities(
            IEnumerable<long> multiplicities,
            string parameterName)
    {
        ArgumentNullException.ThrowIfNull(
            multiplicities,
            parameterName);

        var copiedValues =
            multiplicities.ToList();

        if (copiedValues.Count < 2)
        {
            throw new ArgumentException(
                "Knot multiplicities must contain at least two entries.",
                parameterName);
        }

        if (copiedValues.Any(
                value =>
                    value <= 0))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Knot multiplicities must be greater than zero.");
        }

        return Array.AsReadOnly(
            copiedValues.ToArray());
    }

    private static IReadOnlyList<double>
        CopyKnots(
            IEnumerable<double> knots,
            string parameterName)
    {
        ArgumentNullException.ThrowIfNull(
            knots,
            parameterName);

        var copiedValues =
            knots.ToList();

        if (copiedValues.Count < 2)
        {
            throw new ArgumentException(
                "A knot sequence must contain at least two distinct values.",
                parameterName);
        }

        if (copiedValues.Any(
                value =>
                    !double.IsFinite(value)))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Knot values must be finite.");
        }

        for (var index = 1;
             index < copiedValues.Count;
             index++)
        {
            if (
                copiedValues[index] <=
                copiedValues[index - 1])
            {
                throw new ArgumentException(
                    "Distinct knot values must be strictly increasing and remain in source order.",
                    parameterName);
            }
        }

        return Array.AsReadOnly(
            copiedValues.ToArray());
    }

    private static void ValidateKnotDirection(
        long degree,
        int controlPointCount,
        IReadOnlyList<long> multiplicities,
        IReadOnlyList<double> knots,
        string multiplicityParameterName,
        string directionName)
    {
        if (multiplicities.Count !=
            knots.Count)
        {
            throw new ArgumentException(
                $"{directionName} knot and multiplicity sequences must contain the same number of entries.",
                multiplicityParameterName);
        }

        decimal multiplicitySum =
            multiplicities.Aggregate(
                0m,
                (sum, value) =>
                    sum + value);

        decimal expectedMultiplicitySum =
            (decimal)controlPointCount +
            degree +
            1m;

        if (multiplicitySum !=
            expectedMultiplicitySum)
        {
            throw new ArgumentException(
                $"{directionName} multiplicities must sum to the control-point count plus degree plus one.",
                multiplicityParameterName);
        }

        for (var index = 0;
             index < multiplicities.Count;
             index++)
        {
            bool isEndpoint =
                index == 0 ||
                index ==
                multiplicities.Count - 1;

            long maximumMultiplicity =
                isEndpoint
                    ? degree + 1
                    : degree;

            if (
                multiplicities[index] >
                maximumMultiplicity)
            {
                string location =
                    isEndpoint
                        ? "first or last"
                        : "interior";

                throw new ArgumentOutOfRangeException(
                    multiplicityParameterName,
                    multiplicities[index],
                    $"{directionName} {location} knot multiplicity exceeds the permitted degree-based maximum.");
            }
        }
    }

    private static void ValidateEnumeration(
        string value,
        IReadOnlySet<string> supportedValues,
        string parameterName,
        string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"STEP {displayName} cannot be null, empty, or whitespace.",
                parameterName);
        }

        if (!supportedValues.Contains(
                value))
        {
            throw new ArgumentException(
                $"Unsupported STEP {displayName} '{value}'.",
                parameterName);
        }
    }

    private static void ValidateLogicalValue(
        StepLogicalValue value,
        string parameterName)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "STEP logical value must be False, True, or Unknown.");
        }
    }

    private static bool IsValidEntityReferenceId(
        string? value)
    {
        if (
            string.IsNullOrWhiteSpace(value) ||
            value.Length < 2 ||
            value[0] != '#')
        {
            return false;
        }

        for (var index = 1;
             index < value.Length;
             index++)
        {
            if (!char.IsDigit(
                    value[index]))
            {
                return false;
            }
        }

        return true;
    }
}
