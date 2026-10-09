using Ambio.Domain.Companies;
using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.Persistence;

public class TimestampInterceptorTests : IAsyncLifetime
{
    private const string CompanyName = "Acme";
    private const string NewCompanyName = "Acme Corp";

    private static readonly DateTimeOffset CreationTime = new(2026, 1, 15, 9, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdateTime = new(2026, 2, 20, 14, 0, 0, TimeSpan.Zero);

    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_NewEntity_SetsCreatedAtAndUpdatedAt()
    {
        var id = await AddCompanyAsync();

        var company = await FindCompanyAsync(id);
        Assert.Equal(CreationTime.UtcDateTime, company.CreatedAt);
        Assert.Equal(CreationTime.UtcDateTime, company.UpdatedAt);
    }

    [Fact]
    public async Task SaveChanges_NewEntity_SetsCreatedAtAndUpdatedAt()
    {
        SetNow(CreationTime);
        var company = NewCompany();

        await using (var context = await _services.CreateDbContextAsync())
        {
            context.Companies.Add(company);
            context.SaveChanges();
        }

        var saved = await FindCompanyAsync(company.Id);
        Assert.Equal(CreationTime.UtcDateTime, saved.CreatedAt);
        Assert.Equal(CreationTime.UtcDateTime, saved.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedEntity_UpdatesOnlyUpdatedAt()
    {
        var id = await AddCompanyAsync();
        SetNow(UpdateTime);

        await using (var context = await _services.CreateDbContextAsync())
        {
            var company = await context.Companies.SingleAsync(c => c.Id == id, TestContext.Current.CancellationToken);
            company.Name = NewCompanyName;
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var saved = await FindCompanyAsync(id);
        Assert.Equal(CreationTime.UtcDateTime, saved.CreatedAt);
        Assert.Equal(UpdateTime.UtcDateTime, saved.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedCreatedAt_KeepsOriginalCreatedAt()
    {
        var id = await AddCompanyAsync();
        SetNow(UpdateTime);

        await using (var context = await _services.CreateDbContextAsync())
        {
            var company = await context.Companies.SingleAsync(c => c.Id == id, TestContext.Current.CancellationToken);
            company.CreatedAt = UpdateTime.UtcDateTime;
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var saved = await FindCompanyAsync(id);
        Assert.Equal(CreationTime.UtcDateTime, saved.CreatedAt);
    }

    private void SetNow(DateTimeOffset now) => _services.TimeProvider.GetUtcNow().Returns(now);

    private static Company NewCompany() => new()
    {
        Name = CompanyName,
        Kind = CompanyKind.Employer,
    };

    private async Task<Guid> AddCompanyAsync()
    {
        SetNow(CreationTime);
        var company = NewCompany();

        await using var context = await _services.CreateDbContextAsync();
        context.Companies.Add(company);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return company.Id;
    }

    private async Task<Company> FindCompanyAsync(Guid id)
    {
        await using var context = await _services.CreateDbContextAsync();
        return await context.Companies.AsNoTracking().SingleAsync(c => c.Id == id, TestContext.Current.CancellationToken);
    }
}
