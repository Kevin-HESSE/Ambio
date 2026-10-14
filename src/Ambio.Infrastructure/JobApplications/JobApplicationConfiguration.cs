using Ambio.Domain.JobApplications;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambio.Infrastructure.JobApplications;

/// <summary>The applications of every kind share one table, told apart by the Kind column (TPH).</summary>
public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public const string Discriminator = "Kind";

    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasDiscriminator<string>(Discriminator)
            .HasValue<OfferApplication>("Offer");

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
