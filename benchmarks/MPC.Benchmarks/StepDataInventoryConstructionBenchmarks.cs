using BenchmarkDotNet.Attributes;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Readers;

namespace MPC.Benchmarks;

[MemoryDiagnoser]
public class StepDataInventoryConstructionBenchmarks
{
    private IReadOnlyList<StepDataEntityRecord> _entities =
        null!;

    [Params(
        "T001_PRISM.step",
        "T002_SPHERE.step",
        "T003_CYLINDER.step",
        "T101_BASIC_POCKET.step",
        "T103_MULTIPLE_ISLANDS.step")]
    public string FileName
    {
        get;
        set;
    } = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        string filePath =
            StepBenchmarkSupport
                .GetGoldenStepFilePath(
                    FileName);

        StepDataEntityReader dataReader =
            StepBenchmarkSupport
                .CreateDataReader();

        StepDataInventory inventory =
            StepBenchmarkSupport
                .ReadInventory(
                    dataReader,
                    filePath);

        _entities =
            inventory.Entities;
    }

    [Benchmark]
    public StepDataInventory ConstructInventory()
    {
        return new StepDataInventory(
            _entities);
    }
}