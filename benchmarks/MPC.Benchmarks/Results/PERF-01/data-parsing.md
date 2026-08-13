```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i5-7200U CPU 2.50GHz (Max: 2.71GHz) (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET SDK 8.0.422
  [Host]     : .NET 8.0.28 (8.0.28, 8.0.2826.26413), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 8.0.28 (8.0.28, 8.0.2826.26413), X64 RyuJIT x86-64-v3


```
| Method             | FileName             | Mean       | Error    | StdDev   | Gen0     | Gen1     | Gen2    | Allocated  |
|------------------- |--------------------- |-----------:|---------:|---------:|---------:|---------:|--------:|-----------:|
| **ParseDataInventory** | **T001_PRISM.step**      |   **821.6 μs** | **14.66 μs** | **11.45 μs** | **328.1250** |  **93.7500** |       **-** |  **700.61 KB** |
| **ParseDataInventory** | **T002_SPHERE.step**     |   **410.7 μs** |  **8.18 μs** | **11.20 μs** | **194.3359** |        **-** |       **-** |  **299.01 KB** |
| **ParseDataInventory** | **T003_CYLINDER.step**   |   **587.2 μs** | **11.46 μs** | **16.80 μs** | **277.3438** |  **48.8281** |       **-** |   **488.6 KB** |
| **ParseDataInventory** | **T101_(...).step [22]** | **1,307.5 μs** | **25.92 μs** | **37.99 μs** | **343.7500** | **195.3125** |       **-** | **1217.65 KB** |
| **ParseDataInventory** | **T103_(...).step [26]** | **2,963.1 μs** | **58.51 μs** | **71.85 μs** | **664.0625** | **335.9375** | **62.5000** | **2750.42 KB** |
