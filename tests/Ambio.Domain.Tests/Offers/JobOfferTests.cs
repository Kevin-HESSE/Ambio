using Ambio.Domain.Offers;

namespace Ambio.Domain.Tests.Offers;

public class JobOfferTests
{
    private const string Title = "Backend Developer";
    private const string ShortId = "482913";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string OtherUrl = "https://www.welcometothejungle.com/fr/companies/kelvio/jobs/backend";

    private static readonly DateTime ArchivedAt = new(2026, 9, 27, 10, 0, 0, DateTimeKind.Utc);

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

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void ShortId_WhenBlank_ThrowsArgumentException(string shortId)
    {
        Assert.Throws<ArgumentException>(() => NewOffer(shortId: shortId));
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

    [Fact]
    public void HasPostingWithUrl_WithPaddedUrlOfAPosting_ReturnsTrue()
    {
        var offer = NewOffer();
        offer.AddPosting(Site, Url);

        Assert.True(offer.HasPostingWithUrl($" {Url}\n"));
        Assert.False(offer.HasPostingWithUrl(OtherUrl));
    }

    [Fact]
    public void CanRemovePosting_WithOnePosting_ReturnsFalse()
    {
        var offer = NewOffer();
        offer.AddPosting(Site, Url);

        Assert.False(offer.CanRemovePosting);
    }

    [Fact]
    public void CanRemovePosting_WithSeveralPostings_ReturnsTrue()
    {
        var offer = NewOffer();
        offer.AddPosting(Site, Url);
        offer.AddPosting(Site, OtherUrl);

        Assert.True(offer.CanRemovePosting);
    }

    [Fact]
    public void RemovePosting_WithSeveralPostings_RemovesIt()
    {
        var offer = NewOffer();
        var posting = offer.AddPosting(Site, Url);
        offer.AddPosting(Site, OtherUrl);

        offer.RemovePosting(posting.Id);

        Assert.Equal(OtherUrl, Assert.Single(offer.Postings).Url);
    }

    [Fact]
    public void RemovePosting_LastPosting_ThrowsAndKeepsIt()
    {
        var offer = NewOffer();
        var posting = offer.AddPosting(Site, Url);

        Assert.Throws<InvalidOperationException>(() => offer.RemovePosting(posting.Id));
        Assert.Single(offer.Postings);
    }

    [Fact]
    public void RemovePosting_UnknownId_ThrowsAndKeepsThePostings()
    {
        var offer = NewOffer();
        offer.AddPosting(Site, Url);
        offer.AddPosting(Site, OtherUrl);

        Assert.Throws<InvalidOperationException>(() => offer.RemovePosting(Guid.CreateVersion7()));
        Assert.Equal(2, offer.Postings.Count);
    }

    [Fact]
    public void Archive_Active_RecordsTheDay()
    {
        var offer = NewOffer();

        offer.Archive(ArchivedAt);

        Assert.Equal(ArchivedAt, offer.ArchivedAt);
        Assert.True(offer.IsArchived);
    }

    [Fact]
    public void Archive_AlreadyArchived_KeepsTheFirstDay()
    {
        var offer = NewOffer();
        offer.Archive(ArchivedAt);

        offer.Archive(ArchivedAt.AddDays(1));

        Assert.Equal(ArchivedAt, offer.ArchivedAt);
    }

    [Fact]
    public void Restore_Archived_ClearsTheDay()
    {
        var offer = NewOffer();
        offer.Archive(ArchivedAt);

        offer.Restore();

        Assert.Null(offer.ArchivedAt);
        Assert.False(offer.IsArchived);
    }

    private static JobOffer NewOffer(string title = Title, string shortId = ShortId) => new()
    {
        ShortId = shortId,
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
