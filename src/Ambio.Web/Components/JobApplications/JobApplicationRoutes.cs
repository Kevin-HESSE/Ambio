using System.Globalization;

using Ambio.Application.JobApplications.Dtos;
using Ambio.Web.Components.Shared;

using Microsoft.AspNetCore.WebUtilities;

namespace Ambio.Web.Components.JobApplications;

/// <summary>Base-relative URLs of the application pages, for links and redirects. An application is reached by its
/// short id and its slug (<see cref="ShortIdKey"/>), <c>applications/482913-backend-developer-acme</c> (ADR 0014).</summary>
public static class JobApplicationRoutes
{
    public const string List = "applications";

    /// <summary>Query string key of the search in the list.</summary>
    public const string SearchQuery = "q";

    /// <summary>Query string key of the status filter, such as <c>?status=interview</c>.</summary>
    public const string StatusQuery = "status";

    /// <summary>Query string key of the company filter: its slug.</summary>
    public const string CompanyQuery = "company";

    /// <summary>Query string key of the kind filter: <c>offer</c> or <c>spontaneous</c>.</summary>
    public const string KindQuery = "kind";

    /// <summary>Query string key of the number of applications shown, raised by "Load more".</summary>
    public const string ShownQuery = "shown";

    public const string New = "applications/new";

    /// <summary>The new application page with this offer already chosen.</summary>
    public static string NewForOffer(string offerShortId) => $"applications/new?offer={offerShortId}";

    /// <summary>The new application page, spontaneous, with this company already chosen.</summary>
    public static string NewForCompany(string companySlug) => $"applications/new?company={companySlug}";

    /// <summary>The list with the criteria of <paramref name="filter"/>, and <paramref name="shown"/> applications when
    /// more than the first page; the criteria left empty stay out of the URL.</summary>
    public static string Filtered(JobApplicationFilter filter, int? shown = null) =>
        QueryHelpers.AddQueryString(List, new Dictionary<string, string?>
        {
            [SearchQuery] = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim(),
            [StatusQuery] = filter.Status is { } status ? QueryValue.Of(status) : null,
            [CompanyQuery] = filter.CompanySlug,
            [KindQuery] = filter.Kind is { } kind ? QueryValue.Of(kind) : null,
            [ShowArchived.Query] = filter.IncludeArchived ? "true" : null,
            [ShownQuery] = shown?.ToString(CultureInfo.InvariantCulture),
        }.Where(pair => pair.Value is not null));

    public static string Details(string shortId, string slug) => $"applications/{ShortIdKey.Of(shortId, slug)}";

    public static string Edit(string shortId, string slug) => $"applications/{ShortIdKey.Of(shortId, slug)}/edit";
}
