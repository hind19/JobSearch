// Configurations/JobSiteConfiguration.cs
using JobSearch.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSearch.Persistence.Configurations;

public class JobSiteConfiguration : IEntityTypeConfiguration<JobSite>
{
    public void Configure(EntityTypeBuilder<JobSite> builder)
    {
        builder.ToTable("JobSites");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.BaseUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.ScrapeConfig)
            .IsRequired()
            .HasDefaultValue("{}");

        builder.HasMany(s => s.Jobs)
            .WithOne(j => j.JobSite)
            .HasForeignKey(j => j.JobSiteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}