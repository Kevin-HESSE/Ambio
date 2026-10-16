using Ambio.Domain.Companies;
using Ambio.Domain.JobApplications;
using Ambio.Infrastructure.Tests.Fixtures;

namespace Ambio.Infrastructure.Tests.Stories;

/// <summary>A company and the spontaneous applications sent to it: an application can't be saved without its
/// company. Each test builds its own story, so no data is shared between tests.</summary>
public sealed class SpontaneousApplicationStory
{
    private readonly List<SpontaneousApplication> _applications = [];

    public Company Company { get; } = CompanyFactory.GetGenerator().Generate();

    public IReadOnlyList<SpontaneousApplication> Applications => _applications;

    /// <summary>Adds <paramref name="count"/> spontaneous applications to the company.</summary>
    public SpontaneousApplicationStory WithApplications(int count, string ruleSet = "default")
    {
        _applications.AddRange(SpontaneousApplicationFactory.GetGenerator(Company.Id).Generate(count, ruleSet));
        return this;
    }

    /// <summary>Saves the company, then the applications.</summary>
    public async Task SaveAsync(SqliteServiceProvider services)
    {
        await services.AddAsync(Company);
        await services.AddAsync([.. _applications]);
    }
}
