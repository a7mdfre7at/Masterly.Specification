using Shouldly;

namespace Masterly.Specification.UnitTests;

public class TemporalTests
{
    private record Event(string Name, DateTime StartDate, DateTime? EndDate);

    private readonly IQueryable<Event> _events;

    public TemporalTests()
    {
        _events = new[]
        {
            new Event("Past Event", new DateTime(2024, 1, 15), new DateTime(2024, 1, 20)),
            new Event("Current Event", new DateTime(2024, 6, 10), new DateTime(2024, 6, 20)),
            new Event("Future Event", new DateTime(2024, 12, 1), null),
            new Event("Old Event", new DateTime(2020, 5, 1), new DateTime(2020, 5, 5)),
            new Event("Recent Event", new DateTime(2024, 6, 14), null),
            new Event("Weekend Event", new DateTime(2024, 6, 15), null) // Saturday
        }.AsQueryable();
    }

    [Fact]
    public void Before_Should_Match_Dates_Before_Specified_Date()
    {
        var spec = Temporal<Event>.For(e => e.StartDate).Before(new DateTime(2024, 6, 1));

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldBe(new[] { "Past Event", "Old Event" }, ignoreOrder: true);
    }

    [Fact]
    public void After_Should_Match_Dates_After_Specified_Date()
    {
        var spec = Temporal<Event>.For(e => e.StartDate).After(new DateTime(2024, 6, 14));

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldBe(new[] { "Future Event", "Weekend Event" }, ignoreOrder: true);
    }

    [Fact]
    public void Between_Should_Match_Dates_In_Range()
    {
        var spec = Temporal<Event>.For(e => e.StartDate)
            .Between(new DateTime(2024, 6, 1), new DateTime(2024, 6, 30));

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldBe(new[] { "Current Event", "Recent Event", "Weekend Event" }, ignoreOrder: true);
    }

    [Fact]
    public void InYear_Should_Match_Events_In_Specified_Year()
    {
        var spec = Temporal<Event>.For(e => e.StartDate).InYear(2020);

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Old Event");
    }

    [Fact]
    public void InMonth_Should_Match_Events_In_Specified_Month()
    {
        var spec = Temporal<Event>.For(e => e.StartDate).InMonth(2024, 6);

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldBe(new[] { "Current Event", "Recent Event", "Weekend Event" }, ignoreOrder: true);
    }

    [Fact]
    public void OnDayOfWeek_Should_Match_Events_On_Specified_Day()
    {
        var spec = Temporal<Event>.For(e => e.StartDate).OnDayOfWeek(DayOfWeek.Saturday);

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Weekend Event");
    }

    [Fact]
    public void OnWeekend_Should_Match_Saturday_And_Sunday()
    {
        var spec = Temporal<Event>.For(e => e.StartDate).OnWeekend();

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldContain("Weekend Event");
    }

    [Fact]
    public void OnWeekday_Should_Exclude_Weekend_Events()
    {
        var spec = Temporal<Event>.For(e => e.StartDate).OnWeekday();

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldNotContain("Weekend Event");
    }

    [Fact]
    public void NullableDateTime_HasValue_Should_Match_NonNull_Dates()
    {
        var spec = Temporal<Event>.For(e => e.EndDate).HasValue();

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldBe(new[] { "Past Event", "Current Event", "Old Event" }, ignoreOrder: true);
    }

    [Fact]
    public void NullableDateTime_IsNull_Should_Match_Null_Dates()
    {
        var spec = Temporal<Event>.For(e => e.EndDate).IsNull();

        var result = _events.Where(spec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldBe(new[] { "Future Event", "Recent Event", "Weekend Event" }, ignoreOrder: true);
    }

    [Fact]
    public void TimeBetween_Should_Match_Time_Of_Day()
    {
        var events = new[]
        {
            new Event("Morning", new DateTime(2024, 6, 15, 9, 30, 0), null),
            new Event("Afternoon", new DateTime(2024, 6, 15, 14, 0, 0), null),
            new Event("Evening", new DateTime(2024, 6, 15, 19, 30, 0), null)
        }.AsQueryable();

        var spec = Temporal<Event>.For(e => e.StartDate)
            .TimeBetween(TimeSpan.FromHours(9), TimeSpan.FromHours(17));

        var result = events.Where(spec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldBe(new[] { "Morning", "Afternoon" }, ignoreOrder: true);
    }

    [Fact]
    public void Temporal_Specs_Should_Combine_With_Other_Specs()
    {
        var in2024 = Temporal<Event>.For(e => e.StartDate).InYear(2024);
        var hasEndDate = Temporal<Event>.For(e => e.EndDate).HasValue();

        var combinedSpec = in2024.And(hasEndDate);

        var result = _events.Where(combinedSpec.ToExpression()).ToList();
        result.Select(e => e.Name).ShouldBe(new[] { "Past Event", "Current Event" }, ignoreOrder: true);
    }
}