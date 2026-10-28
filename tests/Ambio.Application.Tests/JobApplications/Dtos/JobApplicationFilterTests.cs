using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

namespace Ambio.Application.Tests.JobApplications.Dtos;

public class JobApplicationFilterTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void IsEmpty_WithoutCriteriaOrWithABlankSearch_ReturnsTrue(string? search)
    {
        Assert.True(new JobApplicationFilter(Search: search).IsEmpty);
    }

    [Theory]
    [MemberData(nameof(FiltersWithACriterion))]
    public void IsEmpty_WithACriterion_ReturnsFalse(JobApplicationFilter filter)
    {
        Assert.False(filter.IsEmpty);
    }

    public static TheoryData<JobApplicationFilter> FiltersWithACriterion => new()
    {
        new JobApplicationFilter(Status: ApplicationStatus.Draft),
        new JobApplicationFilter(CompanySlug: "brightwave"),
        new JobApplicationFilter(Kind: ApplicationKind.Offer),
        new JobApplicationFilter(Search: "backend"),
        new JobApplicationFilter(IncludeArchived: true),
    };
}
