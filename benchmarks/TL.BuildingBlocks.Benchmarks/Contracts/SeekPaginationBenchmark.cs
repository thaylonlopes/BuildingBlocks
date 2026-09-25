using BenchmarkDotNet.Attributes;
using TL.BaseContracts;

namespace TL.BuildingBlocks.Benchmarks.Contracts;

[MemoryDiagnoser]
public class SeekPaginationBenchmark
{
    private static readonly List<string> Items = ["Item-1", "Item-2", "Item-3", "Item-4", "Item-5"];

    [Benchmark(Description = "SeekRequest Creation and SeekResult Instantiation")]
    public SeekResult<string, long> CreateSeekResult()
    {
        var request = new SeekRequest<long>(lastSeenId: 1000, pageSize: 50);
        return SeekResult<string, long>.Create(Items, request.PageSize, hasNextPage: true, nextCursor: 1005);
    }
}
