// Configurations/UserJobMatchConfiguration.cs
using JobSearch.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSearch.Persistence.Configurations;

public class UserJobMatchConfiguration : IEntityTypeConfiguration<UserJobMatch>
{
    public void Configure(EntityTypeBuilder<UserJobMatch> builder)
    {
        builder.ToTable("UserJobMatches");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedOnAdd();

        builder.Property(m => m.RelevanceScore)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(m => m.RelevanceReason)
            .HasMaxLength(1000);

        builder.Property(m => m.WasNotified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.FoundInRunAt)
            .IsRequired();

        builder.HasIndex(m => new { m.UserId, m.JobId })
            .IsUnique();

        builder.HasIndex(m => m.WasNotified);
        builder.HasIndex(m => m.FoundInRunAt);
    }
}