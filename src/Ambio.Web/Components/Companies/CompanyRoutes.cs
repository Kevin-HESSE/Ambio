namespace Ambio.Web.Components.Companies;

/// <summary>Base-relative URLs of the company pages, for links and redirects.</summary>
public static class CompanyRoutes
{
    public const string List = "companies";

    public const string New = "companies/new";

    public static string Details(string slug) => $"companies/{slug}";

    public static string Edit(string slug) => $"companies/{slug}/edit";
}
