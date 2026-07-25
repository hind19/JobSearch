// Configurations/EmailSettingsConfiguration.cs
using JobSearch.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSearch.Persistence.Configurations;

public class EmailSettingsConfiguration : IEntityTypeConfiguration<EmailSettings>
{
    public void Configure(EntityTypeBuilder<EmailSettings> builder)
    {
        builder.ToTable("EmailSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.SmtpHost)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.SmtpPort)
            .IsRequired();

        builder.Property(s => s.UseSsl)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.SmtpUsername)
            .IsRequired()
            .HasMaxLength(255)
            .HasDefaultValue(string.Empty);

        builder.Property(s => s.FromAddress)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.FromDisplayName)
            .IsRequired()
            .HasMaxLength(255)
            .HasDefaultValue(string.Empty);

        builder.Property(s => s.UpdatedAt)
            .IsRequired();
    }
}
