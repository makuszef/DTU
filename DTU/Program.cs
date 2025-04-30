using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Digests;

class Program
{
    const int Iterations = 100;

    static async Task Main(string[] args)
    {
        string baseDir = AppContext.BaseDirectory;
        string testDir = Path.Combine(baseDir, "TestowanePliki");

        if (!Directory.Exists(testDir))
        {
            Console.Error.WriteLine($"Folder nie istnieje: {testDir}");
            return;
        }

        var files = Directory.GetFiles(testDir);
        if (files.Length == 0)
        {
            Console.Error.WriteLine("Brak plików w folderze TestowanePliki.");
            return;
        }

        foreach (var filePath in files)
        {
            Console.WriteLine($"Przetwarzanie pliku: {Path.GetFileName(filePath)}");

            byte[] fileData = File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);
            string outputDir = Path.Combine(baseDir, Path.GetFileNameWithoutExtension(fileName));
            Directory.CreateDirectory(outputDir);

            var allResults = await BenchmarkAllHashes(fileData, fileName, outputDir);

            string summaryCsv = Path.Combine(outputDir, "hash_benchmark_results.csv");
            SaveSummaryCsv(summaryCsv, allResults);

            Console.WriteLine($"  -> Zakończono. Wyniki zapisane w {outputDir}\n");
        }

        Console.WriteLine("=== WSZYSTKIE TESTY ZAKOŃCZONE ===");
    }

    static async Task<List<BenchmarkResult>> BenchmarkAllHashes(
        byte[] data, string dataType, string outputDir)
    {
        var algos = new (string name, Func<byte[], byte[]> func)[]
        {
            ("MD5",       MD5Hash),
            ("SHA1",      SHA1Hash),
            ("SHA3-512",  SHA3Hash),
            ("Whirlpool", WhirlpoolHash)
        };

        var results = new List<BenchmarkResult>();
        foreach (var (name, func) in algos)
        {
            var result = await BenchmarkHash(name, data, dataType, func, outputDir);
            results.Add(result);
        }
        return results;
    }

    static async Task<BenchmarkResult> BenchmarkHash(
        string name,
        byte[] data,
        string dataType,
        Func<byte[], byte[]> hashFunc,
        string outputDir)
    {
        double mb = data.Length / 1024.0 / 1024.0;
        var iterationRecords = new List<IterationResult>(Iterations);

        // Pomiar globalny
        TimeSpan cpuGlobalStart = Process.GetCurrentProcess().TotalProcessorTime;
        long memGlobalStart    = GC.GetTotalMemory(false);

        for (int i = 0; i < Iterations; i++)
        {
            // Pomiar per-iteracja
            TimeSpan cpuStart = Process.GetCurrentProcess().TotalProcessorTime;
            long memStart     = GC.GetTotalMemory(false);

            var sw = Stopwatch.StartNew();
            _ = hashFunc(data);
            sw.Stop();

            TimeSpan cpuEnd = Process.GetCurrentProcess().TotalProcessorTime;
            long memEnd     = GC.GetTotalMemory(false);

            double latencyMs   = sw.Elapsed.TotalMilliseconds;
            double cpuSec      = (cpuEnd - cpuStart).TotalSeconds;
            double memMB       = (memEnd - memStart) / 1024.0 / 1024.0;
            double throughput  = mb / sw.Elapsed.TotalSeconds;

            iterationRecords.Add(new IterationResult
            {
                Iteration      = i + 1,
                LatencyMs      = latencyMs,
                CpuTimeSec     = cpuSec,
                MemoryUsageMB  = memMB,
                ThroughputMBps = throughput
            });
        }

        // Pomiar globalny na koniec
        TimeSpan cpuGlobalEnd = Process.GetCurrentProcess().TotalProcessorTime;
        long memGlobalEnd     = GC.GetTotalMemory(false);

        // Zapis surowych danych do CSV
        string rawCsv = Path.Combine(
            outputDir,
            name.ToLower().Replace("-", "_") + ".csv"
        );
        using var rawWriter = new StreamWriter(rawCsv);
        rawWriter.WriteLine("Iteration,LatencyMs,CpuTimeSec,MemoryUsageMB,ThroughputMBps");
        foreach (var rec in iterationRecords)
        {
            rawWriter.WriteLine(string.Join(",",
                rec.Iteration,
                rec.LatencyMs     .ToString("F3", CultureInfo.InvariantCulture),
                rec.CpuTimeSec    .ToString("F3", CultureInfo.InvariantCulture),
                rec.MemoryUsageMB .ToString("F3", CultureInfo.InvariantCulture),
                rec.ThroughputMBps.ToString("F2", CultureInfo.InvariantCulture)
            ));
        }

        // Oblicz podsumowanie
        var latencies = iterationRecords.Select(r => r.LatencyMs).ToList();
        double avg   = latencies.Average();
        double std   = Math.Sqrt(latencies.Select(t => (t - avg) * (t - avg)).Sum() / latencies.Count);
        double p50   = Percentile(latencies, 50);
        double p95   = Percentile(latencies, 95);
        double p99   = Percentile(latencies, 99);
        double totalTimeSec = iterationRecords.Sum(r => r.LatencyMs) / 1000.0;
        double thrMB = mb / (totalTimeSec / Iterations);
        double thrGB = thrMB / 1024.0;

        return new BenchmarkResult
        {
            Algorithm         = name,
            DataType          = dataType,
            DataSizeMB        = mb,
            AvgLatencyMs      = avg,
            StdDevMs          = std,
            P50LatencyMs      = p50,
            P95LatencyMs      = p95,
            P99LatencyMs      = p99,
            ThroughputMBps    = thrMB,
            ThroughputGBps    = thrGB,
            CpuUsageDiffSec   = (cpuGlobalEnd - cpuGlobalStart).TotalSeconds,
            MemoryUsageDiffMB = (memGlobalEnd - memGlobalStart) / 1024.0 / 1024.0
        };
    }

    // Funkcje hashu

    static byte[] MD5Hash(byte[] data)
    {
        using var md5 = MD5.Create();
        return md5.ComputeHash(data);
    }

    static byte[] SHA1Hash(byte[] data)
    {
        using var sha1 = SHA1.Create();
        return sha1.ComputeHash(data);
    }

    static byte[] SHA3Hash(byte[] data)
    {
        var sha3 = new Sha3Digest(512);
        sha3.BlockUpdate(data, 0, data.Length);
        byte[] res = new byte[sha3.GetDigestSize()];
        sha3.DoFinal(res, 0);
        return res;
    }

    static byte[] WhirlpoolHash(byte[] data)
    {
        var w = new WhirlpoolDigest();
        w.BlockUpdate(data, 0, data.Length);
        byte[] res = new byte[w.GetDigestSize()];
        w.DoFinal(res, 0);
        return res;
    }

    // Utility

    static double Percentile(List<double> seq, double pct)
    {
        var sorted = seq.OrderBy(x => x).ToArray();
        double pos = (sorted.Length - 1) * pct / 100.0;
        int idx = (int)pos;
        double frac = pos - idx;
        return (idx + 1 < sorted.Length)
            ? sorted[idx] + (sorted[idx + 1] - sorted[idx]) * frac
            : sorted[idx];
    }

    static void SaveSummaryCsv(string path, List<BenchmarkResult> results)
    {
        using var writer = new StreamWriter(path);
        writer.WriteLine("Algorithm,DataType,DataSizeMB,AvgLatencyMs,StdDevMs,P50LatencyMs,P95LatencyMs,P99LatencyMs," +
                         "ThroughputMBps,ThroughputGBps,CpuUsageDiffSec,MemoryUsageDiffMB");
        foreach (var r in results)
        {
            writer.WriteLine(string.Join(",",
                r.Algorithm,
                r.DataType,
                r.DataSizeMB       .ToString("F2", CultureInfo.InvariantCulture),
                r.AvgLatencyMs     .ToString("F3", CultureInfo.InvariantCulture),
                r.StdDevMs         .ToString("F3", CultureInfo.InvariantCulture),
                r.P50LatencyMs     .ToString("F3", CultureInfo.InvariantCulture),
                r.P95LatencyMs     .ToString("F3", CultureInfo.InvariantCulture),
                r.P99LatencyMs     .ToString("F3", CultureInfo.InvariantCulture),
                r.ThroughputMBps   .ToString("F2", CultureInfo.InvariantCulture),
                r.ThroughputGBps   .ToString("F4", CultureInfo.InvariantCulture),
                r.CpuUsageDiffSec  .ToString("F3", CultureInfo.InvariantCulture),
                r.MemoryUsageDiffMB.ToString("F3", CultureInfo.InvariantCulture)
            ));
        }
    }
}

// Modele wyników

class BenchmarkResult
{
    public string Algorithm { get; set; }
    public string DataType { get; set; }
    public double DataSizeMB { get; set; }
    public double AvgLatencyMs { get; set; }
    public double StdDevMs { get; set; }
    public double P50LatencyMs { get; set; }
    public double P95LatencyMs { get; set; }
    public double P99LatencyMs { get; set; }
    public double ThroughputMBps { get; set; }
    public double ThroughputGBps { get; set; }
    public double CpuUsageDiffSec { get; set; }
    public double MemoryUsageDiffMB { get; set; }
}

class IterationResult
{
    public int    Iteration      { get; set; }
    public double LatencyMs      { get; set; }
    public double CpuTimeSec     { get; set; }
    public double MemoryUsageMB  { get; set; }
    public double ThroughputMBps { get; set; }
}
