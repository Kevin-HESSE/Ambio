using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Offers;

namespace Ambio.Application.Tests.Offers;

public class JobOfferMappingExtensionsTests
{
    private const string CompanyName = "Kelvio";
    private const string Title = "Backend Developer";
    private const string Location = "Nantes";
    private const string Salary = "45–55 k€";
    private const string Description = "We are looking for a backend developer.";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";

    private static readonly Guid CompanyId = Guid.CreateVersion7();

    [Fact]
    public void ToEntity_CopiesEveryFieldAndAddsTheFirstPosting()
    {
        var offer = new JobOfferInput
        {
            CompanyName = CompanyName,
            Title = Title,
            ContractType = ContractType.Freelance,
            RemotePolicy = RemotePolicy.FullRemote,
            Location = Location,
            Salary = Salary,
            Site = Site,
            Url = Url,
            Description = Description,
        }.ToEntity(CompanyId);

        Assert.Equal(
            (CompanyId, Title, ContractType.Freelance, RemotePolicy.FullRemote, Location, Salary, Description),
            (offer.CompanyId, offer.Title, offer.ContractType, offer.RemotePolicy, offer.Location, offer.Salary, offer.Description));
        var posting = Assert.Single(offer.Postings);
        Assert.Equal((Site, Url), (posting.Site, posting.Url));
    }

    [Fact]
    public void ToEntity_WithPaddedOrBlankFields_TrimsThemAndStoresBlankAsNull()
    {
        var offer = new JobOfferInput
        {
            CompanyName = CompanyName,
            Title = $" {Title} ",
            Location = $" {Location}\t",
            Salary = "  ",
            Site = $" {Site} ",
            Url = $" {Url} ",
            Description = "",
        }.ToEntity(CompanyId);

        Assert.Equal((Title, Location, null, null), (offer.Title, offer.Location, offer.Salary, offer.Description));
        var posting = Assert.Single(offer.Postings);
        Assert.Equal((Site, Url), (posting.Site, posting.Url));
    }
}
