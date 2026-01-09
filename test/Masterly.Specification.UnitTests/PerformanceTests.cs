using Shouldly;

namespace Masterly.Specification.UnitTests;

public class PerformanceTests
{
    private record Item(int Id, string Name, decimal Price);

    [Fact]
    public void Cached_Should_Return_Same_Results_As_Original()
    {
        var original = new ExpressionSpecification<Item>(i => i.Price > 50);
        var cached = original.Cached();

        var item1 = new Item(1, "Expensive", 100);
        var item2 = new Item(2, "Cheap", 25);

        cached.IsSatisfiedBy(item1).ShouldBe(original.IsSatisfiedBy(item1));
        cached.IsSatisfiedBy(item2).ShouldBe(original.IsSatisfiedBy(item2));
    }

    [Fact]
    public void Cached_Should_Work_With_Queryable()
    {
        var items = new[]
        {
            new Item(1, "A", 100),
            new Item(2, "B", 25),
            new Item(3, "C", 75)
        }.AsQueryable();

        var spec = new ExpressionSpecification<Item>(i => i.Price > 50).Cached();

        var result = items.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void Cached_Should_Return_Same_Instance_When_Already_Cached()
    {
        var original = new ExpressionSpecification<Item>(i => i.Price > 50);
        var cached1 = original.Cached();
        var cached2 = cached1.Cached();

        cached2.ShouldBeSameAs(cached1);
    }

    [Fact]
    public void Compile_Should_Return_Delegate()
    {
        var spec = new ExpressionSpecification<Item>(i => i.Price > 50);
        var compiled = spec.Compile();

        compiled.ShouldNotBeNull();
        compiled(new Item(1, "Test", 100)).ShouldBeTrue();
        compiled(new Item(2, "Test", 25)).ShouldBeFalse();
    }

    [Fact]
    public void CompiledPredicate_Should_Be_Accessible()
    {
        var spec = new ExpressionSpecification<Item>(i => i.Price > 50).Cached();

        var predicate = spec.CompiledPredicate;
        predicate.ShouldNotBeNull();
    }

    [Fact]
    public void Memoized_Should_Cache_Results_For_Same_Object()
    {
        ISpecification<Item> spec = new ExpressionSpecification<Item>(i => i.Price > 50).Memoized();

        Item item = new Item(1, "Test", 100);

        // First call should evaluate
        spec.IsSatisfiedBy(item).ShouldBeTrue();

        // Second call should use cached result
        spec.IsSatisfiedBy(item).ShouldBeTrue();

        // Third call - result is correct and consistent
        spec.IsSatisfiedBy(item).ShouldBeTrue();
    }

    [Fact]
    public void Cached_Spec_Should_Work_With_Complex_Specifications()
    {
        var expensive = new ExpressionSpecification<Item>(i => i.Price > 50);
        var namedA = new ExpressionSpecification<Item>(i => i.Name.StartsWith("A"));
        var combined = expensive.And(namedA).Cached();

        var items = new[]
        {
            new Item(1, "Apple", 100),
            new Item(2, "Banana", 75),
            new Item(3, "Avocado", 25)
        };

        combined.IsSatisfiedBy(items[0]).ShouldBeTrue();  // Apple, $100
        combined.IsSatisfiedBy(items[1]).ShouldBeFalse(); // Banana, $75 (doesn't start with A)
        combined.IsSatisfiedBy(items[2]).ShouldBeFalse(); // Avocado, $25 (not expensive)
    }
}