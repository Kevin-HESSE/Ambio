using Ambio.Domain.Offers;

namespace Ambio.Domain.Tests.Offers;

public class JobOfferTests
{
    private const string Title = "Backend Developer";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string OtherUrl = "https://www.welcometothejungle.com/fr/companies/kelvio/jobs/backend";

    [Fact]
    public void New_GeneratesVersion7Id()
    {
        Assert.Equal(7, NewOffer().Id.Version);
    }

    [Fact]
    public void New_HasNoPosting()
    {
        Assert.Empty(NewOffer().Postings);
    }

    [Fact]
    public void Title_WithSurroundingWhitespace_IsTrimmed()
    {
        var offer = NewOffer($"  {Title}\t");

        Assert.Equal(Title, offer.Title);
    }

    [Fact]
    public void Title_WhenNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => NewOffer(null!));
    }

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void Title_WhenBlank_ThrowsArgumentException(string title)
    {
        Assert.Throws<ArgumentException>(() => NewOffer(title));
    }

    [Fact]
    public void AddPosting_WithSiteAndUrl_AddsTrimmedPostingOfTheOffer()
    {
        var offer = NewOffer();

        var posting = offer.AddPosting($" {Site} ", $" {Url}\n");

        Assert.Same(posting, Assert.Single(offer.Postings));
        Assert.Equal(offer.Id, posting.JobOfferId);
        Assert.Equal(Site, posting.Site);
        Assert.Equal(Url, posting.Url);
        Assert.Equal(7, posting.Id.Version);
    }

    [Fact]
    public void AddPosting_WithAnotherUrl_KeepsBothPostings()
    {
        var offer = NewOffer();
        offer.AddPosting(Site, Url);

        offer.AddPosting(Site, OtherUrl);

        Assert.Equal([Url, OtherUrl], offer.Postings.Select(p => p.Url));
    }

    [Fact]
    public void AddPosting_WithUrlAlreadyRecorded_ThrowsAndKeepsOnePosting()
    {
        var offer = NewOffer();
        offer.AddPosting(Site, Url);

        Assert.Throws<InvalidOperationException>(() => offer.AddPosting("Indeed", $" {Url} "));
        Assert.Single(offer.Postings);
    }

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void AddPosting_WithBlankSite_ThrowsArgumentException(string site)
    {
        var offer = NewOffer();

        Assert.Throws<ArgumentException>(() => offer.AddPosting(site, Url));
        Assert.Empty(offer.Postings);
    }

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void AddPosting_WithBlankUrl_ThrowsArgumentException(string url)
    {
        var offer = NewOffer();

        Assert.Throws<ArgumentException>(() => offer.AddPosting(Site, url));
        Assert.Empty(offer.Postings);
    }

    private static JobOffer NewOffer(string title = Title) => new()
    {
        CompanyId = Guid.CreateVersion7(),
        Title = title,
        ContractType = ContractType.Permanent,
        RemotePolicy = RemotePolicy.Hybrid,
    };

    public static TheoryData<string> BlankValues =>
    [
        "",
        " ",
        "\t\n",
    ];
}
