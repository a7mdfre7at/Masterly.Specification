using Shouldly;

namespace Masterly.Specification.UnitTests;

public class CompositeTests
{
    private record Product(string Name, decimal Price, int Stock, string Category);

    private readonly IQueryable<Product> _products;

    public CompositeTests()
    {
        _products = new[]
        {
            new Product("Laptop", 999m, 10, "Electronics"),
            new Product("Phone", 599m, 25, "Electronics"),
            new Product("Tablet", 399m, 15, "Electronics"),
            new Product("Chair", 149m, 50, "Furniture"),
            new Product("Desk", 299m, 20, "Furniture"),
            new Product("Lamp", 49m, 100, "Furniture")
        }.AsQueryable();
    }

    [Fact]
    public void All_Should_Require_All_Specifications_To_Pass()
    {
        var expensive = new ExpressionSpecification<Product>(p => p.Price > 200);
        var electronics = new ExpressionSpecification<Product>(p => p.Category == "Electronics");
        var inStock = new ExpressionSpecification<Product>(p => p.Stock > 5);

        var allSpec = Specifications.All(expensive, electronics, inStock);

        var result = _products.Where(allSpec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new[] { "Laptop", "Phone", "Tablet" }, ignoreOrder: true);
    }

    [Fact]
    public void AnyOf_Should_Pass_When_At_Least_One_Specification_Passes()
    {
        var veryExpensive = new ExpressionSpecification<Product>(p => p.Price > 500);
        var furniture = new ExpressionSpecification<Product>(p => p.Category == "Furniture");

        var anySpec = Specifications.AnyOf(veryExpensive, furniture);

        var result = _products.Where(anySpec.ToExpression()).ToList();
        result.Count.ShouldBe(5); // Laptop, Phone, Chair, Desk, Lamp
    }

    [Fact]
    public void Exactly_Should_Require_Exact_Number_Of_Matching_Specifications()
    {
        var expensive = new ExpressionSpecification<Product>(p => p.Price > 200);
        var electronics = new ExpressionSpecification<Product>(p => p.Category == "Electronics");
        var lowStock = new ExpressionSpecification<Product>(p => p.Stock < 30);

        // Exactly 2 conditions should match
        var exactlyTwoSpec = Specifications.Exactly(2, expensive, electronics, lowStock);

        var result = _products.Where(exactlyTwoSpec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new[] { "Desk" });
    }

    [Fact]
    public void AtLeast_Should_Require_Minimum_Number_Of_Matching_Specifications()
    {
        ExpressionSpecification<Product> affordable = new ExpressionSpecification<Product>(p => p.Price < 400);
        ExpressionSpecification<Product> furniture = new ExpressionSpecification<Product>(p => p.Category == "Furniture");
        ExpressionSpecification<Product> highStock = new ExpressionSpecification<Product>(p => p.Stock >= 20);

        ISpecification<Product> atLeastTwoSpec = Specifications.AtLeast(2, affordable, furniture, highStock);

        List<Product> result = _products.Where(atLeastTwoSpec.ToExpression()).ToList();
        // Laptop: 0 matches, Phone: highStock=1, Tablet: affordable=1
        // Chair: affordable+furniture+highStock=3 >= 2 ✓
        // Desk: affordable+furniture+highStock=3 >= 2 ✓
        // Lamp: affordable+furniture+highStock=3 >= 2 ✓
        result.Select(p => p.Name).ShouldBe(new[] { "Chair", "Desk", "Lamp" }, ignoreOrder: true);
    }

    [Fact]
    public void AtMost_Should_Limit_Number_Of_Matching_Specifications()
    {
        var expensive = new ExpressionSpecification<Product>(p => p.Price > 200);
        var electronics = new ExpressionSpecification<Product>(p => p.Category == "Electronics");
        var lowStock = new ExpressionSpecification<Product>(p => p.Stock < 30);

        var atMostOneSpec = Specifications.AtMost(1, expensive, electronics, lowStock);

        var result = _products.Where(atMostOneSpec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new[] { "Chair", "Lamp" }, ignoreOrder: true);
    }

    [Fact]
    public void NoneOf_Should_Require_No_Specifications_To_Pass()
    {
        var expensive = new ExpressionSpecification<Product>(p => p.Price > 500);
        var electronics = new ExpressionSpecification<Product>(p => p.Category == "Electronics");

        var noneSpec = Specifications.NoneOf(expensive, electronics);

        var result = _products.Where(noneSpec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new[] { "Chair", "Desk", "Lamp" }, ignoreOrder: true);
    }

    [Fact]
    public void Composite_Should_Throw_When_Empty_Specifications()
    {
        Should.Throw<ArgumentException>(() => Specifications.All<Product>());
    }
}
