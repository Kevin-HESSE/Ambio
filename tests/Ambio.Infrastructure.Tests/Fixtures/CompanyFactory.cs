using Ambio.Application.Common;
using Ambio.Domain.Companies;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Companies to save straight through the DbContext, without the service.</summary>
public static class CompanyFactory
{
    public const string SlugFallback = "company";

    /// <summary>A company with only the required fields; its slug is the one of its name, without suffix.</summary>
    public static Company New(string name, CompanyKind kind = CompanyKind.Employer) => new()
    {
        Name = name,
        Slug = Slug.From(name, SlugFallback),
        Kind = kind,
    };
}
