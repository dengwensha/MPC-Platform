```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i5-7200U CPU 2.50GHz (Max: 2.71GHz) (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET SDK 8.0.422
  [Host]     : .NET 8.0.28 (8.0.28, 8.0.2826.26413), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 8.0.28 (8.0.28, 8.0.2826.26413), X64 RyuJIT x86-64-v3
  Job-CNUJVU : .NET 8.0.28 (8.0.28, 8.0.2826.26413), X64 RyuJIT x86-64-v3


```
| Method                    | Job        | InvocationCount | UnrollFactor | Mean        | Error     | StdDev     | Median      | Gen0     | Gen1     | Allocated  |
|-------------------------- |----------- |---------------- |------------- |------------:|----------:|-----------:|------------:|---------:|---------:|-----------:|
| ReadSemanticEntities      | DefaultJob | Default         | 16           |   254.20 μs |  4.991 μs |   5.747 μs |   252.05 μs | 153.8086 |        - |  235.98 KB |
| ImportEndToEnd            | DefaultJob | Default         | 16           | 1,353.44 μs | 24.219 μs |  23.786 μs | 1,344.30 μs | 464.8438 | 140.6250 | 1028.34 KB |
| MapCanonicalSolid         | Job-CNUJVU | 1               | 1            |   371.50 μs | 38.174 μs | 107.671 μs |   341.55 μs |        - |        - |   44.87 KB |
| AssembleCanonicalTopology | Job-CNUJVU | 1               | 1            |    94.21 μs |  4.678 μs |  12.962 μs |    92.20 μs |        - |        - |   33.51 KB |
