using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Enums;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Mapping;
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
    private readonly IProfileEnricher _profileEnricher;

    public UserProfileService(
        IUserRepository userRepository,
        IUserProfileRepository userProfileRepository,
        IUserSkillRepository userSkillRepository,
        ICvParser cvParser,
        IQuestionGenerator questionGenerator,
        IProfileEnricher profileEnricher)
    {
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _userSkillRepository = userSkillRepository;
        _cvParser = cvParser;
        _questionGenerator = questionGenerator;
        _profileEnricher = profileEnricher;
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

    public async Task<List<UserSkillDto>> GetUserSkillsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var dtos = await _userSkillRepository.GetByUserIdAsync(userId, ct);
        return dtos.Select(BusinessMapper.ToDto).ToList();
    }

    public async Task<Guid?> FindUserByEmailAsync(
        string email,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, ct);
        return user?.Id;
    }

    public async Task<Guid?> GetCurrentUserIdAsync(
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetMostRecentlyModifiedAsync(ct);
        return user?.Id;
    }

    public async Task<UserProfileDto?> GetProfileAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var dto = await _userProfileRepository.GetByUserIdAsync(userId, ct);
        return dto is null ? null : BusinessMapper.ToDto(dto);
    }

    public async Task<UserDto?> GetUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var dto = await _userRepository.GetByIdAsync(userId, ct);
        return dto is null ? null : BusinessMapper.ToDto(dto);
    }

    public async Task SaveProfileAsync(
     Guid userId,
     CvAnalysisResult result,
     List<ClarifyingQuestionDto> answers,
     CancellationToken ct = default)
    {
        // 1. Обогащаем ClaudeReadyProfile ответами на уточняющие вопросы
        var enrichedProfile = await _profileEnricher.EnrichAsync(
            result.ClaudeReadyProfile, answers, ct);

        // 2. Извлекаем зарплатные данные из ответов
        var salaryAnswer = answers.FirstOrDefault(
            q => q.AnswerType == Application.Abstractions.Enums.AnswerType.NumericRange
              && q.RangeFrom is not null);

        // 3. Создаём или обновляем пользователя
        // TODO: заменить на реальный Email/Name когда появится UserContext
        // ADR-0002: create vs update must be distinguished so CreatedAt is
        // preserved and UpdatedAt is refreshed correctly by the repository
        // on every save (was previously always CreateAsync, which failed
        // on re-save due to the UQ_Users_Email / PK constraints).
        var existingUser = await _userRepository.GetByIdAsync(userId, ct);

        var userDto = new UserPersistenceDto(
            id: userId,
            email: result.Candidate.Email ?? existingUser?.Email
                ?? $"{userId}@placeholder.local",
            name: result.Candidate.FullName ?? existingUser?.Name ?? "Unknown",
            createdAt: existingUser?.CreatedAt ?? DateTime.UtcNow,
            isActive: true);

        if (existingUser is null)
            await _userRepository.CreateAsync(userDto, ct);
        else
            await _userRepository.UpdateAsync(userDto, ct);

        // 4. Создаём профиль
        var profileDto = new UserProfilePersistenceDto(
            id: Guid.NewGuid(),
            userId: userId,
            claudeReadyProfile: enrichedProfile,
            desiredRoles: string.Join(",", result.DesiredRoles),
            desiredSalaryMin: salaryAnswer?.RangeFrom is not null
                ? (int)salaryAnswer.RangeFrom : null,
            desiredSalaryMax: salaryAnswer?.RangeTo is not null
                ? (int)salaryAnswer.RangeTo : null,
            salaryCurrency: salaryAnswer?.Currency ?? string.Empty,
            locationPreference: string.Empty, // TODO: вытащить из answers если будет такой вопрос
            cvParsedAt: DateTime.UtcNow,
            cvFileHash: string.Empty, // TODO: передавать хэш файла из VM
            updatedAt: DateTime.UtcNow);

        await _userProfileRepository.CreateAsync(profileDto, ct);

        // TODO: уточнить поведение при перезаписи профиля (Create vs Update для User и UserProfile)

        // 5. Сохраняем скиллы: сначала удаляем старые, затем вставляем актуальные
        await _userSkillRepository.DeleteAllByUserIdAsync(userId, ct);
        var skillPersistenceDtos = BusinessMapper.ToPersistenceDto(result.Skills);
        await _userSkillRepository.CreateRangeAsync(skillPersistenceDtos, ct);
    }
}
