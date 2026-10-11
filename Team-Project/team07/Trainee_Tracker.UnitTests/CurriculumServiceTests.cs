using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Trainee_Tracker.Service;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Repository;

namespace Trainee_Tracker.UnitTests;

/// <summary>
/// Author: Carl Böhm-Sturm
/// </summary>
public class CurriculumServiceTests {
    /// <summary>
    /// Wraps the given JSON string in an in-memory stream, as expected by the service.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="json">The JSON content to wrap.</param>
    /// <returns>A stream containing the UTF-8 encoded JSON.</returns>
    private static Stream JsonStream(string json) {
        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>
    /// Creates a CurriculumService with the given repository mock and a mocked SignalR hub context.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="mockRepo">The mocked curriculum repository to inject.</param>
    /// <returns>The service under test.</returns>
    private static CurriculumService CreateService(Mock<ICurriculumRepository> mockRepo) {
        var mockClientProxy = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        var mockHub = new Mock<IHubContext<CurriculumHub>>();
        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);
        return new CurriculumService(mockRepo.Object, mockHub.Object);
    }

    /// <summary>
    /// Verifies that a valid JSON payload creates a curriculum whose lessons are mapped
    /// with title, url, estimate and defaultPosition, and that it is saved exactly once.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task CreateWithJson_ValidJson_ReturnsCurriculumWithMappedLessons() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        var service = CreateService(mockRepo);
        var json = """
        [
            { "id": 1, "title": "Intro", "url": "https://example.com/intro", "estimate": 1.5, "deprecated": false },
            { "id": 2, "title": "Basics", "url": "https://example.com/basics", "estimate": 3.0, "deprecated": false }
        ]
        """;

        // Act
        var result = await service.CreateWithJson("Onboarding", JsonStream(json));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Onboarding", result.name);
        Assert.NotEqual(Guid.Empty, result.id);
        Assert.Equal(2, result.lessons.Count);

        var first = result.lessons.First();
        Assert.Equal("Intro", first.title);
        Assert.Equal("https://example.com/intro", first.url);
        Assert.Equal(1.5, first.timeEstimate);
        Assert.Equal(1, first.defaultPosition);

        var second = result.lessons.Last();
        Assert.Equal("Basics", second.title);
        Assert.Equal(2, second.defaultPosition);

        mockRepo.Verify(repo => repo.Create(result), Times.Once);
    }

    /// <summary>
    /// Verifies that the numeric lesson id from the JSON is mapped into the lesson's Guid.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task CreateWithJson_MapsIntIdToGuid() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        var service = CreateService(mockRepo);
        var json = """[{ "id": 255, "title": "Lesson", "url": "https://example.com", "estimate": 1.0, "deprecated": false }]""";

        // Act
        var result = await service.CreateWithJson("Curriculum", JsonStream(json));

        // Assert
        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-0000000000ff"), result.lessons.First().id);
    }

    /// <summary>
    /// Verifies that a missing estimate in the JSON defaults the lesson's time estimate to zero.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task CreateWithJson_NullEstimate_DefaultsToZero() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        var service = CreateService(mockRepo);
        var json = """[{ "id": 1, "title": "Lesson", "url": "https://example.com", "deprecated": false }]""";

        // Act
        var result = await service.CreateWithJson("Curriculum", JsonStream(json));

        // Assert
        Assert.Equal(0.0, result.lessons.First().timeEstimate);
    }

    /// <summary>
    /// Verifies that the deprecated flag from the JSON is inverted into the lesson's isActive flag.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task CreateWithJson_DeprecatedFlag_InvertsToIsActive() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        var service = CreateService(mockRepo);
        var json = """
        [
            { "id": 1, "title": "Old", "url": "https://example.com/old", "estimate": 1.0, "deprecated": true },
            { "id": 2, "title": "Current", "url": "https://example.com/current", "estimate": 1.0, "deprecated": false }
        ]
        """;

        // Act
        var result = await service.CreateWithJson("Curriculum", JsonStream(json));

        // Assert
        Assert.False(result.lessons.First().isActive);
        Assert.True(result.lessons.Last().isActive);
    }

    /// <summary>
    /// Verifies that an empty JSON array creates a curriculum without lessons but still saves it.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task CreateWithJson_EmptyArray_CreatesCurriculumWithNoLessons() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        var service = CreateService(mockRepo);

        // Act
        var result = await service.CreateWithJson("Empty", JsonStream("[]"));

        // Assert
        Assert.Empty(result.lessons);
        mockRepo.Verify(repo => repo.Create(result), Times.Once);
    }

    /// <summary>
    /// Verifies that malformed JSON causes a parsing exception and nothing is saved.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task CreateWithJson_MalformedJson_ThrowsJsonParsingException() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        var service = CreateService(mockRepo);

        // Act
        await Assert.ThrowsAsync<JsonException>(
            () => service.CreateWithJson("Broken", JsonStream("{not json")));

        // Assert
        mockRepo.Verify(
            repo => repo.Create(It.IsAny<Curriculum>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that a database error during creation is wrapped into a
    /// "Database Save Failed" exception carrying the inner cause.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task CreateWithJson_RepoThrowsDbUpdateException_ThrowsDatabaseSaveFailed() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        mockRepo
            .Setup(repo => repo.Create(It.IsAny<Curriculum>()))
            .Throws(new DbUpdateException("outer message", new Exception("inner cause")));
        var service = CreateService(mockRepo);
        var json = """[{ "id": 1, "title": "Lesson", "url": "https://example.com", "estimate": 1.0, "deprecated": false }]""";

        // Act
        var ex = await Assert.ThrowsAsync<Exception>(
            () => service.CreateWithJson("Curriculum", JsonStream(json)));

        // Assert
        Assert.Equal("Database Save Failed! Cause: inner cause", ex.Message);
        Assert.IsType<DbUpdateException>(ex.InnerException);
    }

    /// <summary>
    /// Verifies that a valid JSON payload is mapped to lessons and passed to the
    /// repository's Edit for the given curriculum.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task EditWithJson_ValidJson_MapsLessonsAndCallsRepoEdit() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        List<Lesson>? savedLessons = null;
        mockRepo
            .Setup(repo => repo.Edit(It.IsAny<Guid>(), It.IsAny<List<Lesson>>()))
            .Callback<Guid, List<Lesson>>((_, lessons) => savedLessons = lessons);
        var service = CreateService(mockRepo);
        var curriculumId = Guid.NewGuid();
        var json = """
        [
            { "id": 1, "title": "Intro", "url": "https://example.com/intro", "estimate": 2.0, "deprecated": false },
            { "id": 2, "title": "Basics", "url": "https://example.com/basics", "deprecated": true }
        ]
        """;

        // Act
        var result = await service.EditWithJson(curriculumId, JsonStream(json));

        // Assert
        Assert.True(result);
        mockRepo.Verify(repo => repo.Edit(curriculumId, It.IsAny<List<Lesson>>()), Times.Once);

        Assert.NotNull(savedLessons);
        Assert.Equal(2, savedLessons.Count);
        Assert.Equal("Intro", savedLessons[0].title);
        Assert.Equal(2.0, savedLessons[0].timeEstimate);
        Assert.Equal(1, savedLessons[0].defaultPosition);
        Assert.Equal(0.0, savedLessons[1].timeEstimate);
        Assert.False(savedLessons[1].isActive);
        Assert.Equal(2, savedLessons[1].defaultPosition);
    }

    /// <summary>
    /// Verifies that malformed JSON causes a parsing exception and the repository is not called.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task EditWithJson_MalformedJson_ThrowsJsonParsingException() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        var service = CreateService(mockRepo);

        // Act
        await Assert.ThrowsAsync<JsonException>(
            () => service.EditWithJson(
                Guid.NewGuid(),
                JsonStream("{not json")));

        // Assert
        mockRepo.Verify(
            repo => repo.Edit(
                It.IsAny<Guid>(),
                It.IsAny<List<Lesson>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that a database error during edit is wrapped into a "Database Save Failed"
    /// exception, even when the DbUpdateException has no inner exception.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task EditWithJson_RepoThrowsDbUpdateException_ThrowsDatabaseSaveFailed() {
        // Arrange
        var mockRepo = new Mock<ICurriculumRepository>();
        mockRepo
            .Setup(repo => repo.Edit(It.IsAny<Guid>(), It.IsAny<List<Lesson>>()))
            .Throws(new DbUpdateException("no inner exception here"));
        var service = CreateService(mockRepo);
        var json = """[{ "id": 1, "title": "Lesson", "url": "https://example.com", "estimate": 1.0, "deprecated": false }]""";

        // Act
        var ex = await Assert.ThrowsAsync<Exception>(
            () => service.EditWithJson(Guid.NewGuid(), JsonStream(json)));

        // Assert
        Assert.Equal("Database Save Failed! Cause: no inner exception here", ex.Message);
        Assert.IsType<DbUpdateException>(ex.InnerException);
    }
}
