namespace JobSearch.Application.Abstractions.Interfaces;

public interface IUserProfileService
{
    Task<CvAnalysisResult> AnalyzeCvAsync(byte[] cvBytes, CancellationToken ct);
}
