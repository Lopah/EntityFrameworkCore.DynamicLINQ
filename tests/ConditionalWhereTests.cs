using System.Threading.Tasks;
using EF.EntityFrameworkCore.DynamicWhere;
using EntityFrameworkCore.Tests.Base;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.Tests;

[CollectionDefinition("ConditionalWhere", DisableParallelization = true)]
[Collection("ConditionalWhere")]
public class ConditionalWhereTests
{
    private readonly ApplicationDbContext _applicationDbContext;

    public ConditionalWhereTests()
    {
        _applicationDbContext = new ApplicationFake().DbContext;
    }

    [Fact]
    public async Task PureConditionalWhere_GivenFalsyCondition_ReturnsTheSameQueryableSource()
    {
        var falsyCondition = false;
        var test = await _applicationDbContext.Cars.ConditionalWhere(falsyCondition, e => e.Bought == true)
            .ToListAsync();

        test.Count.Should().Be(5);
    }

    [Fact]
    public async Task PureConditionalWhere_GivenTruthyCondition_ReturnsAppliedPredicateOnQueryableSource()
    {
        var truthyCondition = true;
        var test = await _applicationDbContext.Cars.ConditionalWhere(truthyCondition, e => e.Bought == true)
            .ToListAsync();

        test.Count.Should().Be(3);
    }

    [Fact]
    public async Task ConditionalWhereWithLambda_GivenFalsyCondition_ReturnsTheSameQueryableSource()
    {
        var falsyCondition = () => false;
        var test = await _applicationDbContext.Cars.ConditionalWhere(falsyCondition, e => e.Bought == true)
            .ToListAsync();

        test.Count.Should().Be(5);
    }

    [Fact]
    public async Task ConditionalWhereWithLambda_GivenTruthyCondition_ReturnsAppliedPredicateOnQueryableSource()
    {
        var truthyCondition = () => true;
        var test = await _applicationDbContext.Cars.ConditionalWhere(truthyCondition, e => e.Bought == true)
            .ToListAsync();

        test.Count.Should().Be(3);
    }
}
