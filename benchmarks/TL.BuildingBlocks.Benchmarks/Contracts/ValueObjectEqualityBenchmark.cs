using BenchmarkDotNet.Attributes;
using TL.BaseContracts.Domain;

namespace TL.BuildingBlocks.Benchmarks.Contracts;

public class AddressValueObject : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string ZipCode { get; }

    public AddressValueObject(string street, string city, string zipCode)
    {
        Street = street;
        City = city;
        ZipCode = zipCode;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return ZipCode;
    }
}

[MemoryDiagnoser]
public class ValueObjectEqualityBenchmark
{
    private AddressValueObject _addr1 = null!;
    private AddressValueObject _addr2 = null!;
    private AddressValueObject _addr3 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _addr1 = new AddressValueObject("Avenida Paulista, 1000", "São Paulo", "01310-100");
        _addr2 = new AddressValueObject("Avenida Paulista, 1000", "São Paulo", "01310-100");
        _addr3 = new AddressValueObject("Rua XV de Novembro, 500", "Curitiba", "80020-310");
    }

    [Benchmark(Baseline = true, Description = "ValueObject.Equals (Identical Values)")]
    public bool CompareEqualValueObjects()
    {
        return _addr1.Equals(_addr2);
    }

    [Benchmark(Description = "ValueObject.Equals (Different Values)")]
    public bool CompareDifferentValueObjects()
    {
        return _addr1.Equals(_addr3);
    }
}
