using Shouldly;

namespace Masterly.Specification.UnitTests;

public class DiagnosticsTests
{
    private record Order(int Id, decimal Total, string Status, bool IsPriority);

    [Fact]
    public void Explain_Should_Generate_Readable_Description()
    {
        var spec = new ExpressionSpecification<Order>(o => o.Total > 100);
        var explanation = spec.Explain();

        explanation.ShouldContain("Total");
        explanation.ShouldContain("100");
    }

    [Fact]
    public void Explain_Should_Handle_Complex_Specifications()
    {
        var highValue = new ExpressionSpecification<Order>(o => o.Total > 100);
        var priority = new ExpressionSpecification<Order>(o => o.IsPriority);
        var combinedSpec = highValue.And(priority);

        var explanation = combinedSpec.Explain();

        explanation.ShouldContain("AND");
    }

    [Fact]
    public void Evaluate_Should_Return_Detailed_Results_When_Satisfied()
    {
        var spec = new ExpressionSpecification<Order>(o => o.Total > 100);
        var order = new Order(1, 150m, "Pending", false);

        var result = spec.Evaluate(order);

        result.IsSatisfied.ShouldBeTrue();
        result.Summary.ShouldBe("PASSED");
    }

    [Fact]
    public void Evaluate_Should_Return_Detailed_Results_When_Not_Satisfied()
    {
        var spec = new ExpressionSpecification<Order>(o => o.Total > 100);
        var order = new Order(1, 50m, "Pending", false);

        var result = spec.Evaluate(order);

        result.IsSatisfied.ShouldBeFalse();
        result.Summary.ShouldBe("FAILED");
    }

    [Fact]
    public void Evaluate_Should_Track_Multiple_Conditions()
    {
        var highValue = new ExpressionSpecification<Order>(o => o.Total > 100);
        var priority = new ExpressionSpecification<Order>(o => o.IsPriority);
        var combinedSpec = highValue.And(priority);

        var order = new Order(1, 150m, "Pending", false);
        var result = combinedSpec.Evaluate(order);

        result.IsSatisfied.ShouldBeFalse();
        result.GetPassedConditions().ShouldNotBeEmpty();
        result.GetFailureReasons().ShouldNotBeEmpty();
    }

    [Fact]
    public void GetDetailedResult_Should_Return_Formatted_Output()
    {
        var spec = new ExpressionSpecification<Order>(o => o.Total > 100);
        var order = new Order(1, 150m, "Pending", false);

        var detailedResult = spec.GetDetailedResult(order);

        detailedResult.ShouldContain("Result: PASSED");
        detailedResult.ShouldContain("Details:");
    }

    [Fact]
    public void Explain_Should_Handle_Not_Specification()
    {
        var spec = new ExpressionSpecification<Order>(o => o.IsPriority).Not();
        var explanation = spec.Explain();

        explanation.ShouldContain("NOT");
    }

    [Fact]
    public void Explain_Should_Handle_Or_Specification()
    {
        var highValue = new ExpressionSpecification<Order>(o => o.Total > 1000);
        var priority = new ExpressionSpecification<Order>(o => o.IsPriority);
        var spec = highValue.Or(priority);

        var explanation = spec.Explain();

        explanation.ShouldContain("OR");
    }
}