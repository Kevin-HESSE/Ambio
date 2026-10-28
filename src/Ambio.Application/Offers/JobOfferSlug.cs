using Ambio.Application.Common;

namespace Ambio.Application.Offers;

/// <summary>The readable part of an offer URL (ADR 0025): it isn't stored, but computed from the title and the
/// company name.</summary>
public static class JobOfferSlug
{
    // The slug of an offer whose title and company keep no letter or digit.
    private const string Fallback = "offer";

    /// <summary>The slug of the title followed by the company name, such as <c>backend-developer-kelvio</c>.</summary>
    public static string From(string title, string companyName) => Slug.From($"{title} {companyName}", Fallback);
}
