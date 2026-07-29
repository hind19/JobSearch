// Configurations/UserJobRejectionConfiguration.cs — ADR-0009
using JobSearch.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSearch.Persistence.Configurations;

public class UserJobRejectionConfiguration : IEntityTypeConfiguration<JobRejection>
{
    public void Configure(EntityTypeBuilder<JobRejection> builder)
    {
        builder.ToTable("UserJobRejections");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.RelevanceScore)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(r => r.RelevanceReason)
            .HasMaxLength(1000);

        builder.Property(r => r.AnalyzedAt)
            .IsRequired();

        builder.HasIndex(r => new { r.UserId, r.JobId })
            .IsUnique();

        builder.HasIndex(r => new { r.UserId, r.AnalyzedAt });
    }
}
