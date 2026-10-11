namespace Ambio.Application.Common;

public static class StringExtensions
{
    /// <summary>The value trimmed, or null when it is null, empty or only whitespace: how optional text fields
    /// are stored.</summary>
    public static string? NullIfBlank(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
