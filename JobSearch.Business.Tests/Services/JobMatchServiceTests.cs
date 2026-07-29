using FluentAssertions;
using JobSearch.Application.Abstractions.Configuration;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Business.Services;
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using Microsoft.Extensions.Options;
using Moq;

namespace JobSearch.Business.Tests.Services;

public class JobMatchServiceTests
{
    private readonly Mock<IUserJobMatchRepository> _mockMatchRepository;
    private readonly Mock<IUserJobRejectionRepository> _mockRejectionRepository;
    private readonly Mock<IJobRepository> _mockJobRepository;
    private readonly Mock<IOptions<AnthropicSettings>> _mockAnthropicSettings;
    private readonly JobMatchService _sut;

    public JobMatchServiceTests()
    {
        _mockMatchRepository = new Mock<IUserJobMatchRepository>();
        _mockRejectionRepository = new Mock<IUserJobRejectionRepository>();
        _mockJobRepository = new Mock<IJobRepository>();
        _mockAnthropicSettings = new Mock<IOptions<AnthropicSettings>>();

        var settings = new AnthropicSettings
        {
            RelevanceThreshold = 70
        };
        _mockAnthropicSettings.Setup(x => x.Value).Returns(settings);

        _sut = new JobMatchService(
            _mockMatchRepository.Object,
            _mockRejectionRepository.Object,
            _mockJobRepository.Object,
            _mockAnthropicSettings.Object);
    }

    [Fact]
    public async Task TryCreateMatchAsync_WithScoreAboveThreshold_CreatesMatch()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var score = 85;
        var reason = "Great match with required skills";

        var job = CreateTestJob(jobId);
        _mockJobRepository
            .Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _mockMatchRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserJobMatchPersistenceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserJobMatchPersistenceDto dto, CancellationToken _) => dto);

        // Act
        var result = await _sut.TryCreateMatchAsync(userId, jobId, score, reason);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.JobId.Should().Be(jobId);
        result.RelevanceScore.Should().Be(85);
        result.RelevanceReason.Should().Be(reason);
        result.WasNotified.Should().BeFalse();

        // ADR-0009: no rejection should be written on the match path.
        _mockRejectionRepository.Verify(
            r => r.CreateAsync(It.IsAny<UserJobRejectionPersistenceDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ADR-0009: below-threshold now persists a rejection instead of
    // silently discarding the score/reason. This replaces the old
    // "...ReturnsNull" test, which asserted GetByIdAsync/CreateAsync
    // were *never* called on this path — that's no longer true.
    [Fact]
    public async Task TryCreateMatchAsync_WithScoreBelowThreshold_PersistsRejectionAndReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var score = 50; // Below threshold of 70
        var reason = "Partial match";

        var job = CreateTestJob(jobId);
        _mockJobRepository
            .Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _mockRejectionRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserJobRejectionPersistenceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserJobRejectionPersistenceDto dto, CancellationToken _) => dto);

        // Act
        var result = await _sut.TryCreateMatchAsync(userId, jobId, score, reason);

        // Assert
        result.Should().BeNull();

        _mockJobRepository.Verify(
            r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockRejectionRepository.Verify(
            r => r.CreateAsync(
                It.Is<UserJobRejectionPersistenceDto>(dto =>
                    dto.UserId == userId &&
                    dto.JobId == jobId &&
                    dto.RelevanceScore == 50 &&
                    dto.RelevanceReason == reason),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMatchRepository.Verify(
            r => r.CreateAsync(It.IsAny<UserJobMatchPersistenceDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TryCreateMatchAsync_WithScoreEqualToThreshold_CreatesMatch()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var score = 70; // Exactly at threshold
        var reason = "Minimum acceptable match";

        var job = CreateTestJob(jobId);
        _mockJobRepository
            .Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _mockMatchRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserJobMatchPersistenceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserJobMatchPersistenceDto dto, CancellationToken _) => dto);

        // Act
        var result = await _sut.TryCreateMatchAsync(userId, jobId, score, reason);

        // Assert
        result.Should().NotBeNull();
        result!.RelevanceScore.Should().Be(70);
    }

    [Fact]
    public async Task TryCreateMatchAsync_WithScoreAbove100_ClampsTo100()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var score = 150; // Above max
        var reason = "Excellent match";

        var job = CreateTestJob(jobId);
        _mockJobRepository
            .Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _mockMatchRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserJobMatchPersistenceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserJobMatchPersistenceDto dto, CancellationToken _) => dto);

        // Act
        var result = await _sut.TryCreateMatchAsync(userId, jobId, score, reason);

        // Assert
        result.Should().NotBeNull();
        result!.RelevanceScore.Should().Be(100);
    }

    [Fact]
    public async Task TryCreateMatchAsync_WithNegativeScore_ClampsTo0AndPersistsRejection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var score = -10;
        var reason = "Negative match somehow";

        var job = CreateTestJob(jobId);
        _mockJobRepository
            .Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _mockRejectionRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserJobRejectionPersistenceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserJobRejectionPersistenceDto dto, CancellationToken _) => dto);

        // Act
        var result = await _sut.TryCreateMatchAsync(userId, jobId, score, reason);

        // Assert
        result.Should().BeNull(); // Clamped to 0, which is below threshold

        _mockRejectionRepository.Verify(
            r => r.CreateAsync(
                It.Is<UserJobRejectionPersistenceDto>(dto => dto.RelevanceScore == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TryCreateMatchAsync_WithNonExistentJob_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var score = 85;
        var reason = "Good match";

        _mockJobRepository
            .Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobPersistenceDto?)null);

        // Act
        Func<Task> act = async () => await _sut.TryCreateMatchAsync(userId, jobId, score, reason);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ADR-0009: the existence guard now also applies to the
    // below-threshold path — a missing job should throw before any
    // rejection is written.
    [Fact]
    public async Task TryCreateMatchAsync_WithScoreBelowThresholdAndNonExistentJob_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var score = 40;
        var reason = "Weak match";

        _mockJobRepository
            .Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobPersistenceDto?)null);

        // Act
        Func<Task> act = async () => await _sut.TryCreateMatchAsync(userId, jobId, score, reason);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");

        _mockRejectionRepository.Verify(
            r => r.CreateAsync(It.IsAny<UserJobRejectionPersistenceDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUnnotifiedAsync_WithUnnotifiedMatches_ReturnsMatches()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var job1 = CreateTestJob(Guid.NewGuid());
        var job2 = CreateTestJob(Guid.NewGuid());

        var persistenceMatches = new List<UserJobMatchPersistenceDto>
        {
            CreateTestMatch(userId, job1, 80, false),
            CreateTestMatch(userId, job2, 90, false)
        };

        _mockMatchRepository
            .Setup(r => r.GetUnnotifiedByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistenceMatches);

        // Act
        var result = await _sut.GetUnnotifiedAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => !m.WasNotified);
    }

    [Fact]
    public async Task GetUnnotifiedAsync_WithNoUnnotifiedMatches_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockMatchRepository
            .Setup(r => r.GetUnnotifiedByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserJobMatchPersistenceDto>());

        // Act
        var result = await _sut.GetUnnotifiedAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task MarkAsNotifiedAsync_WithJobIds_CallsRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        _mockMatchRepository
            .Setup(r => r.MarkAsNotifiedAsync(userId, jobIds, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.MarkAsNotifiedAsync(userId, jobIds);

        // Assert
        _mockMatchRepository.Verify(
            r => r.MarkAsNotifiedAsync(userId, jobIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsNotifiedAsync_WithEmptyJobIdsList_CallsRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobIds = new List<Guid>();

        _mockMatchRepository
            .Setup(r => r.MarkAsNotifiedAsync(userId, jobIds, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.MarkAsNotifiedAsync(userId, jobIds);

        // Assert
        _mockMatchRepository.Verify(
            r => r.MarkAsNotifiedAsync(userId, jobIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static JobPersistenceDto CreateTestJob(Guid jobId)
    {
        return new JobPersistenceDto(
            id: jobId,
            jobSiteId: Guid.NewGuid(),
            externalId: $"ext-{jobId}",
            url: $"https://example.com/job/{jobId}",
            title: "Test Job",
            company: "Test Company",
            location: "Test Location",
            salaryRaw: "100k",
            descriptionRaw: "Test Description",
            postedAt: DateTime.UtcNow,
            foundAt: DateTime.UtcNow,
            urlHash: Guid.NewGuid().ToString());
    }

    private static UserJobMatchPersistenceDto CreateTestMatch(
        Guid userId,
        JobPersistenceDto job,
        int score,
        bool wasNotified)
    {
        return new UserJobMatchPersistenceDto(
            id: Guid.NewGuid(),
            userId: userId,
            jobId: job.Id,
            relevanceScore: score,
            relevanceReason: "Test reason",
            wasNotified: wasNotified,
            notifiedAt: wasNotified ? DateTime.UtcNow : null,
            foundInRunAt: DateTime.UtcNow,
            job: job);
    }
}
