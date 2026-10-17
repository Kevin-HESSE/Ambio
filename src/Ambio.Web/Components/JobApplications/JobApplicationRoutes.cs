namespace Ambio.Web.Components.JobApplications;

/// <summary>Base-relative URLs of the application pages, for links and redirects. An application is reached by its
/// short id and its slug, <c>applications/482913-backend-developer-acme</c> (ADR 0014).</summary>
public static class JobApplicationRoutes
{
    public const string List = "applications";

    public const string New = "applications/new";

    /// <summary>The new application page with this offer already chosen.</summary>
    public static string NewForOffer(Guid offerId) => $"applications/new?offer={offerId}";

    /// <summary>The new application page, spontaneous, with this company already chosen.</summary>
    public static string NewForCompany(string companySlug) => $"applications/new?company={companySlug}";

    public static string Details(string shortId, string slug) => $"applications/{Key(shortId, slug)}";

    /// <summary>The part of the URL that names the application: its short id, a <c>-</c>, then its slug.</summary>
    public static string Key(string shortId, string slug) => $"{shortId}-{slug}";

    /// <summary>The short id at the start of a <see cref="Key"/>: everything before the first <c>-</c>, which a short
    /// id never contains, or the whole key when it has no slug.</summary>
    public static string ShortIdOf(string key) => key.Split('-', 2)[0];
}
