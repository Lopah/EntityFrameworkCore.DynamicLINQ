using System.Threading.Tasks;
using EF.EntityFrameworkCore.DynamicWhere;
using EntityFrameworkCore.Tests.Base;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.Tests;

[CollectionDefinition("ConditionalWhere", DisableParallelization = true)]
[Collection("ConditionalWhere")]
public class PreConditionalWhereTests
{
    private readonly ApplicationDbContext _applicationDbContext;

    public PreConditionalWhereTests()
    {
        _applicationDbContext = new ApplicationFake().DbContext;
    }

    [Fact]
    public async Task PreConditionalWhere_GivenFalsyCondition_ReturnsAppliedPredicateOnQueryableSource()
    {
        var falsyCondition = false;
        var test = await _applicationDbContext.Cars.PreConditionalWhere(falsyCondition, e => e.Bought == true)
            .ToListAsync();

        test.Count.Should().Be(5);
    }

    [Fact]
    public async Task PreConditionalWhere_GivenTruthyCondition_ReturnsAppliedPredicateOnQueryableSource()
    {
        var truthyCondition = true;
        var test = await _applicationDbContext.Cars.PreConditionalWhere(truthyCondition, e => e.Bought == true)
            .ToListAsync();

        test.Count.Should().Be(3);
    }

    [Fact]
    public async Task PreConditionalWhereWithLambda_GivenFalsyCondition_ReturnsAppliedPredicateOnQueryableSource()
    {
        var falsyCondition = () => false;
        var test = await _applicationDbContext.Cars.PreConditionalWhere(falsyCondition, e => e.Bought == true)
            .ToListAsync();

        test.Count.Should().Be(5);
    }

    [Fact]
    public async Task
        PreConditionalWhereWithLambda_GivenFalsyConditionThatIsNotEqualityExpression_ReturnsAppliedPredicateOnQueryableSourceAndDoesNotFail()
    {
        var falsyCondition = () => false;
        var test = await _applicationDbContext.Cars.PreConditionalWhere(falsyCondition, e => e.Bought)
            .ToListAsync();

        test.Count.Should().Be(5);
    }
    
    [Fact]
    public async Task
        PreConditionalWhereWithLambda_GivenTruthyConditionThatIsNotEqualityExpression_ReturnsAppliedPredicateOnQueryableSourceAndDoesNotFail()
    {
        var truthyCondition = () => true;
        var test = await _applicationDbContext.Cars.PreConditionalWhere(truthyCondition, e => !e.Bought)
            .ToListAsync();

        test.Count.Should().Be(2);
    }
    
    [Fact]
    public async Task
        PreConditionalWhereWithLambda_GivenTruthyConditionThatIsNotEqualityExpression_ReturnsAppliedPredicateOnQueryableSourceAndDoesNotFail2()
    {
        var truthyCondition = () => true;
        var test = await _applicationDbContext.Cars.PreConditionalWhere(truthyCondition, e => e.Bought)
            .ToListAsync();

        test.Count.Should().Be(3);
    }

    [Fact]
    public async Task PreConditionalWhereWithLambda_GivenTruthyCondition_ReturnsAppliedPredicateOnQueryableSource()
    {
        var truthyCondition = () => true;
        var test = await _applicationDbContext.Cars.PreConditionalWhere(truthyCondition, e => e.Bought == true)
            .ToListAsync();

        test.Count.Should().Be(3);
    }
}
