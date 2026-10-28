using System.Globalization;

namespace Ambio.Web.Components.Shared;

/// <summary>How dates are written in the UI, in English whatever the server culture.</summary>
public static class DisplayDates
{
    /// <summary>"28 Aug 2026".</summary>
    public static string ToDisplayDate(this DateTime date) => date.ToString("d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>"28 Aug", where space is short, such as the list cards.</summary>
    public static string ToShortDisplayDate(this DateTime date) => date.ToString("d MMM", CultureInfo.InvariantCulture);

    /// <summary>"28 Aug 2026".</summary>
    public static string ToDisplayDate(this DateOnly date) => date.ToString("d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>"28 Aug", where space is short, such as the list cards.</summary>
    public static string ToShortDisplayDate(this DateOnly date) => date.ToString("d MMM", CultureInfo.InvariantCulture);

    /// <summary>"Tuesday 27 October 2026", such as the day in the dashboard's header.</summary>
    public static string ToLongDisplayDate(this DateOnly date) => date.ToString("dddd d MMMM yyyy", CultureInfo.InvariantCulture);
}
