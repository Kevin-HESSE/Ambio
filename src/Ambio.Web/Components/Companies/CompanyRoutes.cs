using Ambio.Web.Components.Shared;

namespace Ambio.Web.Components.Companies;

/// <summary>Base-relative URLs of the company pages, for links and redirects.</summary>
public static class CompanyRoutes
{
    public const string List = "companies";

    /// <summary>The list with the archived companies too (<see cref="ShowArchived"/>).</summary>
    public const string ListWithArchived = $"{List}?{ShowArchived.Query}=true";

    public const string New = "companies/new";

    /// <summary>Value of the Offers tab of the company page.</summary>
    public const string OffersTab = "offers";

    /// <summary>Value of the Applications tab of the company page.</summary>
    public const string ApplicationsTab = "applications";

    public static string Details(string slug) => $"companies/{slug}";

    /// <summary>The company page showing one of its tabs, such as <see cref="OffersTab"/>.</summary>
    public static string Details(string slug, string tab) => $"companies/{slug}?tab={tab}";

    public static string Edit(string slug) => $"companies/{slug}/edit";
}
