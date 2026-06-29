// Configurations/UserProfileConfiguration.cs
using JobSearch.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSearch.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.ClaudeReadyProfile)
            .IsRequired();

        builder.Property(p => p.DesiredRoles)
            .HasMaxLength(500);

        builder.Property(p => p.SalaryCurrency)
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(p => p.LocationPreference)
            .HasMaxLength(255);

        builder.Property(p => p.CvFileHash)
            .HasMaxLength(64);

        builder.Property(p => p.CvParsedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();
    }
}