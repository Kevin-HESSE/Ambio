using Ambio.Domain.CvDocuments;
using Ambio.Domain.JobApplications;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambio.Infrastructure.CvDocuments;

public class CvDocumentConfiguration : IEntityTypeConfiguration<CvDocument>
{
    public void Configure(EntityTypeBuilder<CvDocument> builder)
    {
        builder.ToTable("CvDocuments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.Property(d => d.Source)
            .HasConversion<string>();

        builder.Property(d => d.FileName)
            .IsRequired();

        builder.Property(d => d.StoragePath)
            .IsRequired();

        builder.HasIndex(d => d.StoragePath)
            .IsUnique();

        // A CV has a file on disk: the service deletes the files first, then the rows.
        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(d => d.JobApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
