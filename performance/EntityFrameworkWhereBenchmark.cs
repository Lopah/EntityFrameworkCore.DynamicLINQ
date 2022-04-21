using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using EF.EntityFrameworkCore.DynamicWhere;
using EntityFrameworkCore.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Performance;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class EntityFrameworkWhereBenchmark
{
    private ApplicationDbContext _applicationDbContext = null!;

    [GlobalSetup]
    public void Setup()
    {
        _applicationDbContext = new ApplicationFake().DbContext;
    }

    [GlobalCleanup]
    public void CleanUp()
    {
        _applicationDbContext.Dispose();
    }

    [Benchmark]
    public async Task ConditionalWhereWithEquation()
    {
        await _applicationDbContext.Cars.ConditionalWhere(true, e => e.Bought == true)
            .ToListAsync();
    }

    [Benchmark]
    public async Task ConditionalWhereWithPropertyAccess()
    {
        await _applicationDbContext.Cars.ConditionalWhere(true, e => e.Bought)
            .ToListAsync();
    }
    
    [Benchmark]
    public async Task PureConditionalWhereWithEquation()
    {
        await _applicationDbContext.Cars.ConditionalWhere(true, e => e.Bought == true)
            .ToListAsync();
    }

    [Benchmark]
    public async Task PureConditionalWhereWithPropertyAccess()
    {
        await _applicationDbContext.Cars.ConditionalWhere(true, e => e.Bought)
            .ToListAsync();
    }
    
}
