namespace Ambio.Web.Components.JobApplications;

/// <summary>Base-relative URLs of the application pages, for links and redirects.</summary>
public static class JobApplicationRoutes
{
    public const string List = "applications";

    public const string New = "applications/new";

    /// <summary>The new application page with this offer already chosen.</summary>
    public static string NewForOffer(Guid offerId) => $"applications/new?offer={offerId}";

    /// <summary>The new application page, spontaneous, with this company already chosen.</summary>
    public static string NewForCompany(string companySlug) => $"applications/new?company={companySlug}";
}
