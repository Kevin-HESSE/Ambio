using Ambio.Domain.Offers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambio.Infrastructure.Offers;

public class JobOfferPostingConfiguration : IEntityTypeConfiguration<JobOfferPosting>
{
    public void Configure(EntityTypeBuilder<JobOfferPosting> builder)
    {
        // No DbSet: the postings are reached through their offer.
        builder.ToTable("JobOfferPostings");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Site)
            .IsRequired();

        builder.Property(p => p.Url)
            .IsRequired();

        builder.HasIndex(p => new { p.JobOfferId, p.Url })
            .IsUnique();
    }
}
