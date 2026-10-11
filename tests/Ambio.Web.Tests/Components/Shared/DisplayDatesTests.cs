using Ambio.Web.Components.Shared;

namespace Ambio.Web.Tests.Components.Shared;

public class DisplayDatesTests
{
    private static readonly DateTime Date = new(2026, 8, 28, 22, 30, 0, DateTimeKind.Utc);

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
}
