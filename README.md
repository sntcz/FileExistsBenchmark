# File.Exists alternatives benchmarks

Is it File.Exists slow or not? There are some answers:
* https://stackoverflow.com/questions/2225415/why-is-file-exists-much-slower-when-the-file-does-not-exist
* https://stackoverflow.com/questions/1707569/is-file-exists-an-expensive-operation

Possible implementations:
1. Traditional uses `File.Exists`
``` csharp
public bool File_Exists(string fileName) =>  File.Exists(fileName);
```
2. Using P/Invoke `PathFileExists` 
``` csharp
[System.Runtime.InteropServices.DllImport("shlwapi.dll", EntryPoint = "PathFileExistsW", SetLastError = true, CharSet = CharSet.Unicode)]
[return: MarshalAs(UnmanagedType.Bool)]
private static extern bool _PathFileExists([MarshalAs(UnmanagedType.LPWStr)]string pszPath);

public bool PathFileExists(string fileName) => _PathFileExists(fileName);
```
3. Using `FileInfo` class
``` csharp
public bool FileInfo(string fileName) => (new FileInfo(fileName)).Exists;
```

4. Using `FileInfo` class with `try ... catch` block
``` csharp
public bool FileInfo_TryCatch(string fileName)
{
    try
    {
        return (new FileInfo(fileName)).Exists;
    }
    catch (Exception)
    {
        return false;
    }
}
```
 
Parameters should cover possible cases
``` csharp
@"C:\TEMP\test.txt", // File exists
@"C:\TEMP\test.not", // File not exist
@"\\10.187.39.110\Temp\test.txt", // File exists
@"\\10.187.39.110\Temp\test.not", // File not exists
@"Z:\test.txt", // File exists, map Z: \\10.187.39.110\Temp
@"Z:\test.not", // File not exists, map Z: \\10.187.39.110\Temp
@"J:\test.not", // File not exists, network drive is inactive
```

Temporary folder on system drive C: (SSD drive) and network file with UNC or mapped drive letter.
Connection to network file uses VPN. 

Using [BenchmarkDotNet](https://benchmarkdotnet.org/index.html)
``` ini
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.657 (1909/November2018Update/19H2)
Intel Core i5-7200U CPU 2.50GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X86 LegacyJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X86 LegacyJIT
```


First run with connected VPN.

|            Method |             fileName |      Mean |    Error |   StdDev |    Median | Ratio | RatioSD |
|------------------ |--------------------- |----------:|---------:|---------:|----------:|------:|--------:|
|       **File_Exists** |     **C:\TEMP\test.not** |  **29.88 μs** | **0.401 μs** | **0.335 μs** |  **29.83 μs** |  **1.00** |    **0.00** |
|    PathFileExists |     C:\TEMP\test.not |  31.81 μs | 0.619 μs | 0.867 μs |  31.53 μs |  1.07 |    0.03 |
|          FileInfo |     C:\TEMP\test.not |  30.05 μs | 0.572 μs | 0.507 μs |  29.82 μs |  1.01 |    0.02 |
| FileInfo_TryCatch |     C:\TEMP\test.not |  30.25 μs | 0.603 μs | 0.645 μs |  29.96 μs |  1.01 |    0.03 |
|                   |                      |           |          |          |           |       |         |
|       **File_Exists** |     **C:\TEMP\test.txt** |  **46.10 μs** | **0.602 μs** | **0.503 μs** |  **45.83 μs** |  **1.00** |    **0.00** |
|    PathFileExists |     C:\TEMP\test.txt |  46.36 μs | 0.251 μs | 0.196 μs |  46.31 μs |  1.01 |    0.01 |
|          FileInfo |     C:\TEMP\test.txt |  46.59 μs | 0.422 μs | 0.395 μs |  46.71 μs |  1.01 |    0.02 |
| FileInfo_TryCatch |     C:\TEMP\test.txt |  46.33 μs | 0.453 μs | 0.378 μs |  46.24 μs |  1.01 |    0.01 |
|                   |                      |           |          |          |           |       |         |
|       **File_Exists** |          **J:\test.not** | **110.75 μs** | **1.002 μs** | **1.113 μs** | **111.00 μs** |  **1.00** |    **0.00** |
|    PathFileExists |          J:\test.not | 111.92 μs | 1.912 μs | 2.125 μs | 111.14 μs |  1.01 |    0.02 |
|          FileInfo |          J:\test.not | 111.52 μs | 1.240 μs | 1.523 μs | 111.14 μs |  1.01 |    0.02 |
| FileInfo_TryCatch |          J:\test.not | 111.58 μs | 1.826 μs | 1.708 μs | 110.86 μs |  1.01 |    0.02 |
|                   |                      |           |          |          |           |       |         |
|       **File_Exists** |          **Z:\test.not** |  **99.31 μs** | **1.643 μs** | **1.283 μs** |  **99.06 μs** |  **1.00** |    **0.00** |
|    PathFileExists |          Z:\test.not | 104.36 μs | 2.059 μs | 3.607 μs | 105.16 μs |  1.02 |    0.04 |
|          FileInfo |          Z:\test.not | 105.06 μs | 2.088 μs | 5.426 μs | 103.23 μs |  1.11 |    0.05 |
| FileInfo_TryCatch |          Z:\test.not | 101.12 μs | 1.630 μs | 2.176 μs | 100.61 μs |  1.03 |    0.03 |
|                   |                      |           |          |          |           |       |         |
|       **File_Exists** |          **Z:\test.txt** | **185.40 μs** | **1.456 μs** | **1.216 μs** | **185.10 μs** |  **1.00** |    **0.00** |
|    PathFileExists |          Z:\test.txt | 187.29 μs | 1.984 μs | 1.549 μs | 186.99 μs |  1.01 |    0.01 |
|          FileInfo |          Z:\test.txt | 187.18 μs | 1.797 μs | 1.593 μs | 187.37 μs |  1.01 |    0.01 |
| FileInfo_TryCatch |          Z:\test.txt | 187.08 μs | 1.464 μs | 1.222 μs | 186.99 μs |  1.01 |    0.01 |
|                   |                      |           |          |          |           |       |         |
|       **File_Exists** | **\\10.(...)t.not [29]** | **105.27 μs** | **1.082 μs** | **0.904 μs** | **105.37 μs** |  **1.00** |    **0.00** |
|    PathFileExists | \\10.(...)t.not [29] | 104.61 μs | 1.111 μs | 0.928 μs | 104.34 μs |  0.99 |    0.02 |
|          FileInfo | \\10.(...)t.not [29] | 105.74 μs | 0.974 μs | 0.863 μs | 105.59 μs |  1.01 |    0.01 |
| FileInfo_TryCatch | \\10.(...)t.not [29] | 105.84 μs | 1.374 μs | 1.738 μs | 105.52 μs |  1.01 |    0.02 |
|                   |                      |           |          |          |           |       |         |
|       **File_Exists** | **\\10.(...)t.txt [29]** | **191.20 μs** | **1.712 μs** | **1.430 μs** | **191.02 μs** |  **1.00** |    **0.00** |
|    PathFileExists | \\10.(...)t.txt [29] | 192.39 μs | 2.864 μs | 2.539 μs | 191.16 μs |  1.01 |    0.02 |
|          FileInfo | \\10.(...)t.txt [29] | 193.22 μs | 3.832 μs | 3.935 μs | 191.47 μs |  1.01 |    0.03 |
| FileInfo_TryCatch | \\10.(...)t.txt [29] | 191.04 μs | 2.153 μs | 1.908 μs | 190.60 μs |  1.00 |    0.01 |

Second run with _disconnected_ VPN, so none of network file exists.

|            Method |             fileName |          Mean |         Error |        StdDev |        Median | Ratio | RatioSD |
|------------------ |--------------------- |--------------:|--------------:|--------------:|--------------:|------:|--------:|
|       **File_Exists** |     **C:\TEMP\test.not** |      **29.88 μs** |      **0.577 μs** |      **0.771 μs** |      **29.56 μs** |  **1.00** |    **0.00** |
|    PathFileExists |     C:\TEMP\test.not |      30.44 μs |      0.445 μs |      0.372 μs |      30.31 μs |  1.01 |    0.03 |
|          FileInfo |     C:\TEMP\test.not |      30.15 μs |      0.551 μs |      0.736 μs |      29.82 μs |  1.01 |    0.03 |
| FileInfo_TryCatch |     C:\TEMP\test.not |      29.94 μs |      0.122 μs |      0.108 μs |      29.94 μs |  0.99 |    0.03 |
|                   |                      |               |               |               |               |       |         |
|       **File_Exists** |     **C:\TEMP\test.txt** |      **46.00 μs** |      **0.651 μs** |      **0.697 μs** |      **45.79 μs** |  **1.00** |    **0.00** |
|    PathFileExists |     C:\TEMP\test.txt |      47.36 μs |      0.213 μs |      0.166 μs |      47.32 μs |  1.03 |    0.02 |
|          FileInfo |     C:\TEMP\test.txt |      46.15 μs |      0.262 μs |      0.219 μs |      46.13 μs |  1.00 |    0.02 |
| FileInfo_TryCatch |     C:\TEMP\test.txt |      46.06 μs |      0.186 μs |      0.156 μs |      46.07 μs |  1.00 |    0.02 |
|                   |                      |               |               |               |               |       |         |
|       **File_Exists** |          **J:\test.not** | **192,579.07 μs** | **24,979.846 μs** | **68,801.750 μs** | **163,922.15 μs** |  **1.00** |    **0.00** |
|    PathFileExists |          J:\test.not |  90,708.63 μs |  6,388.267 μs | 17,487.777 μs |  85,626.45 μs |  0.51 |    0.16 |
|          FileInfo |          J:\test.not | 191,399.02 μs | 18,261.754 μs | 50,603.234 μs | 172,489.40 μs |  1.08 |    0.40 |
| FileInfo_TryCatch |          J:\test.not | 192,275.34 μs | 15,548.226 μs | 43,342.240 μs | 176,359.63 μs |  1.09 |    0.39 |
|                   |                      |               |               |               |               |       |         |
|       **File_Exists** |          **Z:\test.not** |     **734.13 μs** |     **50.053 μs** |    **143.611 μs** |     **731.10 μs** |  **1.00** |    **0.00** |
|    PathFileExists |          Z:\test.not |     207.38 μs |      3.918 μs |      3.272 μs |     208.13 μs |  0.27 |    0.04 |
|          FileInfo |          Z:\test.not |     408.34 μs |      7.726 μs |      9.198 μs |     407.63 μs |  0.55 |    0.09 |
| FileInfo_TryCatch |          Z:\test.not |     426.86 μs |      8.530 μs |     14.252 μs |     423.69 μs |  0.55 |    0.10 |
|                   |                      |               |               |               |               |       |         |
|       **File_Exists** |          **Z:\test.txt** |     **423.64 μs** |      **8.192 μs** |      **7.663 μs** |     **422.04 μs** |  **1.00** |    **0.00** |
|    PathFileExists |          Z:\test.txt |     206.42 μs |      3.919 μs |      5.493 μs |     205.68 μs |  0.49 |    0.02 |
|          FileInfo |          Z:\test.txt |     418.75 μs |      8.126 μs |     15.655 μs |     416.72 μs |  0.96 |    0.04 |
| FileInfo_TryCatch |          Z:\test.txt |     410.17 μs |      8.149 μs |     12.198 μs |     412.30 μs |  0.95 |    0.04 |
|                   |                      |               |               |               |               |       |         |
|       **File_Exists** | **\\10.(...)t.not [29]** |   **2,105.58 μs** |    **105.539 μs** |    **304.503 μs** |   **2,065.60 μs** |  **1.00** |    **0.00** |
|    PathFileExists | \\10.(...)t.not [29] |     860.60 μs |     16.776 μs |     23.517 μs |     862.48 μs |  0.41 |    0.07 |
|          FileInfo | \\10.(...)t.not [29] |   1,613.70 μs |     31.959 μs |     75.954 μs |   1,600.27 μs |  0.78 |    0.11 |
| FileInfo_TryCatch | \\10.(...)t.not [29] |   1,607.34 μs |     14.477 μs |     12.089 μs |   1,603.82 μs |  0.76 |    0.12 |
|                   |                      |               |               |               |               |       |         |
|       **File_Exists** | **\\10.(...)t.txt [29]** |   **1,609.93 μs** |     **30.897 μs** |     **41.247 μs** |   **1,597.26 μs** |  **1.00** |    **0.00** |
|    PathFileExists | \\10.(...)t.txt [29] |     871.10 μs |     15.604 μs |     13.030 μs |     875.64 μs |  0.54 |    0.01 |
|          FileInfo | \\10.(...)t.txt [29] |   1,596.07 μs |     31.785 μs |     34.009 μs |   1,596.91 μs |  0.99 |    0.03 |
| FileInfo_TryCatch | \\10.(...)t.txt [29] |   1,612.69 μs |     23.002 μs |     19.208 μs |   1,607.73 μs |  1.00 |    0.03 |
