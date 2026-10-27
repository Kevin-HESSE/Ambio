using Ambio.Web.Components.Shared;

namespace Ambio.Web.Tests.Components.Shared;

public class DisplayDatesTests
{
    private static readonly DateTime Date = new(2026, 8, 28, 22, 30, 0, DateTimeKind.Utc);
    private static readonly DateOnly Day = new(2026, 9, 3);

    [Fact]
    public void ToDisplayDate_WritesDayShortMonthAndYear()
    {
        Assert.Equal("28 Aug 2026", Date.ToDisplayDate());
    }

    [Fact]
    public void ToShortDisplayDate_WritesDayAndShortMonth()
    {
        Assert.Equal("28 Aug", Date.ToShortDisplayDate());
    }

    [Fact]
    public void ToDisplayDate_OfDay_WritesDayShortMonthAndYear()
    {
        Assert.Equal("3 Sep 2026", Day.ToDisplayDate());
    }

    [Fact]
    public void ToShortDisplayDate_OfDay_WritesDayAndShortMonth()
    {
        Assert.Equal("3 Sep", Day.ToShortDisplayDate());
    }

    [Fact]
    public void ToLongDisplayDate_OfDay_WritesWeekdayDayMonthAndYear()
    {
        Assert.Equal("Thursday 3 September 2026", Day.ToLongDisplayDate());
    }
}
