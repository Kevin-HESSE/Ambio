namespace Ambio.Web.Components.Offers;

/// <summary>Base-relative URLs of the offer pages, for links and redirects.</summary>
public static class OfferRoutes
{
    public const string List = "offers";

    public const string New = "offers/new";

    public static string Details(Guid id) => $"offers/{id}";
}
