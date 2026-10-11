using System.Globalization;

namespace Ambio.Web.Components.Shared;

/// <summary>How dates are written in the UI, in English whatever the server culture.</summary>
public static class DisplayDates
{
    /// <summary>"28 Aug 2026".</summary>
    public static string ToDisplayDate(this DateTime date) => date.ToString("d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>"28 Aug", where space is short, such as the list cards.</summary>
    public static string ToShortDisplayDate(this DateTime date) => date.ToString("d MMM", CultureInfo.InvariantCulture);
}
