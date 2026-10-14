using Ambio.Domain.Offers;

namespace Ambio.Domain.Tests.Offers;

public class JobOfferTitlesTests
{
    [Theory]
    [MemberData(nameof(SimilarTitles))]
    public void AreSimilar_WithSimilarTitles_ReturnsTrue(string title, string otherTitle)
    {
        Assert.True(JobOfferTitles.AreSimilar(title, otherTitle));
        Assert.True(JobOfferTitles.AreSimilar(otherTitle, title));
    }

    [Theory]
    [MemberData(nameof(DifferentTitles))]
    public void AreSimilar_WithDifferentTitles_ReturnsFalse(string title, string otherTitle)
    {
        Assert.False(JobOfferTitles.AreSimilar(title, otherTitle));
        Assert.False(JobOfferTitles.AreSimilar(otherTitle, title));
    }

    public static TheoryData<string, string> SimilarTitles => new()
    {
        { "Backend Developer", "Backend Developer" },
        { "Full-stack Engineer", "full stack engineer" },
        { "Full Stack Engineer", "Engineer, full stack" },
        { "Développeur Backend", "developpeur backend" },
        { "Senior Backend Developer", "Backend Developer" },
        { "Développeur .NET (H/F)", "Développeur .NET" },
    };

    public static TheoryData<string, string> DifferentTitles => new()
    {
        { "Backend Developer", "Frontend Developer" },
        { "Data Engineer", "Software Engineer" },
        { "Lead Backend Developer (H/F)", "Developer" },
        { "!!!", "!!!" },
    };
}
