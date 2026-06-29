// Configurations/UserSkillConfiguration.cs
using JobSearch.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSearch.Persistence.Configurations;

public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("UserSkills");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.SkillName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ProficiencyLevel)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("NotSpecified");

        builder.Property(s => s.YearsOfExperience)
            .HasPrecision(4, 1);

        builder.Property(s => s.ExtractedByClaude)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(s => new { s.UserId, s.SkillName });
    }
}