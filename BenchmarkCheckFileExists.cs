using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace FileExistsBenchmark
{
    //[ShortRunJob]
    public class BenchmarkCheckFileExists
    {
        [System.Runtime.InteropServices.DllImport("shlwapi.dll", EntryPoint = "PathFileExistsW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool _PathFileExists([MarshalAs(UnmanagedType.LPWStr)]string pszPath);

        public string[] FileNames => new string[]
        {
            @"C:\TEMP\test.txt", // File exists
            @"C:\TEMP\test.not", // File not exist
            @"\\10.187.39.110\Temp\test.txt", // File exists
            @"\\10.187.39.110\Temp\test.not", // File not exists
            @"Z:\test.txt", // File exists, map Z: \\10.187.39.110\Temp
            @"Z:\test.not", // File not exists, map Z: \\10.187.39.110\Temp
            @"J:\test.not", // File not exists, network drive is inactive
        };

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(FileNames))]
        public bool File_Exists(string fileName) =>  File.Exists(fileName);
        
        [Benchmark]
        [ArgumentsSource(nameof(FileNames))]
        public bool PathFileExists(string fileName) => _PathFileExists(fileName);
        
        [Benchmark]
        [ArgumentsSource(nameof(FileNames))]
        public bool FileInfo(string fileName) => (new FileInfo(fileName)).Exists;

        [Benchmark]
        [ArgumentsSource(nameof(FileNames))]
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

        [Benchmark]
        [ArgumentsSource(nameof(FileNames))]
        public bool FileOpenRead(string fileName)
        {
            try
            {
                using (File.OpenRead(fileName))
                { 
                    return true; 
                }                
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(FileNames))]
        // Alternative based on http://www.jonathanantoine.com/2011/08/18/faster-file-exists/
        public bool PathFileExistsAsync(string fileName)
        {
            var task = new Task<bool>(() =>
            {
                return PathFileExists(fileName);
            });
            task.Start();
            return task.Wait(5000) && task.Result;

        }
    }
}
