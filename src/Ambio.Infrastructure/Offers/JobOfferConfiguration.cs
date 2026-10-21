using Ambio.Domain.Offers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambio.Infrastructure.Offers;

public class JobOfferConfiguration : IEntityTypeConfiguration<JobOffer>
{
    public void Configure(EntityTypeBuilder<JobOffer> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.ShortId)
            .IsRequired();

        builder.HasIndex(o => o.ShortId)
            .IsUnique();

        builder.Property(o => o.Title)
            .IsRequired();

        builder.Property(o => o.ContractType)
            .HasConversion<string>();

        builder.Property(o => o.RemotePolicy)
            .HasConversion<string>();

        // A company that has offers can't be deleted: it is archived instead (ADR 0012).
        builder.HasOne(o => o.Company)
            .WithMany()
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Postings)
            .WithOne()
            .HasForeignKey(p => p.JobOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Postings)
            .HasField("_postings");
    }
}
