namespace Ambio.Web.Components.Shared;

/// <summary>An enum value in a query string, written by its name in lowercase, such as <c>?status=interview</c>.</summary>
public static class QueryValue
{
    /// <summary>The value as written in a URL.</summary>
    public static string Of<TEnum>(TEnum value) where TEnum : struct, Enum => value.ToString().ToLowerInvariant();

    /// <summary>The enum value named <paramref name="value"/>, ignoring case, or null for any other text: a number
    /// isn't accepted, even one that matches a value.</summary>
    public static TEnum? Parse<TEnum>(string? value) where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(candidate => (TEnum?)candidate)
            .FirstOrDefault(candidate => string.Equals(candidate.ToString(), value, StringComparison.OrdinalIgnoreCase));
}
