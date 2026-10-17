using Ambio.Application.Common;

namespace Ambio.Application.JobApplications;

/// <summary>The readable part of an application URL (ADR 0014): it isn't stored, but computed from the title and the
/// company name.</summary>
public static class JobApplicationSlug
{
    // The slug of an application whose title and company keep no letter or digit.
    private const string Fallback = "application";

    /// <summary>The slug of the title followed by the company name, such as <c>backend-developer-acme</c>.</summary>
    public static string From(string title, string companyName) => Slug.From($"{title} {companyName}", Fallback);
}
