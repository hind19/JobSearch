// Configurations/SentEmailConfiguration.cs
using JobSearch.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSearch.Persistence.Configurations;

public class SentEmailConfiguration : IEntityTypeConfiguration<SentEmail>
{
    public void Configure(EntityTypeBuilder<SentEmail> builder)
    {
        builder.ToTable("SentEmails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ToAddress)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Body)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Pending");

        builder.Property(e => e.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany(u => u.SentEmails)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.CreatedAt });
        builder.HasIndex(e => e.Status);
    }
}
