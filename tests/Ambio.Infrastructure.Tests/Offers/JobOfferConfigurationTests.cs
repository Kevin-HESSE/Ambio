using Ambio.Domain.Companies;
using Ambio.Domain.Offers;
using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.Offers;

public class JobOfferConfigurationTests : IAsyncLifetime
{
    private const string CompanyName = "Kelvio";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string OtherUrl = "https://kelvio.io/careers/backend-developer";

    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_OfferWithPostings_SavesEveryField()
    {
        var company = await AddCompanyAsync();
        var offer = JobOfferFactory.New(company.Id);
        offer.Description = "We are looking for a backend developer.";
        offer.Location = "Nantes";
        offer.ContractType = ContractType.Freelance;
        offer.RemotePolicy = RemotePolicy.FullRemote;
        offer.Salary = "TJM 500";
        offer.AddPosting(Site, Url);
        offer.AddPosting("Company careers page", OtherUrl);
        await AddAsync(offer);

        await using var context = await _services.CreateDbContextAsync();
        var saved = await context.JobOffers
            .Include(o => o.Company)
            .Include(o => o.Postings)
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            (offer.Title, offer.Description, offer.Location, offer.ContractType, offer.RemotePolicy, offer.Salary, CompanyName),
            (saved.Title, saved.Description, saved.Location, saved.ContractType, saved.RemotePolicy, saved.Salary, saved.Company.Name));
        Assert.Equal(
            offer.Postings.Select(p => (p.Id, p.Site, p.Url)).Order(),
            saved.Postings.Select(p => (p.Id, p.Site, p.Url)).Order());
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedOffer_DeletesItsPostings()
    {
        var company = await AddCompanyAsync();
        var offer = JobOfferFactory.New(company.Id);
        offer.AddPosting(Site, Url);
        await AddAsync(offer);

        await using (var context = await _services.CreateDbContextAsync())
        {
            context.JobOffers.Remove(await context.JobOffers.Include(o => o.Postings).SingleAsync(TestContext.Current.CancellationToken));
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var check = await _services.CreateDbContextAsync();
        Assert.Empty(await check.Set<JobOfferPosting>().ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedCompanyWithOffer_IsRefused()
    {
        var company = await AddCompanyAsync();
        await AddAsync(JobOfferFactory.New(company.Id));

        await using var context = await _services.CreateDbContextAsync();
        context.Companies.Remove(await context.Companies.SingleAsync(TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_UrlAlreadyPostedForTheOffer_IsRefused()
    {
        var company = await AddCompanyAsync();
        var offer = JobOfferFactory.New(company.Id);
        offer.AddPosting(Site, Url);
        await AddAsync(offer);

        // Loaded without its postings, the offer can't see the URL it already has: only the index guards it.
        await using var context = await _services.CreateDbContextAsync();
        var loaded = await context.JobOffers.SingleAsync(TestContext.Current.CancellationToken);
        loaded.AddPosting("Indeed", Url);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_SameUrlForTwoOffers_SavesBoth()
    {
        var company = await AddCompanyAsync();
        var offer = JobOfferFactory.New(company.Id);
        var otherOffer = JobOfferFactory.New(company.Id, "Mobile Developer");
        offer.AddPosting(Site, Url);
        otherOffer.AddPosting(Site, Url);

        await AddAsync(offer, otherOffer);

        await using var context = await _services.CreateDbContextAsync();
        Assert.Equal(2, await context.Set<JobOfferPosting>().CountAsync(p => p.Url == Url, TestContext.Current.CancellationToken));
    }

    private async Task<Company> AddCompanyAsync()
    {
        var company = CompanyFactory.New(CompanyName);
        await using var context = await _services.CreateDbContextAsync();
        context.Companies.Add(company);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return company;
    }

    private async Task AddAsync(params JobOffer[] offers)
    {
        await using var context = await _services.CreateDbContextAsync();
        context.JobOffers.AddRange(offers);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
