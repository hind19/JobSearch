using FluentAssertions;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Business.Services;
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using Moq;

namespace JobSearch.Business.Tests.Services;

public class JobIngestServiceTests
{
    private readonly Mock<IJobRepository> _mockJobRepository;
    private readonly Mock<IJobUrlHasher> _mockUrlHasher;
    private readonly JobIngestService _sut;

    public JobIngestServiceTests()
    {
        _mockJobRepository = new Mock<IJobRepository>();
        _mockUrlHasher = new Mock<IJobUrlHasher>();
        _sut = new JobIngestService(_mockJobRepository.Object, _mockUrlHasher.Object);
    }

    [Fact]
    public async Task ExistsByUrlAsync_WithExistingUrl_ReturnsTrue()
    {
        // Arrange
        var url = "https://example.com/job/123";
        var urlHash = "HASH123";

        _mockUrlHasher
            .Setup(h => h.Compute(url))
            .Returns(urlHash);

        _mockJobRepository
            .Setup(r => r.ExistsByUrlHashAsync(urlHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ExistsByUrlAsync(url);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUrlAsync_WithNonExistingUrl_ReturnsFalse()
    {
        // Arrange
        var url = "https://example.com/job/456";
        var urlHash = "HASH456";

        _mockUrlHasher
            .Setup(h => h.Compute(url))
            .Returns(urlHash);

        _mockJobRepository
            .Setup(r => r.ExistsByUrlHashAsync(urlHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ExistsByUrlAsync(url);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithValidJob_RecomputesHashAndCreatesJob()
    {
        // Arrange
        var jobDto = new JobDto(
            id: Guid.NewGuid(),
            jobSiteId: Guid.NewGuid(),
            externalId: null, // ADR-0008: nullable, reserved for future use
            url: "https://example.com/job/123",
            title: "Software Developer",
            company: "Test Company",
            location: "Remote",
            salaryRaw: "100k",
            descriptionRaw: "Great job",
            postedAt: DateTime.UtcNow,
            foundAt: DateTime.UtcNow,
            urlHash: "WRONG_HASH"); // Should be ignored and recomputed

        var correctHash = "CORRECT_HASH";
        _mockUrlHasher
            .Setup(h => h.Compute(jobDto.Url))
            .Returns(correctHash);

        var createdPersistenceDto = new JobPersistenceDto(
            id: jobDto.Id,
            jobSiteId: jobDto.JobSiteId,
            externalId: jobDto.ExternalId,
            url: jobDto.Url,
            title: jobDto.Title,
            company: jobDto.Company,
            location: jobDto.Location,
            salaryRaw: jobDto.SalaryRaw,
            descriptionRaw: jobDto.DescriptionRaw,
            postedAt: jobDto.PostedAt,
            foundAt: jobDto.FoundAt,
            urlHash: correctHash);

        _mockJobRepository
            .Setup(r => r.CreateAsync(It.IsAny<JobPersistenceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPersistenceDto);

        // Act
        var result = await _sut.CreateAsync(jobDto);

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().Be(jobDto.Url);
        result.Title.Should().Be(jobDto.Title);

        // Verify that the repository was called with the correct hash
        _mockJobRepository.Verify(
            r => r.CreateAsync(
                It.Is<JobPersistenceDto>(j => j.UrlHash == correctHash),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithJobContainingWrongHash_IgnoresProvidedHashAndRecomputes()
    {
        // Arrange
        var jobDto = new JobDto(
            id: Guid.NewGuid(),
            jobSiteId: Guid.NewGuid(),
            externalId: null, // ADR-0008: nullable, reserved for future use
            url: "https://example.com/job/789",
            title: "DevOps Engineer",
            company: "Another Company",
            location: "NYC",
            salaryRaw: "120k",
            descriptionRaw: "DevOps position",
            postedAt: DateTime.UtcNow,
            foundAt: DateTime.UtcNow,
            urlHash: "ATTACKER_SUPPLIED_HASH");

        var correctHash = "SECURE_COMPUTED_HASH";
        _mockUrlHasher
            .Setup(h => h.Compute(jobDto.Url))
            .Returns(correctHash);

        _mockJobRepository
            .Setup(r => r.CreateAsync(It.IsAny<JobPersistenceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobPersistenceDto dto, CancellationToken _) => dto);

        // Act
        await _sut.CreateAsync(jobDto);

        // Assert
        _mockUrlHasher.Verify(h => h.Compute(jobDto.Url), Times.Once);
        _mockJobRepository.Verify(
            r => r.CreateAsync(
                It.Is<JobPersistenceDto>(j => j.UrlHash == correctHash && j.UrlHash != "ATTACKER_SUPPLIED_HASH"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithCancellationToken_PassesToRepository()
    {
        // Arrange
        var jobDto = CreateTestJobDto();
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _mockUrlHasher
            .Setup(h => h.Compute(It.IsAny<string>()))
            .Returns("HASH");

        _mockJobRepository
            .Setup(r => r.CreateAsync(It.IsAny<JobPersistenceDto>(), ct))
            .ReturnsAsync((JobPersistenceDto dto, CancellationToken _) => 
                new JobPersistenceDto(
                    dto.Id, dto.JobSiteId, dto.ExternalId, dto.Url, dto.Title,
                    dto.Company, dto.Location, dto.SalaryRaw, dto.DescriptionRaw,
                    dto.PostedAt, dto.FoundAt, dto.UrlHash));

        // Act
        await _sut.CreateAsync(jobDto, ct);

        // Assert
        _mockJobRepository.Verify(
            r => r.CreateAsync(It.IsAny<JobPersistenceDto>(), ct),
            Times.Once);
    }

    [Fact]
    public async Task ExistsByUrlAsync_WithCancellationToken_PassesToRepository()
    {
        // Arrange
        var url = "https://example.com/job/999";
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        _mockUrlHasher
            .Setup(h => h.Compute(url))
            .Returns("HASH999");

        _mockJobRepository
            .Setup(r => r.ExistsByUrlHashAsync(It.IsAny<string>(), ct))
            .ReturnsAsync(false);

        // Act
        await _sut.ExistsByUrlAsync(url, ct);

        // Assert
        _mockJobRepository.Verify(
            r => r.ExistsByUrlHashAsync("HASH999", ct),
            Times.Once);
    }

    private static JobDto CreateTestJobDto()
    {
        return new JobDto(
            id: Guid.NewGuid(),
            jobSiteId: Guid.NewGuid(),
            externalId: null, // ADR-0008: nullable, reserved for future use
            url: "https://example.com/test",
            title: "Test Job",
            company: "Test Co",
            location: "Test Location",
            salaryRaw: "Test Salary",
            descriptionRaw: "Test Description",
            postedAt: DateTime.UtcNow,
            foundAt: DateTime.UtcNow,
            urlHash: "TEST_HASH");
    }
}
