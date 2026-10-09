using Ambio.Domain.Companies;
using Ambio.Web.Components.Companies.Shared;

namespace Ambio.Web.Tests.Components.Companies.Shared;

public class CompanyLabelsTests
{
    [Fact]
    public void ToLabel_EveryKind_ReturnsDistinctLabels()
    {
        var labels = Enum.GetValues<CompanyKind>().Select(kind => kind.ToLabel()).ToList();

        Assert.Equal(labels.Count, labels.Distinct().Count());
    }
}
