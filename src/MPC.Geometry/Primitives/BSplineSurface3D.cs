namespace MPC.Geometry.Primitives;

using MPC.Geometry.Contracts;

public sealed class BSplineSurface3D :
    ISurface3D,
    IEquatable<BSplineSurface3D>
{
    private readonly IReadOnlyList<
        IReadOnlyList<Point3D>>
        _controlPointGrid;

    private readonly IReadOnlyList<int>
        _uMultiplicities;

    private readonly IReadOnlyList<int>
        _vMultiplicities;

    private readonly IReadOnlyList<double>
        _uKnots;

    private readonly IReadOnlyList<double>
        _vKnots;

    public int UDegree { get; }

    public int VDegree { get; }

    public IReadOnlyList<IReadOnlyList<Point3D>>
        ControlPointGrid =>
            _controlPointGrid;

    public IReadOnlyList<int> UMultiplicities =>
        _uMultiplicities;

    public IReadOnlyList<int> VMultiplicities =>
        _vMultiplicities;

    public IReadOnlyList<double> UKnots =>
        _uKnots;

    public IReadOnlyList<double> VKnots =>
        _vKnots;

    public BSplineSurface3D(
        int uDegree,
        int vDegree,
        IEnumerable<IEnumerable<Point3D>>
            controlPointGrid,
        IEnumerable<int> uMultiplicities,
        IEnumerable<int> vMultiplicities,
        IEnumerable<double> uKnots,
        IEnumerable<double> vKnots)
    {
        ValidateDegree(
            uDegree,
            nameof(uDegree),
            "U");

        ValidateDegree(
            vDegree,
            nameof(vDegree),
            "V");

        var copiedControlPointGrid =
            CopyControlPointGrid(
                controlPointGrid);

        int uControlPointCount =
            copiedControlPointGrid.Count;

        int vControlPointCount =
            copiedControlPointGrid[0].Count;

        ValidateControlPointCount(
            uControlPointCount,
            uDegree,
            nameof(controlPointGrid),
            "U",
            "rows");

        ValidateControlPointCount(
            vControlPointCount,
            vDegree,
            nameof(controlPointGrid),
            "V",
            "columns");

        var copiedUMultiplicities =
            CopyMultiplicities(
                uMultiplicities,
                nameof(uMultiplicities),
                "U");

        var copiedVMultiplicities =
            CopyMultiplicities(
                vMultiplicities,
                nameof(vMultiplicities),
                "V");

        var copiedUKnots =
            CopyKnots(
                uKnots,
                nameof(uKnots),
                "U");

        var copiedVKnots =
            CopyKnots(
                vKnots,
                nameof(vKnots),
                "V");

        ValidateKnotDirection(
            uDegree,
            uControlPointCount,
            copiedUMultiplicities,
            copiedUKnots,
            nameof(uMultiplicities),
            nameof(uKnots),
            "U");

        ValidateKnotDirection(
            vDegree,
            vControlPointCount,
            copiedVMultiplicities,
            copiedVKnots,
            nameof(vMultiplicities),
            nameof(vKnots),
            "V");

        UDegree = uDegree;
        VDegree = vDegree;

        _controlPointGrid =
            copiedControlPointGrid;

        _uMultiplicities =
            copiedUMultiplicities;

        _vMultiplicities =
            copiedVMultiplicities;

        _uKnots =
            copiedUKnots;

        _vKnots =
            copiedVKnots;
    }

    public bool Equals(
        BSplineSurface3D? other)
    {
        if (ReferenceEquals(
                this,
                other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return
            UDegree == other.UDegree &&
            VDegree == other.VDegree &&
            ControlPointGridsEqual(
                _controlPointGrid,
                other._controlPointGrid) &&
            _uMultiplicities.SequenceEqual(
                other._uMultiplicities) &&
            _vMultiplicities.SequenceEqual(
                other._vMultiplicities) &&
            _uKnots.SequenceEqual(
                other._uKnots) &&
            _vKnots.SequenceEqual(
                other._vKnots);
    }

    public override bool Equals(
        object? obj)
    {
        return
            obj is BSplineSurface3D other &&
            Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(UDegree);
        hashCode.Add(VDegree);

        foreach (var row in _controlPointGrid)
        {
            foreach (var point in row)
            {
                hashCode.Add(point);
            }
        }

        foreach (
            int multiplicity in
            _uMultiplicities)
        {
            hashCode.Add(multiplicity);
        }

        foreach (
            int multiplicity in
            _vMultiplicities)
        {
            hashCode.Add(multiplicity);
        }

        foreach (double knot in _uKnots)
        {
            hashCode.Add(knot);
        }

        foreach (double knot in _vKnots)
        {
            hashCode.Add(knot);
        }

        return hashCode.ToHashCode();
    }

    public static bool operator ==(
        BSplineSurface3D? left,
        BSplineSurface3D? right)
    {
        return
            ReferenceEquals(left, right) ||
            left is not null &&
            left.Equals(right);
    }

    public static bool operator !=(
        BSplineSurface3D? left,
        BSplineSurface3D? right)
    {
        return !(left == right);
    }

    private static void ValidateDegree(
        int degree,
        string parameterName,
        string directionName)
    {
        if (degree < 1)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                degree,
                $"{directionName} degree must be greater than zero.");
        }
    }

    private static IReadOnlyList<
        IReadOnlyList<Point3D>>
        CopyControlPointGrid(
            IEnumerable<IEnumerable<Point3D>>
                controlPointGrid)
    {
        ArgumentNullException.ThrowIfNull(
            controlPointGrid);

        var copiedRows =
            new List<IReadOnlyList<Point3D>>();

        int? expectedColumnCount = null;

        foreach (
            var sourceRow in
            controlPointGrid)
        {
            if (sourceRow is null)
            {
                throw new ArgumentException(
                    "The control-point grid cannot contain a null row.",
                    nameof(controlPointGrid));
            }

            Point3D[] copiedRow =
                sourceRow.ToArray();

            if (copiedRow.Length == 0)
            {
                throw new ArgumentException(
                    "Every control-point row must contain at least one point.",
                    nameof(controlPointGrid));
            }

            if (
                expectedColumnCount.HasValue &&
                copiedRow.Length !=
                expectedColumnCount.Value)
            {
                throw new ArgumentException(
                    "The control-point grid must be rectangular.",
                    nameof(controlPointGrid));
            }

            for (
                var columnIndex = 0;
                columnIndex < copiedRow.Length;
                columnIndex++)
            {
                ValidateFinitePoint(
                    copiedRow[columnIndex],
                    nameof(controlPointGrid));
            }

            expectedColumnCount ??=
                copiedRow.Length;

            copiedRows.Add(
                Array.AsReadOnly(
                    copiedRow));
        }

        if (copiedRows.Count == 0)
        {
            throw new ArgumentException(
                "The control-point grid cannot be empty.",
                nameof(controlPointGrid));
        }

        return Array.AsReadOnly(
            copiedRows.ToArray());
    }

    private static void ValidateFinitePoint(
        Point3D point,
        string parameterName)
    {
        if (
            !double.IsFinite(point.X) ||
            !double.IsFinite(point.Y) ||
            !double.IsFinite(point.Z))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                point,
                "B-spline control-point coordinates must be finite.");
        }
    }

    private static void ValidateControlPointCount(
        int controlPointCount,
        int degree,
        string parameterName,
        string directionName,
        string dimensionName)
    {
        if (controlPointCount <= degree)
        {
            throw new ArgumentException(
                $"The control-point grid must contain at least {directionName} degree plus one {dimensionName}.",
                parameterName);
        }
    }

    private static IReadOnlyList<int>
        CopyMultiplicities(
            IEnumerable<int> multiplicities,
            string parameterName,
            string directionName)
    {
        ArgumentNullException.ThrowIfNull(
            multiplicities);

        int[] copiedMultiplicities =
            multiplicities.ToArray();

        if (copiedMultiplicities.Length < 2)
        {
            throw new ArgumentException(
                $"{directionName} multiplicities must contain at least two entries.",
                parameterName);
        }

        foreach (
            int multiplicity in
            copiedMultiplicities)
        {
            if (multiplicity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    multiplicity,
                    $"{directionName} multiplicities must be greater than zero.");
            }
        }

        return Array.AsReadOnly(
            copiedMultiplicities);
    }

    private static IReadOnlyList<double>
        CopyKnots(
            IEnumerable<double> knots,
            string parameterName,
            string directionName)
    {
        ArgumentNullException.ThrowIfNull(
            knots);

        double[] copiedKnots =
            knots.ToArray();

        if (copiedKnots.Length < 2)
        {
            throw new ArgumentException(
                $"{directionName} knots must contain at least two entries.",
                parameterName);
        }

        for (
            var index = 0;
            index < copiedKnots.Length;
            index++)
        {
            double knot =
                copiedKnots[index];

            if (!double.IsFinite(knot))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    knot,
                    $"{directionName} knot values must be finite.");
            }

            if (
                index > 0 &&
                knot <= copiedKnots[index - 1])
            {
                throw new ArgumentException(
                    $"{directionName} distinct knot values must be strictly increasing.",
                    parameterName);
            }
        }

        return Array.AsReadOnly(
            copiedKnots);
    }

    private static void ValidateKnotDirection(
        int degree,
        int controlPointCount,
        IReadOnlyList<int> multiplicities,
        IReadOnlyList<double> knots,
        string multiplicityParameterName,
        string knotParameterName,
        string directionName)
    {
        ValidateMultiplicityBounds(
            degree,
            multiplicities,
            multiplicityParameterName,
            directionName);

        if (multiplicities.Count != knots.Count)
        {
            throw new ArgumentException(
                $"{directionName} knot and multiplicity counts must match.",
                knotParameterName);
        }

        long multiplicitySum = 0L;

        foreach (
            int multiplicity in
            multiplicities)
        {
            multiplicitySum +=
                multiplicity;
        }

        long expectedMultiplicitySum =
            controlPointCount +
            (long)degree +
            1L;

        if (
            multiplicitySum !=
            expectedMultiplicitySum)
        {
            throw new ArgumentException(
                $"{directionName} multiplicities must sum to the control-point count plus degree plus one.",
                multiplicityParameterName);
        }
    }

    private static void ValidateMultiplicityBounds(
        int degree,
        IReadOnlyList<int> multiplicities,
        string parameterName,
        string directionName)
    {
        for (
            var index = 0;
            index < multiplicities.Count;
            index++)
        {
            bool isEndpoint =
                index == 0 ||
                index ==
                multiplicities.Count - 1;

            int maximumMultiplicity =
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
                    parameterName,
                    multiplicities[index],
                    $"{directionName} {location} knot multiplicity exceeds the permitted degree-based maximum.");
            }
        }
    }

    private static bool ControlPointGridsEqual(
        IReadOnlyList<IReadOnlyList<Point3D>>
            first,
        IReadOnlyList<IReadOnlyList<Point3D>>
            second)
    {
        if (first.Count != second.Count)
        {
            return false;
        }

        for (
            var rowIndex = 0;
            rowIndex < first.Count;
            rowIndex++)
        {
            if (
                !first[rowIndex].SequenceEqual(
                    second[rowIndex]))
            {
                return false;
            }
        }

        return true;
    }
}
