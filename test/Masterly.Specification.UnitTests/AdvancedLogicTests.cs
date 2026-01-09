using Shouldly;

namespace Masterly.Specification.UnitTests;

public class AdvancedLogicTests
{
    private record TestEntity(int Value, bool FlagA, bool FlagB);

    [Theory]
    [InlineData(true, true, false)]   // Both true = XOR false
    [InlineData(true, false, true)]   // A true, B false = XOR true
    [InlineData(false, true, true)]   // A false, B true = XOR true
    [InlineData(false, false, false)] // Both false = XOR false
    public void Xor_Should_Return_True_When_Exactly_One_Is_True(bool a, bool b, bool expected)
    {
        var specA = new ExpressionSpecification<TestEntity>(e => e.FlagA == a);
        var specB = new ExpressionSpecification<TestEntity>(e => e.FlagB == b);
        var entity = new TestEntity(0, true, true);

        var xorSpec = specA.Xor(specB);
        xorSpec.IsSatisfiedBy(entity).ShouldBe(expected);
    }

    [Theory]
    [InlineData(true, true, true)]    // A implies B: T->T = T
    [InlineData(true, false, false)]  // A implies B: T->F = F
    [InlineData(false, true, true)]   // A implies B: F->T = T
    [InlineData(false, false, true)]  // A implies B: F->F = T
    public void Implies_Should_Follow_Material_Implication(bool a, bool b, bool expected)
    {
        var specA = new ExpressionSpecification<TestEntity>(e => e.FlagA);
        var specB = new ExpressionSpecification<TestEntity>(e => e.FlagB);
        var entity = new TestEntity(0, a, b);

        var impliesSpec = specA.Implies(specB);
        impliesSpec.IsSatisfiedBy(entity).ShouldBe(expected);
    }

    [Theory]
    [InlineData(true, true, true)]    // A iff B: T<->T = T
    [InlineData(true, false, false)]  // A iff B: T<->F = F
    [InlineData(false, true, false)]  // A iff B: F<->T = F
    [InlineData(false, false, true)]  // A iff B: F<->F = T
    public void Iff_Should_Return_True_When_Both_Have_Same_Value(bool a, bool b, bool expected)
    {
        var specA = new ExpressionSpecification<TestEntity>(e => e.FlagA);
        var specB = new ExpressionSpecification<TestEntity>(e => e.FlagB);
        var entity = new TestEntity(0, a, b);

        var iffSpec = specA.Iff(specB);
        iffSpec.IsSatisfiedBy(entity).ShouldBe(expected);
    }

    [Theory]
    [InlineData(true, true, false)]   // NAND: T,T = F
    [InlineData(true, false, true)]   // NAND: T,F = T
    [InlineData(false, true, true)]   // NAND: F,T = T
    [InlineData(false, false, true)]  // NAND: F,F = T
    public void Nand_Should_Return_False_Only_When_Both_Are_True(bool a, bool b, bool expected)
    {
        var specA = new ExpressionSpecification<TestEntity>(e => e.FlagA);
        var specB = new ExpressionSpecification<TestEntity>(e => e.FlagB);
        var entity = new TestEntity(0, a, b);

        var nandSpec = specA.Nand(specB);
        nandSpec.IsSatisfiedBy(entity).ShouldBe(expected);
    }

    [Theory]
    [InlineData(true, true, false)]   // NOR: T,T = F
    [InlineData(true, false, false)]  // NOR: T,F = F
    [InlineData(false, true, false)]  // NOR: F,T = F
    [InlineData(false, false, true)]  // NOR: F,F = T
    public void Nor_Should_Return_True_Only_When_Both_Are_False(bool a, bool b, bool expected)
    {
        var specA = new ExpressionSpecification<TestEntity>(e => e.FlagA);
        var specB = new ExpressionSpecification<TestEntity>(e => e.FlagB);
        var entity = new TestEntity(0, a, b);

        var norSpec = specA.Nor(specB);
        norSpec.IsSatisfiedBy(entity).ShouldBe(expected);
    }

    [Fact]
    public void Advanced_Logic_Should_Work_With_Queryable()
    {
        var entities = new[]
        {
            new TestEntity(1, true, true),
            new TestEntity(2, true, false),
            new TestEntity(3, false, true),
            new TestEntity(4, false, false)
        }.AsQueryable();

        var specA = new ExpressionSpecification<TestEntity>(e => e.FlagA);
        var specB = new ExpressionSpecification<TestEntity>(e => e.FlagB);

        // XOR should match entities 2 and 3
        var xorResult = entities.Where(specA.Xor(specB).ToExpression()).ToList();
        xorResult.Select(e => e.Value).ShouldBe(new[] { 2, 3 }, ignoreOrder: true);

        // NOR should match entity 4 only
        var norResult = entities.Where(specA.Nor(specB).ToExpression()).ToList();
        norResult.Select(e => e.Value).ShouldBe(new[] { 4 });
    }
}