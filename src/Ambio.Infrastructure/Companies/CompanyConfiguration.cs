using Ambio.Domain.Companies;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambio.Infrastructure.Companies;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .UseCollation("NOCASE");

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.Property(c => c.Kind)
            .HasConversion<string>();

        builder.Property(c => c.Size)
            .HasConversion<string>();
    }
}
