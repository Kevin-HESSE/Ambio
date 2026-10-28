using Ambio.Web.Components.Shared;

namespace Ambio.Web.Components.Offers;

/// <summary>Base-relative URLs of the offer pages, for links and redirects. An offer is reached by its short id and
/// its slug (<see cref="ShortIdKey"/>), <c>offers/482913-backend-developer-kelvio</c> (ADR 0025).</summary>
public static class OfferRoutes
{
    public const string List = "offers";

    /// <summary>The list with the archived offers too (<see cref="ShowArchived"/>).</summary>
    public const string ListWithArchived = $"{List}?{ShowArchived.Query}=true";

    public const string New = "offers/new";

    public static string Details(string shortId, string slug) => $"offers/{ShortIdKey.Of(shortId, slug)}";

    public static string Edit(string shortId, string slug) => $"offers/{ShortIdKey.Of(shortId, slug)}/edit";
}
