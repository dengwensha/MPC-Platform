using MPC.Application.Manufacturing;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Mapping.Step;

namespace MPC.Application.Tests.Manufacturing;

public sealed class PlanarFaceAngleGoldenTests
{
    [Fact]
    public void T001Prism_AllSharedPlanarEdgesHaveRightAngle()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        string? path = null;
        while (directory is not null && path is null)
        {
            string local = Path.Combine(directory.FullName,
                "TestData", "Step", "Golden", "T001_PRISM.step");
            string repository = Path.Combine(directory.FullName, "tests",
                "MPC.Tests", "TestData", "Step", "Golden", "T001_PRISM.step");
            if (File.Exists(local)) path = local;
            else if (File.Exists(repository)) path = repository;
            directory = directory.Parent;
        }
        Assert.NotNull(path);

        var data = new StepDataEntityReader(new StepDataInventoryValidator())
            .Read(path!);
        Assert.NotNull(data.Value);
        IStepSemanticEntityHandler[] handlers =
        [
            new StepCartesianPointEntityHandler(),
            new StepDirectionEntityHandler(),
            new StepVectorEntityHandler(),
            new StepLineEntityHandler(),
            new StepVertexPointEntityHandler(),
            new StepEdgeCurveEntityHandler(),
            new StepOrientedEdgeEntityHandler(),
            new StepEdgeLoopEntityHandler(),
            new StepFaceOuterBoundEntityHandler(),
            new StepAdvancedFaceEntityHandler(),
            new StepPlaneEntityHandler(),
            new StepAxis2Placement3DEntityHandler(),
            new StepClosedShellEntityHandler(),
            new StepManifoldSolidBrepEntityHandler()
        ];

        var reader = new StepSemanticEntityReader(new StepParameterParser(), handlers);
        StepSemanticEntity[] entities = data.Value!.Entities
            .Select(x => reader.Read(x))
            .OfType<StepSemanticEntity>()
            .ToArray();
        var root = Assert.Single(entities.OfType<StepManifoldSolidBrepEntity>());
        var operation = StepCanonicalMappingOperation.Create(
            new StepSemanticEntityResolver(entities));
        var mapped = operation.Map(root);
        Assert.True(mapped.IsSuccessful);
        Assert.NotNull(mapped.Value);
        var faces = mapped.Value!.OuterShell.Faces;
        Assert.Equal(6, faces.Count);

        int signedWitnesses = 0;
        foreach (var face in faces)
        {
            var query = FaceAdjacencyQuery.Analyze(faces, face);
            Assert.Equal(4, query.UniqueNeighbors.Count);
            foreach (var neighbor in query.UniqueNeighbors)
            {
                Assert.True(PlanarFaceAngleQuery.TryCalculate(
                    faces, face, neighbor, out double angle));
                Assert.InRange(angle, 90.0 - 1e-8, 90.0 + 1e-8);
                // Independent prism oracle: a right-angle solid's neighboring
                // support planes rotate by 90 degrees. The sign depends on
                // verified winding and is NOT a material-side classification.
                var orientation = SharedEdgeOrientationQuery.Evaluate(faces, face, neighbor);
                if (orientation == SharedEdgeOrientationStatus.Opposite)
                {
                    Assert.True(SignedLocalDihedralQuery.TryCalculate(
                        faces, face, neighbor, out double signed));
                    Assert.InRange(Math.Abs(signed), 90.0 - 1e-8, 90.0 + 1e-8);
                    Assert.True(SignedLocalDihedralQuery.TryCalculate(
                        faces, neighbor, face, out double reversed));
                    Assert.InRange(Math.Abs(signed - reversed), 0, 1e-8);
                    signedWitnesses++;
                }
            }
        }
        Assert.Equal(24, signedWitnesses); // 12 undirected prism edges, each visited twice.

        // GE03B-04A: independent material-side witness for the pinned T001
        // rectangular solid. Its interior (0,0,10) is known from the STEP
        // corner coordinates, NOT derived from canonical normals or dihedrals.
        // For each supporting plane, an outward normal must point away from
        // this strictly interior reference point.
        var interior = new MPC.Geometry.Primitives.Point3D(0, 0, 10);
        int provenOutward = 0;
        foreach (var face in faces)
        {
            var plane = Assert.IsType<MPC.Geometry.Primitives.Plane3D>(face.Surface);
            double nx = plane.Normal.X, ny = plane.Normal.Y, nz = plane.Normal.Z;
            if (face.Orientation == MPC.Topology.FaceOrientation.Reversed)
            {
                nx = -nx; ny = -ny; nz = -nz;
            }
            bool IsOutward(double x, double y, double z)
            {
                double distance = x * (plane.Origin.X - interior.X)
                    + y * (plane.Origin.Y - interior.Y)
                    + z * (plane.Origin.Z - interior.Z);
                return double.IsFinite(distance) && distance > 1e-8;
            }
            Assert.True(IsOutward(nx, ny, nz),
                "Canonical plane normal does not point out of T001 material.");
            // Exercise the SAME oracle with deliberately inverted input.
            // It must reject the inward normal rather than merely restating
            // the positive signed-distance inequality.
            Assert.False(IsOutward(-nx, -ny, -nz));
            provenOutward++;
        }
        Assert.Equal(6, provenOutward);
        // This proves T001 only, not R01/R02 or arbitrary open/curved shells.

        // GE03B-04B-04: link each actual source ADVANCED_FACE to the sealed
        // mapping-time canonical Face, then test independent box occupancy.
        // Reference box dimensions are pinned from T001 fixture coordinates:
        // X [-50,50], Y [-40,40], Z [0,20]. Not inferred from face normals.
        var sourceFaces = entities.OfType<StepAdvancedFaceEntity>().ToArray();
        Assert.Equal(6, sourceFaces.Length);
        var seen = new HashSet<MPC.Topology.Primitives.Face>(
            System.Collections.Generic.ReferenceEqualityComparer.Instance);
        foreach (var source in sourceFaces)
        {
            Assert.True(operation.FaceIdentities.TryGetFace(
                source.SourceEntityId!, out var face));
            Assert.NotNull(face);
            Assert.Contains(faces, candidate => ReferenceEquals(candidate, face));
            Assert.True(seen.Add(face!));
            var plane = Assert.IsType<MPC.Geometry.Primitives.Plane3D>(face!.Surface);
            double nx = plane.Normal.X, ny = plane.Normal.Y, nz = plane.Normal.Z;
            if (face.Orientation == MPC.Topology.FaceOrientation.Reversed)
            { nx = -nx; ny = -ny; nz = -nz; }

            // Select face-center sample on one of the six known box planes.
            double x=0, y=0, z=10;
            if (Math.Abs(plane.Normal.X) > 0.9) x = plane.Origin.X;
            else if (Math.Abs(plane.Normal.Y) > 0.9) y = plane.Origin.Y;
            else if (Math.Abs(plane.Normal.Z) > 0.9) z = plane.Origin.Z;
            else throw new InvalidOperationException("Unexpected T001 surface normal.");

            bool Material(double px,double py,double pz) =>
                px > -50 + 1e-8 && px < 50 - 1e-8 &&
                py > -40 + 1e-8 && py < 40 - 1e-8 &&
                pz > 1e-8 && pz < 20 - 1e-8;
            bool Outward(double dx,double dy,double dz) =>
                !Material(x + 0.1*dx, y + 0.1*dy, z + 0.1*dz) &&
                 Material(x - 0.1*dx, y - 0.1*dy, z - 0.1*dz);
            Assert.True(Outward(nx,ny,nz),
                $"Source {source.SourceEntityId} material-side mismatch.");
            Assert.False(Outward(-nx,-ny,-nz));
        }
        Assert.Equal(faces.Count,seen.Count);
        // Independent synthetic fixture only: no R01/R02 material proof.

    }
}
