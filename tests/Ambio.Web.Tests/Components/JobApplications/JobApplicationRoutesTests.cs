using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.JobApplications;

namespace Ambio.Web.Tests.Components.JobApplications;

public class JobApplicationRoutesTests
{
    private const string CompanySlug = "brightwave";
    private const string ShortId = "482913";
    private const string Slug = "backend-developer-brightwave";

    [Fact]
    public void Filtered_EmptyFilter_ReturnsTheList()
    {
        Assert.Equal(JobApplicationRoutes.List, JobApplicationRoutes.Filtered(new JobApplicationFilter()));
    }

    [Fact]
    public void Filtered_WithEveryCriterionAndShown_WritesThemInTheQueryString()
    {
        var filter = new JobApplicationFilter(ApplicationStatus.Interview, CompanySlug, ApplicationKind.Spontaneous, " data engineer ", IncludeArchived: true);

        var url = JobApplicationRoutes.Filtered(filter, shown: 50);

        Assert.Equal($"applications?q=data%20engineer&status=interview&company={CompanySlug}&kind=spontaneous&archived=true&shown=50", url);
    }

    [Fact]
    public void Filtered_WithBlankSearch_LeavesItOut()
    {
        var url = JobApplicationRoutes.Filtered(new JobApplicationFilter(Status: ApplicationStatus.Applied, Search: "  "));

        Assert.Equal("applications?status=applied", url);
    }

    [Fact]
    public void Details_WithTab_WritesItInTheQueryString()
    {
        var url = JobApplicationRoutes.Details(ShortId, Slug, JobApplicationRoutes.LettersTab);

        Assert.Equal($"applications/{ShortId}-{Slug}?tab=letters", url);
    }
}
