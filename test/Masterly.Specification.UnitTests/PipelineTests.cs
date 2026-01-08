using Shouldly;

namespace Masterly.Specification.UnitTests;

public class PipelineTests
{
    private record Invoice(int Id, decimal Amount, string Type, bool IsApproved);

    private readonly IQueryable<Invoice> _invoices;

    public PipelineTests()
    {
        _invoices = new[]
        {
            new Invoice(1, 500m, "Standard", true),
            new Invoice(2, 1500m, "Standard", true),
            new Invoice(3, 2500m, "Premium", true),
            new Invoice(4, 100m, "Standard", false),
            new Invoice(5, 5000m, "Premium", false)
        }.AsQueryable();
    }

    [Fact]
    public void OnlyWhen_Should_Apply_Spec_When_Condition_Is_True()
    {
        var highValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount > 1000);
        var filterEnabled = true;

        var spec = highValueSpec.OnlyWhen(filterEnabled);

        var result = _invoices.Where(spec.ToExpression()).ToList();
        result.Select(i => i.Id).ShouldBe(new[] { 2, 3, 5 }, ignoreOrder: true);
    }

    [Fact]
    public void OnlyWhen_Should_Pass_All_When_Condition_Is_False()
    {
        var highValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount > 1000);
        var filterEnabled = false;

        var spec = highValueSpec.OnlyWhen(filterEnabled);

        var result = _invoices.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(5); // All pass
    }

    [Fact]
    public void SkipWhen_Should_Pass_All_When_Condition_Is_True()
    {
        var approvedSpec = new ExpressionSpecification<Invoice>(i => i.IsApproved);
        var skipApprovalCheck = true;

        var spec = approvedSpec.SkipWhen(skipApprovalCheck);

        var result = _invoices.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(5);
    }

    [Fact]
    public void SkipWhen_Should_Apply_Spec_When_Condition_Is_False()
    {
        var approvedSpec = new ExpressionSpecification<Invoice>(i => i.IsApproved);
        var skipApprovalCheck = false;

        var spec = approvedSpec.SkipWhen(skipApprovalCheck);

        var result = _invoices.Where(spec.ToExpression()).ToList();
        result.Select(i => i.Id).ShouldBe(new[] { 1, 2, 3 }, ignoreOrder: true);
    }

    [Fact]
    public void Chain_Should_Combine_Multiple_Specifications()
    {
        var approved = new ExpressionSpecification<Invoice>(i => i.IsApproved);
        var highValue = new ExpressionSpecification<Invoice>(i => i.Amount > 1000);
        var premium = new ExpressionSpecification<Invoice>(i => i.Type == "Premium");

        var spec = approved.Chain(highValue, premium);

        var result = _invoices.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(3); // Only invoice 3 matches all
    }

    [Fact]
    public void AsOptional_Should_Pass_When_Spec_Is_Null()
    {
        ISpecification<Invoice>? nullSpec = null;
        var spec = nullSpec.AsOptional();

        var result = _invoices.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(5);
    }

    [Fact]
    public void AsOptional_Should_Apply_When_Spec_Is_Not_Null()
    {
        ISpecification<Invoice> approvedSpec = new ExpressionSpecification<Invoice>(i => i.IsApproved);
        var spec = approvedSpec.AsOptional();

        var result = _invoices.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void When_Otherwise_Should_Apply_Different_Specs_Based_On_Condition()
    {
        var highValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount > 2000);
        var approvedSpec = new ExpressionSpecification<Invoice>(i => i.IsApproved);

        // If premium, require high value; otherwise require approved
        var conditionalSpec = new ConditionalSpecification<Invoice>(
            i => i.Type == "Premium",
            highValueSpec,
            approvedSpec);

        var result = _invoices.Where(i => conditionalSpec.IsSatisfiedBy(i)).ToList();
        result.Select(i => i.Id).ShouldBe(new[] { 1, 2, 3, 5 }, ignoreOrder: true);
    }

    [Fact]
    public void ConditionalBuilder_Otherwise_Should_Apply_Fallback_Spec()
    {
        var highValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount > 2000);
        var lowValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount < 200);

        var spec = PipelineExtensions.When<Invoice>(
            i => i.Type == "Premium",
            highValueSpec)
            .Otherwise(lowValueSpec);

        var result = _invoices.Where(i => spec.IsSatisfiedBy(i)).ToList();
        result.Select(i => i.Id).ShouldBe(new[] { 3, 4, 5 }, ignoreOrder: true);
    }

    [Fact]
    public void ConditionalBuilder_OtherwisePass_Should_Always_Pass_For_Non_Matching_Condition()
    {
        var highValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount > 2000);

        var spec = PipelineExtensions.When<Invoice>(
            i => i.Type == "Premium",
            highValueSpec)
            .OtherwisePass();

        var result = _invoices.Where(i => spec.IsSatisfiedBy(i)).ToList();
        result.Select(i => i.Id).ShouldBe(new[] { 1, 2, 3, 4, 5 }, ignoreOrder: true);
    }

    [Fact]
    public void ConditionalBuilder_OtherwiseFail_Should_Fail_For_Non_Matching_Condition()
    {
        var anySpec = new AnySpecification<Invoice>();

        var spec = PipelineExtensions.When<Invoice>(
            i => i.Type == "Premium",
            anySpec)
            .OtherwiseFail();

        var result = _invoices.Where(i => spec.IsSatisfiedBy(i)).ToList();
        result.Select(i => i.Id).ShouldBe(new[] { 3, 5 }, ignoreOrder: true);
    }

    [Fact]
    public void OrElse_Should_Short_Circuit_On_First_Match()
    {
        var approvedSpec = new ExpressionSpecification<Invoice>(i => i.IsApproved);
        var premiumSpec = new ExpressionSpecification<Invoice>(i => i.Type == "Premium");

        var spec = approvedSpec.OrElse(() => premiumSpec);

        var result = _invoices.Where(spec.ToExpression()).ToList();
        result.Select(i => i.Id).ShouldBe(new[] { 1, 2, 3, 5 }, ignoreOrder: true);
    }

    [Fact]
    public void AndThen_Should_Short_Circuit_On_First_Failure()
    {
        var approvedSpec = new ExpressionSpecification<Invoice>(i => i.IsApproved);
        var highValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount > 1000);

        var spec = approvedSpec.AndThen(() => highValueSpec);

        var result = _invoices.Where(spec.ToExpression()).ToList();
        result.Select(i => i.Id).ShouldBe(new[] { 2, 3 }, ignoreOrder: true);
    }
}
