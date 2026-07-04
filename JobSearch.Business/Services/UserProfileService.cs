using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Enums;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Business.Services;

public class UserProfileService : IUserProfileService
{
    private readonly ICvParser _cvParser;
    private readonly IQuestionGenerator _questionGenerator;
    private readonly IUserRepository _userRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserSkillRepository _userSkillRepository;

    public UserProfileService(
        ICvParser cvParser,
        IQuestionGenerator questionGenerator,
        IUserRepository userRepository,
        IUserProfileRepository userProfileRepository,
        IUserSkillRepository userSkillRepository)
    {
        _cvParser = cvParser;
        _questionGenerator = questionGenerator;
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _userSkillRepository = userSkillRepository;
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

    public async Task<Guid?> FindUserByEmailAsync(
        string email,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, ct);
        return user?.Id;
    }

    public async Task SaveProfileAsync(
        Guid userId,
        CvAnalysisResult result,
        List<ClarifyingQuestionDto> answers,
        CancellationToken ct = default)
    {
        if (!await _userRepository.ExistsAsync(userId, ct))
        {
            await _userRepository.CreateAsync(new UserPersistenceDto(
                id: userId,
                email: result.Candidate.Email ?? string.Empty,
                name: result.Candidate.FullName ?? string.Empty,
                createdAt: DateTime.UtcNow,
                isActive: true), ct);
        }

        var salaryAnswer = answers.FirstOrDefault(a =>
            a.AnswerType == AnswerType.NumericRange && a.Currency is not null);

        var now = DateTime.UtcNow;
        var existingProfile = await _userProfileRepository.GetByUserIdAsync(userId, ct);

        var profileDto = new UserProfilePersistenceDto(
            id: existingProfile?.Id ?? Guid.NewGuid(),
            userId: userId,
            claudeReadyProfile: result.ClaudeReadyProfile,
            desiredRoles: string.Join(", ", result.DesiredRoles),
            desiredSalaryMin: salaryAnswer?.RangeFrom.HasValue == true
                ? (int)salaryAnswer.RangeFrom.Value : null,
            desiredSalaryMax: salaryAnswer?.RangeTo.HasValue == true
                ? (int)salaryAnswer.RangeTo.Value : null,
            salaryCurrency: salaryAnswer?.Currency ?? "USD",
            locationPreference: string.Empty,
            cvParsedAt: now,
            cvFileHash: string.Empty,
            updatedAt: now);

        if (existingProfile is not null)
            await _userProfileRepository.UpdateAsync(profileDto, ct);
        else
            await _userProfileRepository.CreateAsync(profileDto, ct);

        await _userSkillRepository.DeleteAllByUserIdAsync(userId, ct);

        var skillDtos = result.Skills
            .Select(s => new UserSkillPersistenceDto(
                id: s.Id == Guid.Empty ? Guid.NewGuid() : s.Id,
                userId: userId,
                skillName: s.SkillName,
                proficiencyLevel: s.ProficiencyLevel.ToString(),
                yearsOfExperience: s.YearsOfExperience,
                extractedByClaude: s.ExtractedByClaude))
            .ToList();

        if (skillDtos.Count > 0)
            await _userSkillRepository.CreateRangeAsync(skillDtos, ct);
    }
}
