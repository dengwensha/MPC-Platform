using BenchmarkDotNet.Attributes;
using MPC.Connectors.Step.Readers;

namespace MPC.Benchmarks;

[MemoryDiagnoser]
public class StepDataParsingBenchmarks
{
    private StepDataEntityReader _dataReader =
        null!;

    private string _filePath =
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
        _filePath =
            StepBenchmarkSupport
                .GetGoldenStepFilePath(
                    FileName);

        _dataReader =
            StepBenchmarkSupport
                .CreateDataReader();

        StepBenchmarkSupport.ReadInventory(
            _dataReader,
            _filePath);
    }

    [Benchmark]
    public object ParseDataInventory()
    {
        return _dataReader.Read(
            _filePath);
    }
}