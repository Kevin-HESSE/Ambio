using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;

namespace Ambio.Domain.Tests.JobApplications;

public class OfferApplicationTests
{
    private const string ShortId = "482913";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";

    private static readonly DateOnly SentOn = new(2026, 9, 3);

    [Fact]
    public void New_GeneratesVersion7Id()
    {
        Assert.Equal(7, NewApplication(Guid.CreateVersion7()).Id.Version);
    }

    [Fact]
    public void New_IsDraftWithoutSendingDate()
    {
        var application = NewApplication(Guid.CreateVersion7());

        Assert.Equal(ApplicationStatus.Draft, application.Status);
        Assert.Null(application.AppliedOn);
        Assert.Null(application.AppliedViaPostingId);
    }

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void ShortId_WhenBlank_ThrowsArgumentException(string shortId)
    {
        Assert.Throws<ArgumentException>(() => NewApplication(Guid.CreateVersion7(), shortId));
    }

    [Fact]
    public void MarkAsSent_OnDraft_MovesToAppliedWithTheDate()
    {
        var application = NewApplication(Guid.CreateVersion7());

        application.MarkAsSent(SentOn);

        Assert.Equal((ApplicationStatus.Applied, SentOn), (application.Status, application.AppliedOn));
    }

    [Fact]
    public void MarkAsSent_WhenAlreadySent_ThrowsInvalidOperationException()
    {
        var application = NewApplication(Guid.CreateVersion7());
        application.MarkAsSent(SentOn);

        Assert.Throws<InvalidOperationException>(() => application.MarkAsSent(SentOn.AddDays(1)));
    }

    [Fact]
    public void ApplyVia_PostingOfTheOffer_RecordsIt()
    {
        var offer = NewOffer();
        var posting = offer.AddPosting(Site, Url);
        var application = NewApplication(offer.Id);

        application.ApplyVia(posting);

        Assert.Equal(posting.Id, application.AppliedViaPostingId);
    }

    [Fact]
    public void ApplyVia_PostingOfAnotherOffer_ThrowsInvalidOperationException()
    {
        var posting = NewOffer().AddPosting(Site, Url);
        var application = NewApplication(NewOffer().Id);

        Assert.Throws<InvalidOperationException>(() => application.ApplyVia(posting));
        Assert.Null(application.AppliedViaPostingId);
    }

    [Fact]
    public void ClearAppliedVia_WithPosting_ForgetsIt()
    {
        var offer = NewOffer();
        var application = NewApplication(offer.Id);
        application.ApplyVia(offer.AddPosting(Site, Url));

        application.ClearAppliedVia();

        Assert.Null(application.AppliedViaPostingId);
    }

    [Fact]
    public void ChangeAppliedOn_WhenSent_ChangesTheDayButNotTheStatus()
    {
        var application = NewApplication(Guid.CreateVersion7());
        application.MarkAsSent(SentOn);

        application.ChangeAppliedOn(SentOn.AddDays(-1));

        Assert.Equal((ApplicationStatus.Applied, SentOn.AddDays(-1)), (application.Status, application.AppliedOn));
    }

    [Fact]
    public void ChangeAppliedOn_OnDraft_ThrowsInvalidOperationException()
    {
        var application = NewApplication(Guid.CreateVersion7());

        Assert.Throws<InvalidOperationException>(() => application.ChangeAppliedOn(SentOn));
        Assert.Null(application.AppliedOn);
    }

    private static OfferApplication NewApplication(Guid jobOfferId, string shortId = ShortId) => new()
    {
        ShortId = shortId,
        JobOfferId = jobOfferId,
        Channel = Channel.OnlineForm,
    };

    private static JobOffer NewOffer() => new()
    {
        CompanyId = Guid.CreateVersion7(),
        Title = "Backend Developer",
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
