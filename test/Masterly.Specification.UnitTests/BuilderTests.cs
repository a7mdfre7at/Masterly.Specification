using Shouldly;

namespace Masterly.Specification.UnitTests;

public class BuilderTests
{
    private record User(string Name, int Age, string Email, bool IsActive);

    private readonly IQueryable<User> _users;

    public BuilderTests()
    {
        _users = new[]
        {
            new User("Alice", 25, "alice@example.com", true),
            new User("Bob", 17, "bob@example.com", true),
            new User("Charlie", 30, "charlie@test.com", false),
            new User("Diana", 22, "diana@example.com", true),
            new User("Eve", 35, "eve@company.com", false)
        }.AsQueryable();
    }

    [Fact]
    public void Spec_For_Should_Create_Builder_And_Build_Specification()
    {
        var spec = Spec.For<User>()
            .Where(u => u.Age >= 18)
            .And(u => u.IsActive)
            .Build();

        var result = _users.Where(spec.ToExpression()).ToList();
        result.Select(u => u.Name).ShouldBe(new[] { "Alice", "Diana" }, ignoreOrder: true);
    }

    [Fact]
    public void Spec_Where_Should_Create_Simple_Specification()
    {
        var spec = Spec.Where<User>(u => u.Age >= 18);

        var result = _users.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(4);
    }

    [Fact]
    public void Builder_Should_Support_Or_Conditions()
    {
        var spec = Spec.For<User>()
            .Where(u => u.Age < 18)
            .Or(u => u.Email.Contains("company"))
            .Build();

        var result = _users.Where(spec.ToExpression()).ToList();
        result.Select(u => u.Name).ShouldBe(new[] { "Bob", "Eve" }, ignoreOrder: true);
    }

    [Fact]
    public void Builder_Should_Support_AndNot_Conditions()
    {
        var spec = Spec.For<User>()
            .Where(u => u.IsActive)
            .AndNot(u => u.Age < 20)
            .Build();

        var result = _users.Where(spec.ToExpression()).ToList();
        result.Select(u => u.Name).ShouldBe(new[] { "Alice", "Diana" }, ignoreOrder: true);
    }

    [Fact]
    public void Builder_Should_Support_Not()
    {
        var spec = Spec.For<User>()
            .Where(u => u.IsActive)
            .Not()
            .Build();

        var result = _users.Where(spec.ToExpression()).ToList();
        result.Select(u => u.Name).ShouldBe(new[] { "Charlie", "Eve" }, ignoreOrder: true);
    }

    [Fact]
    public void Builder_Should_Support_Xor()
    {
        var spec = Spec.For<User>()
            .Where(u => u.Age >= 25)
            .Xor(u => u.IsActive)
            .Build();

        var result = _users.Where(spec.ToExpression()).ToList();
        result.Select(u => u.Name).ShouldBe(new[] { "Bob", "Charlie", "Diana", "Eve" }, ignoreOrder: true);
    }

    [Fact]
    public void Builder_Should_Support_Existing_Specifications()
    {
        ISpecification<User> adultSpec = new ExpressionSpecification<User>(u => u.Age >= 18);

        ISpecification<User> spec = Spec.For<User>()
            .Where(u => u.IsActive)
            .And(adultSpec)
            .Build();

        List<User> result = _users.Where(spec.ToExpression()).ToList();
        result.Select(u => u.Name).ShouldBe(new[] { "Alice", "Diana" }, ignoreOrder: true);
    }

    [Fact]
    public void Builder_Should_Support_Grouped_Conditions()
    {
        ISpecification<User> spec = Spec.For<User>()
            .Where(u => u.Age >= 18)
            .Group(g => g.Where(u => u.IsActive).Or(u => u.Email.Contains("test")))
            .Build();

        List<User> result = _users.Where(spec.ToExpression()).ToList();
        // Age >= 18: Alice(25), Charlie(30), Diana(22), Eve(35)
        // Then (IsActive OR Email contains "test"):
        // Alice: Active=true -> pass
        // Charlie: Active=false, Email=charlie@test.com contains "test" -> pass
        // Diana: Active=true -> pass
        // Eve: Active=false, Email=eve@company.com no "test" -> fail
        result.Select(u => u.Name).ShouldBe(new[] { "Alice", "Charlie", "Diana" }, ignoreOrder: true);
    }

    [Fact]
    public void Spec_Any_Should_Return_All_Items()
    {
        var spec = Spec.Any<User>();
        var result = _users.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(5);
    }

    [Fact]
    public void Spec_None_Should_Return_No_Items()
    {
        var spec = Spec.None<User>();
        var result = _users.Where(spec.ToExpression()).ToList();
        result.ShouldBeEmpty();
    }
}
