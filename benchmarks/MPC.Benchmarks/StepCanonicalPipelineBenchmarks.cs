using BenchmarkDotNet.Attributes;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Benchmarks;

[MemoryDiagnoser]
public class StepCanonicalPipelineBenchmarks
{
    private string _filePath =
        null!;

    private StepDataEntityReader _dataReader =
        null!;

    private StepDataInventory _inventory =
        null!;

    private StepSemanticEntityReader _semanticReader =
        null!;

    private List<StepSemanticEntity> _semanticEntities =
        null!;

    private StepManifoldSolidBrepEntity _manifoldSolidBrep =
        null!;

    private StepCanonicalMappingOperation _mappingOperation =
        null!;

    private Solid _solidForAssembly =
        null!;

    private readonly CanonicalTopologyAssembler _assembler =
        new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        _filePath =
            StepBenchmarkSupport.GetGoldenStepFilePath(
                "T001_PRISM.step");

        _dataReader =
            StepBenchmarkSupport.CreateDataReader();

        _inventory =
            StepBenchmarkSupport.ReadInventory(
                _dataReader,
                _filePath);

        _semanticReader =
            StepBenchmarkSupport.CreateSemanticReader();

        _semanticEntities =
            StepBenchmarkSupport.ReadSemanticEntities(
                _semanticReader,
                _inventory);

        _manifoldSolidBrep =
            _semanticEntities
                .OfType<StepManifoldSolidBrepEntity>()
                .Single();

        ValidateInput();

        PrepareMappingInvocation();
        PrepareAssemblyInvocation();
    }

    [Benchmark]
    public List<StepSemanticEntity> ReadSemanticEntities()
    {
        return StepBenchmarkSupport.ReadSemanticEntities(
            _semanticReader,
            _inventory);
    }

    [IterationSetup(
        Target = nameof(MapCanonicalSolid))]
    public void PrepareMappingInvocation()
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                _semanticEntities);

        _mappingOperation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);
    }

    [Benchmark]
    public MappingResult<Solid> MapCanonicalSolid()
    {
        return _mappingOperation.Map(
            _manifoldSolidBrep);
    }

    [IterationSetup(
        Target = nameof(AssembleCanonicalTopology))]
    public void PrepareAssemblyInvocation()
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                _semanticEntities);

        StepCanonicalMappingOperation operation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);

        MappingResult<Solid> result =
            operation.Map(
                _manifoldSolidBrep);

        _solidForAssembly =
            result.Value
            ?? throw new InvalidOperationException(
                "Canonical solid preparation failed.");
    }

    [Benchmark]
    public CanonicalTopologyAssemblyResult
        AssembleCanonicalTopology()
    {
        return _assembler.Assemble(
            _solidForAssembly);
    }

    [Benchmark]
    public CanonicalTopologyAssemblyResult
        ImportEndToEnd()
    {
        StepDataEntityReader dataReader =
            StepBenchmarkSupport.CreateDataReader();

        StepDataInventory inventory =
            StepBenchmarkSupport.ReadInventory(
                dataReader,
                _filePath);

        StepSemanticEntityReader semanticReader =
            StepBenchmarkSupport.CreateSemanticReader();

        List<StepSemanticEntity> semanticEntities =
            StepBenchmarkSupport.ReadSemanticEntities(
                semanticReader,
                inventory);

        StepManifoldSolidBrepEntity manifoldSolidBrep =
            semanticEntities
                .OfType<StepManifoldSolidBrepEntity>()
                .Single();

        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        StepCanonicalMappingOperation operation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);

        MappingResult<Solid> mappingResult =
            operation.Map(
                manifoldSolidBrep);

        Solid solid =
            mappingResult.Value
            ?? throw new InvalidOperationException(
                "End-to-end canonical mapping failed.");

        var assembler =
            new CanonicalTopologyAssembler();

        return assembler.Assemble(
            solid);
    }

    private void ValidateInput()
    {
        if (_inventory.TotalEntityCount != 187)
        {
            throw new InvalidOperationException(
                "T001_PRISM inventory count changed.");
        }

        if (_semanticEntities.Count != 154)
        {
            throw new InvalidOperationException(
                "T001_PRISM semantic entity count changed.");
        }

        var semanticResolver =
            new StepSemanticEntityResolver(
                _semanticEntities);

        StepCanonicalMappingOperation operation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);

        MappingResult<Solid> mappingResult =
            operation.Map(
                _manifoldSolidBrep);

        if (!mappingResult.IsSuccessful ||
            mappingResult.Value is null ||
            mappingResult.Diagnostics.Count != 0)
        {
            throw new InvalidOperationException(
                "T001_PRISM canonical mapping validation failed.");
        }

        CanonicalTopologyAssemblyResult assemblyResult =
            _assembler.Assemble(
                mappingResult.Value);

        if (assemblyResult.Topology.Faces.Count != 6 ||
            assemblyResult.Topology.Loops.Count != 6 ||
            assemblyResult.Topology.Edges.Count != 12 ||
            assemblyResult.Topology.Vertices.Count != 8)
        {
            throw new InvalidOperationException(
                "T001_PRISM canonical topology validation failed.");
        }
    }
}