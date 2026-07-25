using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Enums;
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Business.Mapping;

internal static class BusinessMapper
{
    // ─── User ────────────────────────────────────────────────

    internal static UserDto ToDto(UserPersistenceDto dto) =>
        new(
            id: dto.Id,
            email: dto.Email,
            name: dto.Name,
            createdAt: dto.CreatedAt,
            isActive: dto.IsActive
        );

    internal static UserPersistenceDto ToPersistenceDto(UserDto dto) =>
        new(
            id: dto.Id,
            email: dto.Email,
            name: dto.Name,
            createdAt: dto.CreatedAt,
            isActive: dto.IsActive
        );

    // ─── UserProfile ─────────────────────────────────────────

    internal static UserProfileDto ToDto(UserProfilePersistenceDto dto) =>
        new(
            id: dto.Id,
            userId: dto.UserId,
            claudeReadyProfile: dto.ClaudeReadyProfile,
            desiredRoles: dto.DesiredRoles,
            desiredSalaryMin: dto.DesiredSalaryMin,
            desiredSalaryMax: dto.DesiredSalaryMax,
            salaryCurrency: dto.SalaryCurrency,
            locationPreference: dto.LocationPreference,
            cvParsedAt: dto.CvParsedAt,
            cvFileHash: dto.CvFileHash,
            updatedAt: dto.UpdatedAt
        );

    internal static UserProfilePersistenceDto ToPersistenceDto(
        UserProfileDto dto) =>
        new(
            id: dto.Id,
            userId: dto.UserId,
            claudeReadyProfile: dto.ClaudeReadyProfile,
            desiredRoles: dto.DesiredRoles,
            desiredSalaryMin: dto.DesiredSalaryMin,
            desiredSalaryMax: dto.DesiredSalaryMax,
            salaryCurrency: dto.SalaryCurrency,
            locationPreference: dto.LocationPreference,
            cvParsedAt: dto.CvParsedAt,
            cvFileHash: dto.CvFileHash,
            updatedAt: dto.UpdatedAt
        );

    // ─── UserSkill ───────────────────────────────────────────

    internal static UserSkillDto ToDto(UserSkillPersistenceDto dto) =>
        new(
            id: dto.Id,
            userId: dto.UserId,
            skillName: dto.SkillName,
            proficiencyLevel: ParseProficiencyLevel(dto.ProficiencyLevel),
            yearsOfExperience: dto.YearsOfExperience,
            extractedByClaude: dto.ExtractedByClaude
        );

    internal static UserSkillPersistenceDto ToPersistenceDto(
        UserSkillDto dto) =>
        new(
            id: dto.Id,
            userId: dto.UserId,
            skillName: dto.SkillName,
            proficiencyLevel: dto.ProficiencyLevel.ToString(),
            yearsOfExperience: dto.YearsOfExperience,
            extractedByClaude: dto.ExtractedByClaude
        );

    // ─── JobSite ─────────────────────────────────────────────

    internal static JobSiteDto ToDto(JobSitePersistenceDto dto) =>
     new(
         id: dto.Id,
         name: dto.Name,
         baseUrl: dto.BaseUrl,
         isActive: dto.IsActive,
         // TODO: рассмотреть вопрос с изменением БД под объект вместо строки
         //       с добавлением новой таблицы ScrapeConfigs (Id, JobSiteId, поля конфига)
         scrapeConfig: ScrapeConfigMapper.FromJson(dto.ScrapeConfig)
     );

    internal static JobSitePersistenceDto ToPersistenceDto(JobSiteDto dto) =>
    new(
        id: dto.Id,
        name: dto.Name,
        baseUrl: dto.BaseUrl,
        isActive: dto.IsActive,
        // TODO: рассмотреть вопрос с изменением БД под объект вместо строки
        //       с добавлением новой таблицы ScrapeConfigs (Id, JobSiteId, поля конфига)
        scrapeConfig: ScrapeConfigMapper.ToJson(dto.ScrapeConfig)
    );

    // ─── Job ─────────────────────────────────────────────────

    internal static JobDto ToDto(JobPersistenceDto dto) =>
        new(
            id: dto.Id,
            jobSiteId: dto.JobSiteId,
            externalId: dto.ExternalId,
            url: dto.Url,
            title: dto.Title,
            company: dto.Company,
            location: dto.Location,
            salaryRaw: dto.SalaryRaw,
            descriptionRaw: dto.DescriptionRaw,
            postedAt: dto.PostedAt,
            foundAt: dto.FoundAt,
            urlHash: dto.UrlHash
        );

    internal static JobPersistenceDto ToPersistenceDto(JobDto dto) =>
        new(
            id: dto.Id,
            jobSiteId: dto.JobSiteId,
            externalId: dto.ExternalId,
            url: dto.Url,
            title: dto.Title,
            company: dto.Company,
            location: dto.Location,
            salaryRaw: dto.SalaryRaw,
            descriptionRaw: dto.DescriptionRaw,
            postedAt: dto.PostedAt,
            foundAt: dto.FoundAt,
            urlHash: dto.UrlHash
        );

    // ─── UserJobMatch ─────────────────────────────────────────

    internal static UserJobMatchDto ToDto(
        UserJobMatchPersistenceDto dto) =>
        new(
            id: dto.Id,
            userId: dto.UserId,
            jobId: dto.JobId,
            relevanceScore: dto.RelevanceScore,
            relevanceReason: dto.RelevanceReason,
            wasNotified: dto.WasNotified,
            notifiedAt: dto.NotifiedAt,
            foundInRunAt: dto.FoundInRunAt,
            job: ToDto(dto.Job)
        );

    internal static UserJobMatchPersistenceDto ToPersistenceDto(
        UserJobMatchDto dto) =>
        new(
            id: dto.Id,
            userId: dto.UserId,
            jobId: dto.JobId,
            relevanceScore: dto.RelevanceScore,
            relevanceReason: dto.RelevanceReason,
            wasNotified: dto.WasNotified,
            notifiedAt: dto.NotifiedAt,
            foundInRunAt: dto.FoundInRunAt,
            job: ToPersistenceDto(dto.Job)
        );

    // ─── Collections ─────────────────────────────────────────

    internal static List<UserSkillDto> ToDto(
        List<UserSkillPersistenceDto> dtos) =>
        dtos.Select(ToDto).ToList();

    internal static List<UserSkillPersistenceDto> ToPersistenceDto(
        List<UserSkillDto> dtos) =>
        dtos.Select(ToPersistenceDto).ToList();

    internal static List<JobDto> ToDto(
        List<JobPersistenceDto> dtos) =>
        dtos.Select(ToDto).ToList();

    internal static List<UserJobMatchDto> ToDto(
        List<UserJobMatchPersistenceDto> dtos) =>
        dtos.Select(ToDto).ToList();

    // ─── SentEmail ───────────────────────────────────────────

    internal static SentEmailDto ToDto(SentEmailPersistenceDto dto) =>
        new(
            id: dto.Id,
            userId: dto.UserId,
            toAddress: dto.ToAddress,
            subject: dto.Subject,
            body: dto.Body,
            status: ParseEmailSendStatus(dto.Status),
            attemptCount: dto.AttemptCount,
            errorMessage: dto.ErrorMessage,
            sentAt: dto.SentAt,
            createdAt: dto.CreatedAt
        );

    internal static SentEmailPersistenceDto ToPersistenceDto(SentEmailDto dto) =>
        new(
            id: dto.Id,
            userId: dto.UserId,
            toAddress: dto.ToAddress,
            subject: dto.Subject,
            body: dto.Body,
            status: dto.Status.ToString(),
            attemptCount: dto.AttemptCount,
            errorMessage: dto.ErrorMessage,
            sentAt: dto.SentAt,
            createdAt: dto.CreatedAt
        );

    // ─── EmailSettings ───────────────────────────────────────

    internal static EmailSettingsDto ToDto(EmailSettingsPersistenceDto dto) =>
        new(
            id: dto.Id,
            smtpHost: dto.SmtpHost,
            smtpPort: dto.SmtpPort,
            useSsl: dto.UseSsl,
            smtpUsername: dto.SmtpUsername,
            fromAddress: dto.FromAddress,
            fromDisplayName: dto.FromDisplayName,
            updatedAt: dto.UpdatedAt
        );

    internal static EmailSettingsPersistenceDto ToPersistenceDto(EmailSettingsDto dto) =>
        new(
            id: dto.Id,
            smtpHost: dto.SmtpHost,
            smtpPort: dto.SmtpPort,
            useSsl: dto.UseSsl,
            smtpUsername: dto.SmtpUsername,
            fromAddress: dto.FromAddress,
            fromDisplayName: dto.FromDisplayName,
            updatedAt: dto.UpdatedAt
        );

    // ─── JobSiteStatistics ───────────────────────────────────

    internal static JobSiteStatisticsDto ToDto(
        JobSiteStatisticsPersistenceDto dto) =>
        new(
            jobSiteId: dto.JobSiteId,
            jobSiteName: dto.JobSiteName,
            jobsScrapedCount: dto.JobsScrapedCount,
            matchesCount: dto.MatchesCount,
            averageRelevanceScore: dto.AverageRelevanceScore,
            mostRecentMatchAt: dto.MostRecentMatchAt
        );

    // ─── Helpers ─────────────────────────────────────────────

    private static ProficiencyLevel ParseProficiencyLevel(
        string value) =>
        Enum.TryParse<ProficiencyLevel>(
            value,
            ignoreCase: true,
            out var result)
                ? result
                : ProficiencyLevel.NotSpecified;

    private static EmailSendStatus ParseEmailSendStatus(
        string value) =>
        Enum.TryParse<EmailSendStatus>(
            value,
            ignoreCase: true,
            out var result)
                ? result
                : EmailSendStatus.Pending;
}