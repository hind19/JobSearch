using FluentAssertions;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Business.Services;
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using Moq;

namespace JobSearch.Business.Tests.Services;

public class JobServiceTests
{
    private readonly Mock<IUserJobMatchRepository> _mockRepository;
    private readonly JobService _sut;

    public JobServiceTests()
    {
        _mockRepository = new Mock<IUserJobMatchRepository>();
        _sut = new JobService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetMatchesByUserIdAsync_WithValidUserId_ReturnsMatches()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var job = new JobPersistenceDto(
            id: Guid.NewGuid(),
            jobSiteId: Guid.NewGuid(),
            externalId: "ext-123",
            url: "https://example.com/job/123",
            title: "Software Developer",
            company: "Test Company",
            location: "Remote",
            salaryRaw: "100k-150k",
            descriptionRaw: "Great job",
            postedAt: DateTime.UtcNow,
            foundAt: DateTime.UtcNow,
            urlHash: "HASH123");

        var persistenceMatches = new List<UserJobMatchPersistenceDto>
        {
            new(
                id: Guid.NewGuid(),
                userId: userId,
                jobId: job.Id,
                relevanceScore: 85,
                relevanceReason: "Good match",
                wasNotified: false,
                notifiedAt: null,
                foundInRunAt: DateTime.UtcNow,
                job: job)
        };

        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistenceMatches);

        // Act
        var result = await _sut.GetMatchesByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(userId);
        result[0].RelevanceScore.Should().Be(85);
        result[0].Job.Title.Should().Be("Software Developer");
    }

    [Fact]
    public async Task GetMatchesByUserIdAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserJobMatchPersistenceDto>());

        // Act
        var result = await _sut.GetMatchesByUserIdAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMatchesByUserIdAsync_WithCancellationToken_PassesToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, ct))
            .ReturnsAsync(new List<UserJobMatchPersistenceDto>());

        // Act
        await _sut.GetMatchesByUserIdAsync(userId, ct);

        // Assert
        _mockRepository.Verify(
            r => r.GetByUserIdAsync(userId, ct),
            Times.Once);
    }

    [Fact]
    public async Task GetMatchesByUserIdAsync_WithMultipleMatches_ReturnsAllMatches()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var job1 = CreateTestJob("Job 1");
        var job2 = CreateTestJob("Job 2");
        var job3 = CreateTestJob("Job 3");

        var persistenceMatches = new List<UserJobMatchPersistenceDto>
        {
            CreateTestMatch(userId, job1, 90),
            CreateTestMatch(userId, job2, 75),
            CreateTestMatch(userId, job3, 80)
        };

        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistenceMatches);

        // Act
        var result = await _sut.GetMatchesByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(m => m.UserId == userId);
        result.Select(m => m.RelevanceScore).Should().BeEquivalentTo(new[] { 90m, 75m, 80m });
    }

    private static JobPersistenceDto CreateTestJob(string title)
    {
        return new JobPersistenceDto(
            id: Guid.NewGuid(),
            jobSiteId: Guid.NewGuid(),
            externalId: $"ext-{Guid.NewGuid()}",
            url: $"https://example.com/job/{Guid.NewGuid()}",
            title: title,
            company: "Test Company",
            location: "Remote",
            salaryRaw: "100k",
            descriptionRaw: "Description",
            postedAt: DateTime.UtcNow,
            foundAt: DateTime.UtcNow,
            urlHash: Guid.NewGuid().ToString());
    }

    private static UserJobMatchPersistenceDto CreateTestMatch(Guid userId, JobPersistenceDto job, int score)
    {
        return new UserJobMatchPersistenceDto(
            id: Guid.NewGuid(),
            userId: userId,
            jobId: job.Id,
            relevanceScore: score,
            relevanceReason: "Test reason",
            wasNotified: false,
            notifiedAt: null,
            foundInRunAt: DateTime.UtcNow,
            job: job);
    }
}
