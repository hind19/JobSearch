using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

public interface IUserProfileService
{
    Task<CvAnalysisResult> AnalyzeCvAsync(byte[] cvBytes, CancellationToken ct);

    Task<Guid?> FindUserByEmailAsync(string email, CancellationToken ct = default);

    Task SaveProfileAsync(Guid userId, CvAnalysisResult result, List<ClarifyingQuestionDto> answers, CancellationToken ct = default);
}
