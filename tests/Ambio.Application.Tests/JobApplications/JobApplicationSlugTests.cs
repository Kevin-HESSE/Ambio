using Ambio.Application.JobApplications;

namespace Ambio.Application.Tests.JobApplications;

public class JobApplicationSlugTests
{
    private const string Fallback = "application";

    [Fact]
    public void From_TitleAndCompany_JoinsThemInOneSlug()
    {
        Assert.Equal("backend-developer-brightwave", JobApplicationSlug.From("Backend Developer", "Brightwave"));
    }

    [Fact]
    public void From_WithoutLetterOrDigitLeft_ReturnsFallback()
    {
        Assert.Equal(Fallback, JobApplicationSlug.From("—", "&"));
    }
}
