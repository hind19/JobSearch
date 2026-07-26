using FluentAssertions;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Services;
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using Moq;

namespace JobSearch.Business.Tests.Services;

public class UserProfileServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserProfileRepository> _mockUserProfileRepository;
    private readonly Mock<IUserSkillRepository> _mockUserSkillRepository;
    private readonly Mock<ICvParser> _mockCvParser;
    private readonly Mock<IQuestionGenerator> _mockQuestionGenerator;
    private readonly Mock<IProfileEnricher> _mockProfileEnricher;
    private readonly UserProfileService _sut;

    public UserProfileServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserProfileRepository = new Mock<IUserProfileRepository>();
        _mockUserSkillRepository = new Mock<IUserSkillRepository>();
        _mockCvParser = new Mock<ICvParser>();
        _mockQuestionGenerator = new Mock<IQuestionGenerator>();
        _mockProfileEnricher = new Mock<IProfileEnricher>();

        _sut = new UserProfileService(
            _mockUserRepository.Object,
            _mockUserProfileRepository.Object,
            _mockUserSkillRepository.Object,
            _mockCvParser.Object,
            _mockQuestionGenerator.Object,
            _mockProfileEnricher.Object);
    }

    [Fact]
    public async Task AnalyzeCvAsync_WithFailedParsing_ReturnsFailureResult()
    {
        // Arrange
        var pdfBytes = new byte[] { 1, 2, 3 };
        var cvResult = CvAnalysisResult.Failure("Failed to parse CV");

        _mockCvParser
            .Setup(p => p.ParseCvAsync(pdfBytes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvResult);

        // Act
        var result = await _sut.AnalyzeCvAsync(pdfBytes);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Failed to parse CV");
        _mockQuestionGenerator.Verify(
            g => g.GetClarifyingQuestionsAsync(It.IsAny<CvAnalysisResult>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserSkillsAsync_WithExistingSkills_ReturnsSkills()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var persistenceSkills = new List<UserSkillPersistenceDto>
        {
            new(Guid.NewGuid(), userId, "C#", "Expert", 5.0m, true),
            new(Guid.NewGuid(), userId, "SQL", "Intermediate", 3.0m, true)
        };

        _mockUserSkillRepository
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistenceSkills);

        // Act
        var result = await _sut.GetUserSkillsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result[0].SkillName.Should().Be("C#");
        result[1].SkillName.Should().Be("SQL");
    }

    [Fact]
    public async Task GetUserSkillsAsync_WithNoSkills_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserSkillRepository
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserSkillPersistenceDto>());

        // Act
        var result = await _sut.GetUserSkillsAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindUserByEmailAsync_WithExistingEmail_ReturnsUserId()
    {
        // Arrange
        var email = "john@example.com";
        var userId = Guid.NewGuid();
        var user = new UserPersistenceDto(
            id: userId,
            email: email,
            name: "John Doe",
            createdAt: DateTime.UtcNow,
            isActive: true);

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.FindUserByEmailAsync(email);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public async Task FindUserByEmailAsync_WithNonExistingEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPersistenceDto?)null);

        // Act
        var result = await _sut.FindUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserIdAsync_WithExistingUsers_ReturnsMostRecentUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserPersistenceDto(
            id: userId,
            email: "recent@example.com",
            name: "Recent User",
            createdAt: DateTime.UtcNow,
            isActive: true);

        _mockUserRepository
            .Setup(r => r.GetMostRecentlyModifiedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetCurrentUserIdAsync();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public async Task GetCurrentUserIdAsync_WithNoUsers_ReturnsNull()
    {
        // Arrange
        _mockUserRepository
            .Setup(r => r.GetMostRecentlyModifiedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPersistenceDto?)null);

        // Act
        var result = await _sut.GetCurrentUserIdAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_WithExistingProfile_ReturnsProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var persistenceProfile = new UserProfilePersistenceDto(
            id: Guid.NewGuid(),
            userId: userId,
            claudeReadyProfile: "Profile text",
            desiredRoles: "Developer,Architect",
            desiredSalaryMin: 100000,
            desiredSalaryMax: 150000,
            salaryCurrency: "USD",
            locationPreference: "Remote",
            cvParsedAt: DateTime.UtcNow,
            cvFileHash: "hash123",
            updatedAt: DateTime.UtcNow);

        _mockUserProfileRepository
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistenceProfile);

        // Act
        var result = await _sut.GetProfileAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.ClaudeReadyProfile.Should().Be("Profile text");
        result.DesiredSalaryMin.Should().Be(100000);
    }

    [Fact]
    public async Task GetProfileAsync_WithNonExistingProfile_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserProfileRepository
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfilePersistenceDto?)null);

        // Act
        var result = await _sut.GetProfileAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserPersistenceDto(
            id: userId,
            email: "user@example.com",
            name: "Test User",
            createdAt: DateTime.UtcNow,
            isActive: true);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("user@example.com");
        result.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task GetUserAsync_WithNonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPersistenceDto?)null);

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        result.Should().BeNull();
    }
}
