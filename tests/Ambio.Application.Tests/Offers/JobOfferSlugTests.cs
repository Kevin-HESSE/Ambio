using Ambio.Application.Offers;

namespace Ambio.Application.Tests.Offers;

public class JobOfferSlugTests
{
    private const string Fallback = "offer";

    [Fact]
    public void From_TitleAndCompany_JoinsThemInOneSlug()
    {
        Assert.Equal("backend-developer-kelvio", JobOfferSlug.From("Backend Developer", "Kelvio"));
    }

    [Fact]
    public void From_WithoutLetterOrDigitLeft_ReturnsFallback()
    {
        Assert.Equal(Fallback, JobOfferSlug.From("—", "&"));
    }
}
