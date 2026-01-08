using Shouldly;

namespace Masterly.Specification.UnitTests;

public class PropertySpecificationTests
{
    private record Person(string? Name, int Age, string Email, IEnumerable<string> Tags);

    private readonly IQueryable<Person> _people;

    public PropertySpecificationTests()
    {
        _people = new[]
        {
            new Person("Alice", 25, "alice@example.com", new[] { "developer", "senior" }),
            new Person("Bob", 17, "bob@test.org", new[] { "intern" }),
            new Person("Charlie", 30, "", new[] { "manager", "senior" }),
            new Person("Diana", 22, "diana@example.com", new[] { "developer" }),
            new Person(null, 35, "anonymous@company.com", Array.Empty<string>())
        }.AsQueryable();
    }

    [Fact]
    public void Property_EqualTo_Should_Match_Exact_Value()
    {
        var spec = Property<Person>.For(p => p.Age).EqualTo(25);
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Alice");
    }

    [Fact]
    public void Property_NotEqualTo_Should_Exclude_Value()
    {
        var spec = Property<Person>.For(p => p.Age).NotEqualTo(25);
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(4);
    }

    [Fact]
    public void Property_IsNull_Should_Match_Null_Values()
    {
        var spec = Property<Person>.For(p => p.Name).IsNull();
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(1);
        result[0].Age.ShouldBe(35);
    }

    [Fact]
    public void Property_IsNotNull_Should_Exclude_Null_Values()
    {
        var spec = Property<Person>.For(p => p.Name).IsNotNull();
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(4);
    }

    [Fact]
    public void Property_In_Should_Match_Any_Value_In_Collection()
    {
        var spec = Property<Person>.For(p => p.Age).In(17, 22, 25);
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new[] { "Alice", "Bob", "Diana" }, ignoreOrder: true);
    }

    [Fact]
    public void Property_NotIn_Should_Exclude_Values_In_Collection()
    {
        var spec = Property<Person>.For(p => p.Age).NotIn(17, 22, 25);
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new string?[] { "Charlie", null }, ignoreOrder: true);
    }

    [Fact]
    public void Property_GreaterThan_Should_Compare_Values()
    {
        var spec = Property<Person>.For(p => p.Age).GreaterThan(25);
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new string?[] { "Charlie", null }, ignoreOrder: true);
    }

    [Fact]
    public void Property_InRange_Should_Match_Values_In_Range()
    {
        var spec = Property<Person>.For(p => p.Age).InRange(20, 30);
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new[] { "Alice", "Charlie", "Diana" }, ignoreOrder: true);
    }

    [Fact]
    public void StringProperty_StartsWith_Should_Match_Prefix()
    {
        var spec = Property<Person>.For(p => p.Email).StartsWith("alice");
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Alice");
    }

    [Fact]
    public void StringProperty_Contains_Should_Match_Substring()
    {
        var spec = Property<Person>.For(p => p.Email).Contains("example");
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new[] { "Alice", "Diana" }, ignoreOrder: true);
    }

    [Fact]
    public void StringProperty_EndsWith_Should_Match_Suffix()
    {
        var spec = Property<Person>.For(p => p.Email).EndsWith(".org");
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Bob");
    }

    [Fact]
    public void StringProperty_IsNullOrEmpty_Should_Match_Empty_Strings()
    {
        var spec = Property<Person>.For(p => p.Email).IsNullOrEmpty();
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Charlie");
    }

    [Fact]
    public void StringProperty_HasContent_Should_Match_NonEmpty_Strings()
    {
        var spec = Property<Person>.For(p => p.Email).HasContent();
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(4);
    }

    [Fact]
    public void StringProperty_HasLengthBetween_Should_Match_Length_Range()
    {
        ISpecification<Person> spec = Property<Person>.For(p => p.Email).HasLengthBetween(10, 20);
        List<Person> result = _people.Where(spec.ToExpression()).ToList();
        // alice@example.com=17, bob@test.org=12, diana@example.com=17 all within 10-20
        result.Select(p => p.Name).ShouldBe(new[] { "Alice", "Bob", "Diana" }, ignoreOrder: true);
    }

    [Fact]
    public void Property_Matches_Should_Apply_Custom_Predicate()
    {
        var spec = Property<Person>.For(p => p.Age).Matches(age => age % 5 == 0);
        var result = _people.Where(spec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new string?[] { "Alice", "Charlie", null }, ignoreOrder: true);
    }

    [Fact]
    public void Property_Combined_With_Other_Specs_Should_Work()
    {
        var ageSpec = Property<Person>.For(p => p.Age).GreaterThanOrEqual(18);
        var emailSpec = Property<Person>.For(p => p.Email).Contains("example");

        var combinedSpec = ageSpec.And(emailSpec);
        var result = _people.Where(combinedSpec.ToExpression()).ToList();
        result.Select(p => p.Name).ShouldBe(new[] { "Alice", "Diana" }, ignoreOrder: true);
    }
}
