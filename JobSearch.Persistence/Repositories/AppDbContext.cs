using JobSearch.Persistence.Configurations;
using JobSearch.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<JobSite> JobSites => Set<JobSite>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<UserJobMatch> UserJobMatches => Set<UserJobMatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
        modelBuilder.ApplyConfiguration(new UserSkillConfiguration());
        modelBuilder.ApplyConfiguration(new JobSiteConfiguration());
        modelBuilder.ApplyConfiguration(new JobConfiguration());
        modelBuilder.ApplyConfiguration(new UserJobMatchConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}