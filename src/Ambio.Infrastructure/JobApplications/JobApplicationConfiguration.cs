using Ambio.Domain.JobApplications;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambio.Infrastructure.JobApplications;

/// <summary>The applications of every kind share one table, told apart by the Kind column (TPH).</summary>
public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public const string Discriminator = "Kind";

    public const string KindConstraint = "CK_JobApplications_Kind";

    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        // The columns of each kind are nullable, as the other kind leaves them empty: the constraint requires those
        // of the row's kind and forbids the others.
        builder.ToTable("JobApplications", table => table.HasCheckConstraint(
            KindConstraint,
            $"""
             ("{Discriminator}" = '{nameof(ApplicationKind.Offer)}' AND "JobOfferId" IS NOT NULL
                 AND "CompanyId" IS NULL AND "TargetPosition" IS NULL)
             OR ("{Discriminator}" = '{nameof(ApplicationKind.Spontaneous)}' AND "CompanyId" IS NOT NULL
                 AND "TargetPosition" IS NOT NULL AND "JobOfferId" IS NULL AND "AppliedViaPostingId" IS NULL)
             """));

        builder.HasDiscriminator<string>(Discriminator)
            .HasValue<OfferApplication>(nameof(ApplicationKind.Offer))
            .HasValue<SpontaneousApplication>(nameof(ApplicationKind.Spontaneous));

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.ShortId)
            .IsRequired();

        builder.HasIndex(a => a.ShortId)
            .IsUnique();

        builder.Property(a => a.Status)
            .HasConversion<string>();

        builder.Property(a => a.Channel)
            .HasConversion<string>();
    }
}
