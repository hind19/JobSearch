// ADR-0009
using FluentAssertions;
using JobSearch.Business.Services;
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using Moq;

namespace JobSearch.Business.Tests.Services;

public class JobRejectionServiceTests
{
    private readonly Mock<IUserJobRejectionRepository> _mockRepository;
    private readonly JobRejectionService _sut;

    public JobRejectionServiceTests()
    {
        _mockRepository = new Mock<IUserJobRejectionRepository>();
        _sut = new JobRejectionService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetMostRecentAnalysisDateAsync_DelegatesToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expected = new DateTime(2026, 7, 25, 10, 0, 0, DateTimeKind.Utc);

        _mockRepository
            .Setup(r => r.GetMostRecentAnalyzedDateAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetMostRecentAnalysisDateAsync(userId);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetMostRecentAnalysisDateAsync_WithNoRejections_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetMostRecentAnalyzedDateAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        // Act
        var result = await _sut.GetMostRecentAnalysisDateAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRejectedJobsAsync_MapsPersistencePageToApplicationPage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = new DateTime(2026, 7, 25);
        var job = CreateTestJob();

        var rejection = new UserJobRejectionPersistenceDto(
            id: Guid.NewGuid(),
            userId: userId,
            jobId: job.Id,
            relevanceScore: 35,
            relevanceReason: "Missing required Python experience",
            analyzedAt: date.AddHours(9),
            job: job);

        var persistencePage = new RejectedJobsPagePersistenceDto(
            items: [rejection],
            totalCount: 45); // 3 pages at pageSize 20

        _mockRepository
            .Setup(r => r.GetByUserIdAndDateAsync(userId, date, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistencePage);

        // Act
        var result = await _sut.GetRejectedJobsAsync(userId, date, 1, 20);

        // Assert
        result.TotalCount.Should().Be(45);
        result.TotalPages.Should().Be(3);
        result.Items.Should().HaveCount(1);
        result.Items[0].JobUrl.Should().Be(job.Url);
        result.Items[0].JobTitle.Should().Be(job.Title);
        result.Items[0].RelevanceReason.Should().Be("Missing required Python experience");
    }

    [Fact]
    public async Task GetRejectedJobsAsync_WithNoRejectionsOnDate_ReturnsEmptyPageWithOneTotalPage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = new DateTime(2026, 7, 25);

        _mockRepository
            .Setup(r => r.GetByUserIdAndDateAsync(userId, date, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RejectedJobsPagePersistenceDto(items: [], totalCount: 0));

        // Act
        var result = await _sut.GetRejectedJobsAsync(userId, date, 1, 20);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        // TotalPages is 1 even at zero rows, so "page 1 of 1" reads
        // sensibly rather than "page 1 of 0".
        result.TotalPages.Should().Be(1);
    }

    private static JobPersistenceDto CreateTestJob()
    {
        var jobId = Guid.NewGuid();
        return new JobPersistenceDto(
            id: jobId,
            jobSiteId: Guid.NewGuid(),
            externalId: null,
            url: $"https://example.com/job/{jobId}",
            title: "Backend Developer",
            company: "Test Company",
            location: "Remote",
            salaryRaw: "100k",
            descriptionRaw: "Test Description",
            postedAt: DateTime.UtcNow,
            foundAt: DateTime.UtcNow,
            urlHash: Guid.NewGuid().ToString());
    }
}
