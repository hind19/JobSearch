using FluentAssertions;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Worker;
using Microsoft.Extensions.Logging;
using Moq;

namespace JobSearch.Worker.Tests;

public class WorkerRunTests
{
    private readonly Mock<ILogger<WorkerRun>> _mockLogger;
    private readonly Mock<IUserProfileService> _mockUserProfileService;
    private readonly Mock<IJobSiteQueryService> _mockJobSiteQueryService;
    private readonly Mock<IJobSearchAgent> _mockJobSearchAgent;
    private readonly Mock<IJobMatchService> _mockJobMatchService;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly WorkerRun _sut;

    public WorkerRunTests()
    {
        _mockLogger = new Mock<ILogger<WorkerRun>>();
        _mockUserProfileService = new Mock<IUserProfileService>();
        _mockJobSiteQueryService = new Mock<IJobSiteQueryService>();
        _mockJobSearchAgent = new Mock<IJobSearchAgent>();
        _mockJobMatchService = new Mock<IJobMatchService>();
        _mockEmailSender = new Mock<IEmailSender>();

        _sut = new WorkerRun(
            _mockLogger.Object,
            _mockUserProfileService.Object,
            _mockJobSiteQueryService.Object,
            _mockJobSearchAgent.Object,
            _mockJobMatchService.Object,
            _mockEmailSender.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoUsers_ReturnsErrorCode()
    {
        // Arrange
        _mockUserProfileService
            .Setup(s => s.GetCurrentUserIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoProfile_ReturnsErrorCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserProfileService
            .Setup(s => s.GetCurrentUserIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _mockUserProfileService
            .Setup(s => s.GetProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileDto?)null);

        _mockJobSiteQueryService
            .Setup(s => s.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<JobSiteDto>());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoActiveJobSites_ReturnsErrorCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreateTestProfile(userId);

        _mockUserProfileService
            .Setup(s => s.GetCurrentUserIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _mockUserProfileService
            .Setup(s => s.GetProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _mockJobSiteQueryService
            .Setup(s => s.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<JobSiteDto>());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidUserAndNoMatches_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreateTestProfile(userId);
        var jobSites = new List<JobSiteDto> { CreateTestJobSite() };
        var agentResult = new JobSearchAgentResult(
            toolCallCount: 5,
            jobsSaved: 3,
            matchesCreated: 0,
            completed: true);

        SetupSuccessfulRun(userId, profile, jobSites, agentResult);

        _mockJobMatchService
            .Setup(s => s.GetUnnotifiedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserJobMatchDto>());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithMatchesAndSuccessfulEmail_MarksAsNotified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreateTestProfile(userId);
        var jobSites = new List<JobSiteDto> { CreateTestJobSite() };
        var agentResult = new JobSearchAgentResult(
            toolCallCount: 10,
            jobsSaved: 5,
            matchesCreated: 2,
            completed: true);

        var job1 = CreateTestJob();
        var job2 = CreateTestJob();
        var matches = new List<UserJobMatchDto>
        {
            CreateTestMatch(userId, job1),
            CreateTestMatch(userId, job2)
        };

        var user = new UserDto(userId, "test@example.com", "Test User", DateTime.UtcNow, true);

        SetupSuccessfulRun(userId, profile, jobSites, agentResult);

        _mockJobMatchService
            .Setup(s => s.GetUnnotifiedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        _mockUserProfileService
            .Setup(s => s.GetUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockEmailSender
            .Setup(s => s.SendJobDigestAsync(userId, user.Email, matches, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailSendResult(sent: true, sentEmailId: Guid.NewGuid(), errorMessage: null));

        _mockJobMatchService
            .Setup(s => s.MarkAsNotifiedAsync(userId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().Be(0);
        _mockJobMatchService.Verify(
            s => s.MarkAsNotifiedAsync(
                userId,
                It.Is<List<Guid>>(ids => ids.Count == 2 && ids.Contains(job1.Id) && ids.Contains(job2.Id)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailedEmailSend_DoesNotMarkAsNotified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreateTestProfile(userId);
        var jobSites = new List<JobSiteDto> { CreateTestJobSite() };
        var agentResult = new JobSearchAgentResult(
            toolCallCount: 10,
            jobsSaved: 5,
            matchesCreated: 2,
            completed: true);

        var job = CreateTestJob();
        var matches = new List<UserJobMatchDto> { CreateTestMatch(userId, job) };
        var user = new UserDto(userId, "test@example.com", "Test User", DateTime.UtcNow, true);

        SetupSuccessfulRun(userId, profile, jobSites, agentResult);

        _mockJobMatchService
            .Setup(s => s.GetUnnotifiedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        _mockUserProfileService
            .Setup(s => s.GetUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockEmailSender
            .Setup(s => s.SendJobDigestAsync(userId, user.Email, matches, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailSendResult(sent: false, sentEmailId: Guid.Empty, errorMessage: "SMTP error"));

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().Be(0); // Still returns success (worker run completed)
        _mockJobMatchService.Verify(
            s => s.MarkAsNotifiedAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithIncompleteAgentRun_StillReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreateTestProfile(userId);
        var jobSites = new List<JobSiteDto> { CreateTestJobSite() };
        var agentResult = new JobSearchAgentResult(
            toolCallCount: 100,
            jobsSaved: 50,
            matchesCreated: 25,
            completed: false); // Hit iteration cap

        SetupSuccessfulRun(userId, profile, jobSites, agentResult);

        _mockJobMatchService
            .Setup(s => s.GetUnnotifiedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserJobMatchDto>());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().Be(0); // Returns success even though not completed
    }

    [Fact]
    public async Task ExecuteAsync_WithMatchesButUserDisappeared_SkipsEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreateTestProfile(userId);
        var jobSites = new List<JobSiteDto> { CreateTestJobSite() };
        var agentResult = new JobSearchAgentResult(
            toolCallCount: 10,
            jobsSaved: 5,
            matchesCreated: 2,
            completed: true);

        var matches = new List<UserJobMatchDto> { CreateTestMatch(userId, CreateTestJob()) };

        SetupSuccessfulRun(userId, profile, jobSites, agentResult);

        _mockJobMatchService
            .Setup(s => s.GetUnnotifiedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        _mockUserProfileService
            .Setup(s => s.GetUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().Be(0);
        _mockEmailSender.Verify(
            s => s.SendJobDigestAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<UserJobMatchDto>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupSuccessfulRun(
        Guid userId,
        UserProfileDto profile,
        List<JobSiteDto> jobSites,
        JobSearchAgentResult agentResult)
    {
        _mockUserProfileService
            .Setup(s => s.GetCurrentUserIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _mockUserProfileService
            .Setup(s => s.GetProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _mockJobSiteQueryService
            .Setup(s => s.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobSites);

        _mockJobSearchAgent
            .Setup(a => a.RunAsync(userId, profile, jobSites, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agentResult);
    }

    private static UserProfileDto CreateTestProfile(Guid userId)
    {
        return new UserProfileDto(
            id: Guid.NewGuid(),
            userId: userId,
            claudeReadyProfile: "Test profile",
            desiredRoles: "Developer",
            desiredSalaryMin: 100000,
            desiredSalaryMax: 150000,
            salaryCurrency: "USD",
            locationPreference: "Remote",
            cvParsedAt: DateTime.UtcNow,
            cvFileHash: "hash",
            updatedAt: DateTime.UtcNow);
    }

    private static JobSiteDto CreateTestJobSite()
    {
        return new JobSiteDto(
            id: Guid.NewGuid(),
            name: "Test Site",
            baseUrl: "https://example.com",
            isActive: true,
            scrapeConfig: ScrapeConfigDto.Empty);
    }

    private static JobDto CreateTestJob()
    {
        return new JobDto(
            id: Guid.NewGuid(),
            jobSiteId: Guid.NewGuid(),
            externalId: "ext-123",
            url: $"https://example.com/job/{Guid.NewGuid()}",
            title: "Test Job",
            company: "Test Company",
            location: "Remote",
            salaryRaw: "100k",
            descriptionRaw: "Description",
            postedAt: DateTime.UtcNow,
            foundAt: DateTime.UtcNow,
            urlHash: "hash");
    }

    private static UserJobMatchDto CreateTestMatch(Guid userId, JobDto job)
    {
        return new UserJobMatchDto(
            id: Guid.NewGuid(),
            userId: userId,
            jobId: job.Id,
            relevanceScore: 85,
            relevanceReason: "Good match",
            wasNotified: false,
            notifiedAt: null,
            foundInRunAt: DateTime.UtcNow,
            job: job);
    }
}
