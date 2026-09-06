using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepBSplineSurfaceWithKnotsConstraintTests
{
    [Fact]
    public void Constructor_EndpointMultiplicityAboveDegreePlusOne_IsRejected()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    CreateEntity(
                        uMultiplicities:
                            new long[] { 5, 3 }));

        Assert.Equal(
            "uMultiplicities",
            exception.ParamName);

        Assert.Equal(
            5L,
            exception.ActualValue);
    }

    [Fact]
    public void Constructor_InteriorMultiplicityAboveDegree_IsRejected()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    CreateEntity(
                        controlPointReferenceGrid:
                            CreateGrid(
                                rows:
                                    7,
                                columns:
                                    2),
                        uMultiplicities:
                            new long[] { 4, 4, 1, 1, 1 },
                        uKnots:
                            new[]
                            {
                                0.0,
                                0.25,
                                0.5,
                                0.75,
                                1.0
                            }));

        Assert.Equal(
            "uMultiplicities",
            exception.ParamName);

        Assert.Equal(
            4L,
            exception.ActualValue);
    }

    [Fact]
    public void Constructor_VDirectionInteriorMultiplicityAboveDegree_IsRejected()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    CreateEntity(
                        controlPointReferenceGrid:
                            CreateGrid(
                                rows:
                                    4,
                                columns:
                                    3),
                        vMultiplicities:
                            new long[] { 2, 2, 1 },
                        vKnots:
                            new[]
                            {
                                0.0,
                                0.5,
                                1.0
                            }));

        Assert.Equal(
            "vMultiplicities",
            exception.ParamName);

        Assert.Equal(
            2L,
            exception.ActualValue);
    }

    [Theory]
    [InlineData("PLANE_SURF")]
    [InlineData("CYLINDRICAL_SURF")]
    [InlineData("CONICAL_SURF")]
    [InlineData("SPHERICAL_SURF")]
    [InlineData("TOROIDAL_SURF")]
    [InlineData("SURF_OF_REVOLUTION")]
    [InlineData("RULED_SURF")]
    [InlineData("GENERALISED_CONE")]
    [InlineData("QUADRIC_SURF")]
    [InlineData("SURF_OF_LINEAR_EXTRUSION")]
    [InlineData("UNSPECIFIED")]
    public void Constructor_StandardSurfaceForm_IsPreserved(
        string surfaceForm)
    {
        var entity =
            CreateEntity(
                surfaceForm:
                    surfaceForm);

        Assert.Equal(
            surfaceForm,
            entity.SurfaceForm);
    }

    [Theory]
    [InlineData("UNIFORM_KNOTS")]
    [InlineData("QUASI_UNIFORM_KNOTS")]
    [InlineData("PIECEWISE_BEZIER_KNOTS")]
    [InlineData("UNSPECIFIED")]
    public void Constructor_StandardKnotSpecification_IsPreserved(
        string knotSpecification)
    {
        var entity =
            CreateEntity(
                knotSpecification:
                    knotSpecification);

        Assert.Equal(
            knotSpecification,
            entity.KnotSpecification);
    }

    private static StepBSplineSurfaceWithKnotsEntity
        CreateEntity(
            IEnumerable<IEnumerable<string>>?
                controlPointReferenceGrid = null,
            IEnumerable<long>? uMultiplicities = null,
            IEnumerable<long>? vMultiplicities = null,
            IEnumerable<double>? uKnots = null,
            IEnumerable<double>? vKnots = null,
            string surfaceForm = "UNSPECIFIED",
            string knotSpecification = "UNSPECIFIED")
    {
        return new StepBSplineSurfaceWithKnotsEntity(
            sourceEntityId:
                "#75",
            name:
                "NONE",
            uDegree:
                3,
            vDegree:
                1,
            controlPointReferenceGrid:
                controlPointReferenceGrid ??
                CreateGrid(
                    rows:
                        4,
                    columns:
                        2),
            surfaceForm:
                surfaceForm,
            uClosed:
                StepLogicalValue.False,
            vClosed:
                StepLogicalValue.False,
            selfIntersect:
                StepLogicalValue.False,
            uMultiplicities:
                uMultiplicities ??
                new long[] { 4, 4 },
            vMultiplicities:
                vMultiplicities ??
                new long[] { 2, 2 },
            uKnots:
                uKnots ??
                new[] { 0.0, 1.0 },
            vKnots:
                vKnots ??
                new[] { 0.0, 1.0 },
            knotSpecification:
                knotSpecification);
    }

    private static IReadOnlyList<IReadOnlyList<string>>
        CreateGrid(
            int rows,
            int columns)
    {
        var result =
            new List<IReadOnlyList<string>>();

        var referenceNumber =
            1;

        for (var rowIndex = 0;
             rowIndex < rows;
             rowIndex++)
        {
            var row =
                new List<string>();

            for (var columnIndex = 0;
                 columnIndex < columns;
                 columnIndex++)
            {
                row.Add(
                    $"#{referenceNumber}");

                referenceNumber++;
            }

            result.Add(
                row.AsReadOnly());
        }

        return result.AsReadOnly();
    }
}
