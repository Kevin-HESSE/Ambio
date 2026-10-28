using Ambio.Domain.JobApplications;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambio.Infrastructure.JobApplications;

public class SpontaneousApplicationConfiguration : IEntityTypeConfiguration<SpontaneousApplication>
{
    public void Configure(EntityTypeBuilder<SpontaneousApplication> builder)
    {
        // A company that has applications can't be deleted: it is archived instead (ADR 0012).
        builder.HasOne(a => a.Company)
            .WithMany()
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
