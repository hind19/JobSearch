using FluentAssertions;
using JobSearch.Business.Services;

namespace JobSearch.Business.Tests.Services;

public class JobUrlHasherTests
{
    private readonly JobUrlHasher _sut;

    public JobUrlHasherTests()
    {
        _sut = new JobUrlHasher();
    }

    [Fact]
    public void Compute_WithValidUrl_ReturnsNonEmptyHash()
    {
        // Arrange
        var url = "https://example.com/job/123";

        // Act
        var result = _sut.Compute(url);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(64); // SHA256 produces 64 hex characters
    }

    [Fact]
    public void Compute_WithSameUrl_ReturnsConsistentHash()
    {
        // Arrange
        var url = "https://example.com/job/123";

        // Act
        var result1 = _sut.Compute(url);
        var result2 = _sut.Compute(url);

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void Compute_WithDifferentUrls_ReturnsDifferentHashes()
    {
        // Arrange
        var url1 = "https://example.com/job/123";
        var url2 = "https://example.com/job/456";

        // Act
        var result1 = _sut.Compute(url1);
        var result2 = _sut.Compute(url2);

        // Assert
        result1.Should().NotBe(result2);
    }

    [Fact]
    public void Compute_WithUrlContainingWhitespace_TrimsAndProducesHash()
    {
        // Arrange
        var urlWithWhitespace = "  https://example.com/job/123  ";
        var urlTrimmed = "https://example.com/job/123";

        // Act
        var result1 = _sut.Compute(urlWithWhitespace);
        var result2 = _sut.Compute(urlTrimmed);

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void Compute_WithEmptyString_ReturnsHash()
    {
        // Arrange
        var url = "";

        // Act
        var result = _sut.Compute(url);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(64);
    }

    [Fact]
    public void Compute_WithSpecialCharacters_ProducesValidHash()
    {
        // Arrange
        var url = "https://example.com/job?id=123&lang=ru&utm_source=test";

        // Act
        var result = _sut.Compute(url);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(64);
        result.Should().MatchRegex("^[0-9A-F]+$"); // Hex string
    }

    [Fact]
    public void Compute_WithUnicodeCharacters_ProducesValidHash()
    {
        // Arrange
        var url = "https://example.com/работа/программист";

        // Act
        var result = _sut.Compute(url);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(64);
        result.Should().MatchRegex("^[0-9A-F]+$");
    }
}
