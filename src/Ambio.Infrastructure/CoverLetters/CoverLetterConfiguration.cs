using Ambio.Domain.CoverLetters;
using Ambio.Domain.JobApplications;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambio.Infrastructure.CoverLetters;

public class CoverLetterConfiguration : IEntityTypeConfiguration<CoverLetter>
{
    public void Configure(EntityTypeBuilder<CoverLetter> builder)
    {
        builder.ToTable("CoverLetters");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedNever();

        builder.Property(l => l.Title)
            .IsRequired();

        builder.Property(l => l.Content)
            .IsRequired();

        // The letters go with their application.
        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(l => l.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
