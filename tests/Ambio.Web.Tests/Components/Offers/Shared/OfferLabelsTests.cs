using Ambio.Domain.Offers;
using Ambio.Web.Components.Offers.Shared;

namespace Ambio.Web.Tests.Components.Offers.Shared;

public class OfferLabelsTests
{
    [Fact]
    public void ToLabel_EveryContractType_ReturnsDistinctLabels()
    {
        var labels = Enum.GetValues<ContractType>().Select(contractType => contractType.ToLabel()).ToList();

        Assert.Equal(labels.Count, labels.Distinct().Count());
    }

    [Fact]
    public void ToLabel_EveryRemotePolicy_ReturnsDistinctLabels()
    {
        var labels = Enum.GetValues<RemotePolicy>().Select(remotePolicy => remotePolicy.ToLabel()).ToList();

        Assert.Equal(labels.Count, labels.Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(PostingCounts))]
    public void PostingWord_WithCount_ReturnsSingularOrPlural(int count, string expected)
    {
        Assert.Equal(expected, OfferLabels.PostingWord(count));
    }

    public static TheoryData<int, string> PostingCounts => new()
    {
        { 0, "postings" },
        { 1, "posting" },
        { 3, "postings" },
    };
}
