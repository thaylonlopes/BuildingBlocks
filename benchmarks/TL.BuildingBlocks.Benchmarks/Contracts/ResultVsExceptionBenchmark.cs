using BenchmarkDotNet.Attributes;
using TL.BaseContracts;

namespace TL.BuildingBlocks.Benchmarks.Contracts;

[MemoryDiagnoser]
public class ResultVsExceptionBenchmark
{
    private static readonly Error SampleError = Error.Failure("Order.ProcessingFailed", "Falha de processamento de pedido.");

    [Benchmark(Baseline = true, Description = "Throw/Catch Exception (Flow Control)")]
    public string ThrowAndCatchException()
    {
        try
        {
            throw new InvalidOperationException("Falha de processamento de pedido.");
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
    }

    [Benchmark(Description = "Result.Failure (Functional Pattern)")]
    public string ReturnFailureResult()
    {
        Result<string> result = Result.Failure<string>(SampleError);
        return result.IsFailure ? result.Error.Message : string.Empty;
    }
}
