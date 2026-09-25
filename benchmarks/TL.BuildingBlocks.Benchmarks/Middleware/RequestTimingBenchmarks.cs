using BenchmarkDotNet.Attributes;
using System.Diagnostics;

namespace TL.BuildingBlocks.Benchmarks.Middleware;

[MemoryDiagnoser]
public class RequestTimingBenchmarks
{
    [Benchmark(Baseline = true, Description = "Stopwatch.StartNew (Heap Allocation)")]
    public long Tradicional_StopwatchStartNew()
    {
        var stopwatch = Stopwatch.StartNew();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    [Benchmark(Description = "Stopwatch.GetTimestamp (Zero-Allocation)")]
    public long Otimizado_ZeroAllocationTimestamp()
    {
        long startTimestamp = Stopwatch.GetTimestamp();
        return (long)Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
    }
}
