namespace Ambio.Web.Components.Shared;

/// <summary>The part of a URL that names an application or an offer: its short id, a <c>-</c>, then its slug, such as
/// <c>482913-backend-developer-acme</c> (ADR 0014).</summary>
public static class ShortIdKey
{
    /// <summary>The key of the short id and the slug.</summary>
    public static string Of(string shortId, string slug) => $"{shortId}-{slug}";

    /// <summary>The short id at the start of a key: everything before the first <c>-</c>, which a short id never
    /// contains, or the whole key when it has no slug.</summary>
    public static string ShortIdOf(string key) => key.Split('-', 2)[0];
}
