using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Entities;

namespace JobSearch.Persistence.Mapping;

internal static class PersistenceMapper
{
    internal static UserPersistenceDto ToDto(User entity) =>
        new(
            id: entity.Id,
            email: entity.Email,
            name: entity.Name,
            createdAt: entity.CreatedAt,
            isActive: entity.IsActive
        );

    internal static User ToEntity(UserPersistenceDto dto) =>
        new()
        {
            Id = dto.Id,
            Email = dto.Email,
            Name = dto.Name,
            CreatedAt = dto.CreatedAt,
            IsActive = dto.IsActive
        };

    internal static UserProfilePersistenceDto ToDto(UserProfile entity) =>
        new(
            id: entity.Id,
            userId: entity.UserId,
            claudeReadyProfile: entity.ClaudeReadyProfile,
            desiredRoles: entity.DesiredRoles,
            desiredSalaryMin: entity.DesiredSalaryMin,
            desiredSalaryMax: entity.DesiredSalaryMax,
            salaryCurrency: entity.SalaryCurrency,
            locationPreference: entity.LocationPreference,
            cvParsedAt: entity.CvParsedAt,
            cvFileHash: entity.CvFileHash,
            updatedAt: entity.UpdatedAt
        );

    internal static UserProfile ToEntity(UserProfilePersistenceDto dto) =>
        new()
        {
            Id = dto.Id,
            UserId = dto.UserId,
            ClaudeReadyProfile = dto.ClaudeReadyProfile,
            DesiredRoles = dto.DesiredRoles,
            DesiredSalaryMin = dto.DesiredSalaryMin,
            DesiredSalaryMax = dto.DesiredSalaryMax,
            SalaryCurrency = dto.SalaryCurrency,
            LocationPreference = dto.LocationPreference,
            CvParsedAt = dto.CvParsedAt,
            CvFileHash = dto.CvFileHash,
            UpdatedAt = dto.UpdatedAt
        };

    internal static UserSkillPersistenceDto ToDto(UserSkill entity) =>
        new(
            id: entity.Id,
            userId: entity.UserId,
            skillName: entity.SkillName,
            proficiencyLevel: entity.ProficiencyLevel,
            yearsOfExperience: entity.YearsOfExperience,
            extractedByClaude: entity.ExtractedByClaude
        );

    internal static UserSkill ToEntity(UserSkillPersistenceDto dto) =>
        new()
        {
            Id = dto.Id,
            UserId = dto.UserId,
            SkillName = dto.SkillName,
            ProficiencyLevel = dto.ProficiencyLevel,
            YearsOfExperience = dto.YearsOfExperience,
            ExtractedByClaude = dto.ExtractedByClaude
        };

    internal static JobSitePersistenceDto ToDto(JobSite entity) =>
        new(
            id: entity.Id,
            name: entity.Name,
            baseUrl: entity.BaseUrl,
            isActive: entity.IsActive,
            scrapeConfig: entity.ScrapeConfig
        );

    internal static JobSite ToEntity(JobSitePersistenceDto dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            BaseUrl = dto.BaseUrl,
            IsActive = dto.IsActive,
            ScrapeConfig = dto.ScrapeConfig
        };

    internal static JobPersistenceDto ToDto(Job entity) =>
        new(
            id: entity.Id,
            jobSiteId: entity.JobSiteId,
            externalId: entity.ExternalId,
            url: entity.Url,
            title: entity.Title,
            company: entity.Company,
            location: entity.Location,
            salaryRaw: entity.SalaryRaw,
            descriptionRaw: entity.DescriptionRaw,
            postedAt: entity.PostedAt,
            foundAt: entity.FoundAt,
            urlHash: entity.UrlHash
        );

    internal static Job ToEntity(JobPersistenceDto dto) =>
        new()
        {
            Id = dto.Id,
            JobSiteId = dto.JobSiteId,
            ExternalId = dto.ExternalId,
            Url = dto.Url,
            Title = dto.Title,
            Company = dto.Company,
            Location = dto.Location,
            SalaryRaw = dto.SalaryRaw,
            DescriptionRaw = dto.DescriptionRaw,
            PostedAt = dto.PostedAt,
            FoundAt = dto.FoundAt,
            UrlHash = dto.UrlHash
        };

    internal static UserJobMatchPersistenceDto ToDto(
        UserJobMatch entity) =>
        new(
            id: entity.Id,
            userId: entity.UserId,
            jobId: entity.JobId,
            relevanceScore: entity.RelevanceScore,
            relevanceReason: entity.RelevanceReason,
            wasNotified: entity.WasNotified,
            notifiedAt: entity.NotifiedAt,
            foundInRunAt: entity.FoundInRunAt,
            job: ToDto(entity.Job)
        );

    internal static UserJobMatch ToEntity(
        UserJobMatchPersistenceDto dto) =>
        new()
        {
            Id = dto.Id,
            UserId = dto.UserId,
            JobId = dto.JobId,
            RelevanceScore = dto.RelevanceScore,
            RelevanceReason = dto.RelevanceReason,
            WasNotified = dto.WasNotified,
            NotifiedAt = dto.NotifiedAt,
            FoundInRunAt = dto.FoundInRunAt
        };
}