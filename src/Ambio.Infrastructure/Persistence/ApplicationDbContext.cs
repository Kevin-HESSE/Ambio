using Ambio.Domain.Companies;
using Ambio.Domain.CoverLetters;
using Ambio.Domain.CvDocuments;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;
using Ambio.Domain.Settings;
using Ambio.Infrastructure.Users;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<JobOffer> JobOffers => Set<JobOffer>();

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    public DbSet<CoverLetter> CoverLetters => Set<CoverLetter>();

    public DbSet<CvDocument> CvDocuments => Set<CvDocument>();

    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
