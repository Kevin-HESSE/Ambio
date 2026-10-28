using Ambio.Application.CoverLetters.Dtos;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Cover letter editor forms, as sent to <c>ICoverLetterService</c>.</summary>
public static class CoverLetterInputFactory
{
    public static Faker<CoverLetterInput> GetGenerator() =>
        new Faker<CoverLetterInput>()
            .RuleFor(l => l.Title, faker => $"Cover letter · {faker.Company.CompanyName()}")
            .RuleFor(l => l.Content, faker => faker.Lorem.Paragraphs(3));
}
