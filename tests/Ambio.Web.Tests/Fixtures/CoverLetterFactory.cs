using Ambio.Application.CoverLetters.Dtos;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Cover letters of an application, as returned by the mocked <c>ICoverLetterService</c>.</summary>
public static class CoverLetterFactory
{
    public static Faker<CoverLetterDto> GetGenerator() =>
        new Faker<CoverLetterDto>()
            .WithRecord()
            .RuleFor(l => l.Id, Guid.CreateVersion7)
            .RuleFor(l => l.Title, f => $"Cover letter · {f.Company.CompanyName()}")
            .RuleFor(l => l.Content, f => f.Lorem.Paragraphs(3))
            .RuleFor(l => l.EditedAt, f => f.Date.RecentOffset().UtcDateTime);
}
