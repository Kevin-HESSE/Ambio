using Ambio.Domain.JobApplications;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambio.Infrastructure.JobApplications;

public class OfferApplicationConfiguration : IEntityTypeConfiguration<OfferApplication>
{
    public void Configure(EntityTypeBuilder<OfferApplication> builder)
    {
        // An offer that has applications can't be deleted: it is archived instead (ADR 0012).
        builder.HasOne(a => a.JobOffer)
            .WithMany()
            .HasForeignKey(a => a.JobOfferId)
            .OnDelete(DeleteBehavior.Restrict);

        // A removed posting doesn't lose the application, only where it was sent from.
        builder.HasOne(a => a.AppliedViaPosting)
            .WithMany()
            .HasForeignKey(a => a.AppliedViaPostingId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
