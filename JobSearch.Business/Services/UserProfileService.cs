using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Persistence.Abstractions;

namespace JobSearch.Business.Services;

public class UserProfileService : IUserProfileService
{
    private readonly ICvParser _cvParser;
    private readonly IQuestionGenerator _questionGenerator;
    private readonly IUserRepository _userRepository;

    public UserProfileService(
        ICvParser cvParser,
        IQuestionGenerator questionGenerator,
        IUserRepository userRepository)
    {
        _cvParser = cvParser;
        _questionGenerator = questionGenerator;
        _userRepository = userRepository;
    }

    public async Task<CvAnalysisResult> AnalyzeCvAsync(
        byte[] pdfBytes,
        CancellationToken ct = default)
    {
        var cvResult = await _cvParser.ParseCvAsync(pdfBytes, ct);

        if (!cvResult.IsSuccess)
            return cvResult;

        var questions = await _questionGenerator
            .GetClarifyingQuestionsAsync(cvResult, ct);

        return CvAnalysisResult.WithQuestions(cvResult, questions);
    }
}
