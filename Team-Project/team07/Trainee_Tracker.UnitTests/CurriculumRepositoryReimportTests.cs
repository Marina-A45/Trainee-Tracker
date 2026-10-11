using Microsoft.EntityFrameworkCore;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Repository;

namespace Trainee_Tracker.UnitTests;

public class CurriculumRepositoryReimportTests
{
    /// <summary>
    /// Verifies that a lesson omitted during reimport remains stored and is
    /// marked inactive instead of being deleted.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void Edit_RemovedLesson_MarksLessonInactiveAndKeepsIt()
    {
        // Arrange
        using var context = CreateContext();
        var lesson = CreateLesson();
        var curriculum = CreateCurriculum(lesson);

        context.curriculums.Add(curriculum);
        context.SaveChanges();

        var repository = new CurriculumRepository(context);

        // Act
        repository.Edit(curriculum.id, new List<Lesson>());

        // Assert
        var savedCurriculum = context.curriculums
            .AsNoTracking()
            .Include(entry => entry.lessons)
            .Single(entry => entry.id == curriculum.id);

        var savedLesson = Assert.Single(savedCurriculum.lessons);

        Assert.Equal(lesson.id, savedLesson.id);
        Assert.False(savedLesson.isActive);
        Assert.True(
            context.lessons.Any(entry => entry.id == lesson.id));
    }

    /// <summary>
    /// Verifies that reimporting a curriculum does not delete progress for a
    /// lesson which a trainee has already started.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void Edit_RemovedStartedLesson_KeepsTraineeProgress()
    {
        // Arrange
        using var context = CreateContext();
        var lesson = CreateLesson();
        var curriculum = CreateCurriculum(lesson);

        var trainee = new Trainee(
            "trainee-reimport@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)))
        {
            curriculumId = curriculum.id
        };

        var progress = new LessonProgress
        {
            traineeId = trainee.guid,
            lessonId = lesson.id,
            status = LessonStatus.Started,
            title = lesson.title,
            timeEstimate = lesson.timeEstimate,
            url = lesson.url,
            isActive = true,
            position = lesson.defaultPosition
        };

        context.curriculums.Add(curriculum);
        context.users.Add(trainee);
        context.progresses.Add(progress);
        context.SaveChanges();

        var repository = new CurriculumRepository(context);

        // Act
        repository.Edit(curriculum.id, new List<Lesson>());

        // Assert
        var savedProgress = context.progresses
            .AsNoTracking()
            .Single(entry => entry.id == progress.id);

        Assert.Equal(LessonStatus.Started, savedProgress.status);
        Assert.Equal(trainee.guid, savedProgress.traineeId);
        Assert.Equal(lesson.id, savedProgress.lessonId);
    }

    /// <summary>
    /// Creates an isolated in-memory database for a reimport test.
    /// Author: Dennis Axt
    /// </summary>
    private static TraineeTrackerContext CreateContext()
    {
        var context = new ReimportTestContext();
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Creates an active lesson used by the reimport tests.
    /// Author: Dennis Axt
    /// </summary>
    private static Lesson CreateLesson()
    {
        return new Lesson
        {
            id = Guid.NewGuid(),
            title = "Lesson removed during reimport",
            timeEstimate = 1,
            url = "https://example.com/reimport",
            isActive = true,
            defaultPosition = 1
        };
    }

    /// <summary>
    /// Creates a curriculum containing the supplied lesson.
    /// Author: Dennis Axt
    /// </summary>
    private static Curriculum CreateCurriculum(Lesson lesson)
    {
        return new Curriculum
        {
            name = "Reimport Test Curriculum",
            updated = DateTime.UtcNow,
            lessons = new List<Lesson> { lesson }
        };
    }

    private sealed class ReimportTestContext : TraineeTrackerContext
    {
        private readonly string _databaseName =
            $"CurriculumReimport-{Guid.NewGuid()}";

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(_databaseName);
        }
    }
}