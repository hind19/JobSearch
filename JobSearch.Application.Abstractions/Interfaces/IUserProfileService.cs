using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

public interface IUserProfileService
{
    Task<CvAnalysisResult> AnalyzeCvAsync(byte[] cvBytes, CancellationToken ct);

    Task<Guid?> FindUserByEmailAsync(string email, CancellationToken ct = default);

    // ADR-0002: resolves the target user for headless callers (Worker) as
    // the most recently modified user — no login/bypass involved.
    Task<Guid?> GetCurrentUserIdAsync(CancellationToken ct = default);

    // Нужны для отображения ранее сохранённого профиля при входе:
    // UserProfileViewModel.LoadUserProfileAsync грузит и то, и другое.
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);

    Task<UserDto?> GetUserAsync(Guid userId, CancellationToken ct = default);

    Task<List<UserSkillDto>> GetUserSkillsAsync(Guid userId, CancellationToken ct = default);

    Task SaveProfileAsync(Guid userId, CvAnalysisResult result, List<ClarifyingQuestionDto> answers, CancellationToken ct = default);
}
