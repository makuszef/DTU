using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Digests;

class Program
{
    const int Iterations = 10;

    static async Task Main(string[] args)
    {
        string outputCsv = "hash_benchmark_results.csv";
        // Plik PDF musi znajdować się w katalogu bin/Debug/... obok .exe
        string filePath = Path.Combine(AppContext.BaseDirectory, "MAKOWSKIWCY24KX1S41.pdf");

        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Plik nie znaleziony: {filePath}");
            return;
        }

        // Wczytujemy całą zawartość pliku do pamięci
        byte[] fileData = File.ReadAllBytes(filePath);

        // Jedyny testowany zestaw danych to ten plik
        var allResults = await BenchmarkAllHashes(fileData, Path.GetFileName(filePath));

        SaveResultsToCsv(outputCsv, allResults);

        Console.WriteLine("Testy zakończone! Wyniki zapisane do " + outputCsv);
    }

    static async Task<List<BenchmarkResult>> BenchmarkAllHashes(byte[] data, string dataType)
    {
        var results = new List<BenchmarkResult>();
        results.Add(await BenchmarkHash("MD5",        data, dataType, MD5Hash));
        results.Add(await BenchmarkHash("SHA1",       data, dataType, SHA1Hash));
        results.Add(await BenchmarkHash("SHA3-512",   data, dataType, SHA3Hash));
        results.Add(await BenchmarkHash("Whirlpool",  data, dataType, WhirlpoolHash));
        return results;
    }

    static async Task<BenchmarkResult> BenchmarkHash(
        string name,
        byte[] data,
        string dataType,
        Func<byte[], byte[]> hashFunc)
    {
        var times = new List<double>(Iterations);
        var sw = new Stopwatch();
        double totalTimeSec = 0;

        // Pomiar CPU i pamięci na starcie
        TimeSpan cpuStart = Process.GetCurrentProcess().TotalProcessorTime;
        long memStart    = GC.GetTotalMemory(false);

        for (int i = 0; i < Iterations; i++)
        {
            sw.Restart();
            _ = hashFunc(data);
            sw.Stop();
            times.Add(sw.Elapsed.TotalMilliseconds);
            totalTimeSec += sw.Elapsed.TotalSeconds;
        }

        // Pomiar CPU i pamięci po zakończeniu
        TimeSpan cpuEnd = Process.GetCurrentProcess().TotalProcessorTime;
        long memEnd     = GC.GetTotalMemory(false);

        double avg   = times.Average();
        double std   = Math.Sqrt(times.Select(t => (t - avg)*(t - avg)).Sum() / times.Count);
        double p50   = Percentile(times, 50);
        double p95   = Percentile(times, 95);
        double p99   = Percentile(times, 99);
        double mb    = data.Length / 1024.0 / 1024.0;
        double thrMB = mb / (totalTimeSec / Iterations);
        double thrGB = thrMB / 1024.0;

        return new BenchmarkResult
        {
            Algorithm        = name,
            DataType         = dataType,
            DataSizeMB       = mb,
            AvgLatencyMs     = avg,
            StdDevMs         = std,
            P50LatencyMs     = p50,
            P95LatencyMs     = p95,
            P99LatencyMs     = p99,
            ThroughputMBps   = thrMB,
            ThroughputGBps   = thrGB,
            CpuUsageDiffSec  = (cpuEnd - cpuStart).TotalSeconds,
            MemoryUsageDiffMB= (memEnd - memStart) / 1024.0 / 1024.0
        };
    }

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

    static double Percentile(List<double> seq, double pct)
    {
        var sorted = seq.OrderBy(x => x).ToArray();
        double pos = (sorted.Length - 1) * pct / 100.0;
        int idx = (int)pos;
        double frac = pos - idx;
        if (idx + 1 < sorted.Length)
            return sorted[idx] + (sorted[idx + 1] - sorted[idx]) * frac;
        else
            return sorted[idx];
    }

    static void SaveResultsToCsv(string path, List<BenchmarkResult> results)
    {
        var header = "Algorithm,DataType,DataSizeMB,AvgLatencyMs,StdDevMs,P50LatencyMs,P95LatencyMs,P99LatencyMs,ThroughputMBps,ThroughputGBps,CpuUsageDiffSec,MemoryUsageDiffMB";
        var lines = new List<string> { header };
        lines.AddRange(results.Select(r =>
            $"{r.Algorithm},{r.DataType},{r.DataSizeMB:F2},{r.AvgLatencyMs:F3},{r.StdDevMs:F3},{r.P50LatencyMs:F3},{r.P95LatencyMs:F3},{r.P99LatencyMs:F3},{r.ThroughputMBps:F2},{r.ThroughputGBps:F4},{r.CpuUsageDiffSec:F3},{r.MemoryUsageDiffMB:F3}"
        ));
        File.WriteAllLines(path, lines);
    }
}

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
