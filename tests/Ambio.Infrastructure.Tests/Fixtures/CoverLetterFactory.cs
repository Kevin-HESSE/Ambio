using Ambio.Domain.CoverLetters;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Cover letters to save straight through the DbContext, without the service.</summary>
public static class CoverLetterFactory
{
    /// <summary>Letters of the application, written on several lines. Save their application first.</summary>
    public static Faker<CoverLetter> GetGenerator(Guid jobApplicationId) =>
        new Faker<CoverLetter>()
            .RuleFor(l => l.JobApplicationId, jobApplicationId)
            .RuleFor(l => l.Title, faker => $"Cover letter · {faker.Company.CompanyName()}")
            .RuleFor(l => l.Content, faker => faker.Lorem.Paragraphs(3));
}
