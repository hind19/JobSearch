// Configurations/JobConfiguration.cs
using JobSearch.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSearch.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .ValueGeneratedOnAdd();

        builder.Property(j => j.ExternalId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(j => j.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(j => j.Company)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(j => j.Location)
            .HasMaxLength(255);

        builder.Property(j => j.SalaryRaw)
            .HasMaxLength(255);

        builder.Property(j => j.DescriptionRaw)
            .IsRequired();

        builder.Property(j => j.UrlHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(j => j.FoundAt)
            .IsRequired();

        builder.HasIndex(j => j.UrlHash)
            .IsUnique();

        builder.HasIndex(j => new { j.JobSiteId, j.ExternalId })
            .IsUnique();

        builder.HasMany(j => j.UserJobMatches)
            .WithOne(m => m.Job)
            .HasForeignKey(m => m.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}