using Moq;
using Xunit;

using Trainee_Tracker.Service;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.Exceptions;
using Trainee_Tracker.Core.Models;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Service;

namespace Trainee_Tracker.UnitTests;

public class PlannerServiceTests {
    private readonly Mock<IPlannerRepository> _plannerRepo = new();
    private readonly Mock<ITraineeRepository> _traineeRepo = new();
    private readonly Mock<IWorkingHoursRepository> _workingHoursRepo = new();
    private readonly Mock<IDateTimeTrackingApiClient> _api = new();

    // Patrick Plechinger
    /// <summary>
    /// Setup planner service
    /// </summary>
    /// <returns>
    /// new PlannerService Element
    /// </returns>
    private PlannerService CreateService() {
        return new PlannerService(
            _plannerRepo.Object,
            _api.Object,
            _traineeRepo.Object,
            _workingHoursRepo.Object);
    }

    // Patrick Plechinger
    /// <summary>
    /// Helper Methods that set up a default test enviorment
    /// </summary>
    /// <returns>
    /// specifiv elements of the test eviorment
    /// </returns>
    #region Helper Methods

    /// returns default Trainee
    private static Trainee CreateTrainee() {
        return new Trainee(
            "test@test.com",
            "Max",
            "Mustermann",
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 12, 31));
    }

    /// returns default Lesson
    private static Lesson CreateLesson(
        double effort = 1,
        int position = 1) {
        return new Lesson {
            id = Guid.NewGuid(),
            title = "Lesson",
            timeEstimate = effort,
            url = "https://example.com",
            isActive = true,
            defaultPosition = position
        };
    }

    /// returns default LessonProgress
    private static LessonProgress CreateProgress(
        Lesson lesson,
        LessonStatus status) {
        return new LessonProgress {
            lessonId = lesson.id,
            traineeId = Guid.NewGuid(),

            status = status,

            title = lesson.title,
            timeEstimate = lesson.timeEstimate,
            url = lesson.url,
            isActive = lesson.isActive,
            position = lesson.defaultPosition
        };
    }

    /// returns list of default WorkingHoursEntryDtos (default working days with dates and working hours)
    private static List<WorkingHoursEntryDto> CreateWorkingHours() {
        return new()
        {
            new WorkingHoursEntryDto
            {
                date = "2025-01-01",
                working_hours = 8
            },
            new WorkingHoursEntryDto
            {
                date = "2025-01-02",
                working_hours = 8
            }
        };
    }

    /// returns default WorkingHoursResponseDto (storage of working days)
    private static WorkingHoursResponseDto CreateWorkingHoursResponse() {
        return new WorkingHoursResponseDto {
            entries = CreateWorkingHours()
        };
    }

    /// sets up the default test enviorment
    private void SetupDefaults() {
        var trainee = CreateTrainee();

        _traineeRepo
            .Setup(x => x.GetTraineeById(It.IsAny<Guid>()))
            .Returns(trainee);

        _plannerRepo
            .Setup(x => x.GetActiveLessons(It.IsAny<Guid>()))
            .Returns(new List<Lesson>());

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>());

        _api
            .Setup(x => x.GetWorkinghours(
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync(CreateWorkingHoursResponse());

        _workingHoursRepo
            .Setup(x => x.GetLatestWorkingHoursByTraineeId(It.IsAny<Guid>()))
            .ReturnsAsync(CreateWorkingHours());

        _workingHoursRepo
            .Setup(x => x.UpdateWorkingHours(
                It.IsAny<Guid>(),
                It.IsAny<List<WorkingHoursEntryDto>>()))
            .Returns(Task.CompletedTask);
    }

    #endregion

    // Patrick Plechinger
    /// <summary>
    /// test if a missing trainee returns the TraineeNotFoundException Exception
    /// </summary>
    [Fact]
    public async Task GetPlanner_TraineeMissing_Throws() {
        //set trainee null
        _traineeRepo
            .Setup(x => x.GetTraineeById(It.IsAny<Guid>()))
            .Returns((Trainee?)null);

        //test
        await Assert.ThrowsAsync<TraineeNotFoundException>(
            () => CreateService().GetPlanner(Guid.NewGuid()));
    }

    // Patrick Plechinger
    /// <summary>
    /// test if an empty planner is returned when no lessons are given
    /// </summary>
    [Fact]
    public async Task GetPlanner_NoLessons_ReturnsEmptyPlanner() {
        SetupDefaults();
        //set lessons empty
        var planner = await CreateService().GetPlanner(Guid.NewGuid());

        //test
        Assert.Empty(planner.Events);
    }

    // Patrick Plechinger
    /// <summary>
    /// test if skipped lessons are correctly ignored
    /// </summary>
    [Fact]
    public async Task GetPlanner_SkippedLesson_IsIgnored() {
        SetupDefaults();

        //create skipped lesson
        var lesson = CreateLesson();

        _plannerRepo
            .Setup(x => x.GetActiveLessons(It.IsAny<Guid>()))
            .Returns(new List<Lesson> { lesson });

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>
            {
                CreateProgress(lesson, LessonStatus.Skipped)
            });

        var planner = await CreateService().GetPlanner(Guid.NewGuid());
        //test
        Assert.Empty(planner.Events);
    }

    // Patrick Plechinger
    /// <summary>
    /// test if open lessons are startable
    /// </summary>
    [Fact]
    public async Task GetPlanner_OpenLesson_IsStartable() {
        SetupDefaults();

        //create open lesson
        var lesson = CreateLesson();

        _plannerRepo
            .Setup(x => x.GetActiveLessons(It.IsAny<Guid>()))
            .Returns(new List<Lesson> { lesson });

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>
            {
                CreateProgress(lesson, LessonStatus.Open)
            });

        var planner = await CreateService().GetPlanner(Guid.NewGuid());

        //test
        Assert.Single(planner.Events);
        Assert.True(planner.Events[0].isStartable);
        Assert.Equal("Open", planner.Events[0].status);
    }

    // Patrick Plechinger
    /// <summary>
    /// test if open lessons are correctly not startable
    /// </summary>
    [Fact]
    public async Task GetPlanner_StartedLesson_IsNotStartable() {
        SetupDefaults();

        //create started lesson
        var lesson = CreateLesson();

        _plannerRepo
            .Setup(x => x.GetActiveLessons(It.IsAny<Guid>()))
            .Returns(new List<Lesson> { lesson });

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>
            {
                CreateProgress(lesson, LessonStatus.Started)
            });

        var planner = await CreateService().GetPlanner(Guid.NewGuid());

        //test
        Assert.Single(planner.Events);
        Assert.False(planner.Events[0].isStartable);
        Assert.Equal("Started", planner.Events[0].status);
    }

    // Patrick Plechinger
    /// <summary>
    /// test if days without working hours are correctly counted as blocked
    /// </summary>
    [Fact]
    public async Task GetPlanner_ZeroWorkingHours_ReturnsBlockedDay() {
        SetupDefaults();

        //create workingDay with 0 working_hours
        _api
            .Setup(x => x.GetWorkinghours(
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync(new WorkingHoursResponseDto {
                entries = new List<WorkingHoursEntryDto>
                {
                    new()
                    {
                        date = "2025-01-01",
                        working_hours = 0
                    }
                }
            });

        var planner = await CreateService().GetPlanner(Guid.NewGuid());

        //test
        Assert.Single(planner.BlockedDays);
        Assert.Equal(
            new DateOnly(2025, 1, 1),
            planner.BlockedDays.Single());
    }

    // Patrick Plechinger
    /// <summary>
    /// test if half days are correcly normed to 4 hours and if the remaining work of a lesson is correctly transferred to the following day
    /// </summary>
    [Fact]
    public async Task GetPlanner_HalfDayWorkingHours_ReturnsHalfDayLeftOnSecondDay() {
        SetupDefaults();

        //create lesson of length 1 day
        var lesson = CreateLesson(1);

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>
            {
                CreateProgress(lesson, LessonStatus.Open)
            });

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>
            {
                CreateProgress(lesson, LessonStatus.Open)
            });
        _plannerRepo
            .Setup(x => x.GetActiveLessons(It.IsAny<Guid>()))
            .Returns(new List<Lesson> { lesson });

        //create working day of 5 hours, which is suppoed to be rounded as well as a follow up day of standard lenght
        var workingHours = new List<WorkingHoursEntryDto>
        {
                    new()
                    {
                        date = "2025-01-01",
                        working_hours = 5
                    },
                    new()
                    {
                        date = "2025-01-02",
                        working_hours = 8
                    }
        };
        _api
            .Setup(x => x.GetWorkinghours(
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync(new WorkingHoursResponseDto {
                entries = workingHours
            });

        var planner = await CreateService().GetPlanner(Guid.NewGuid());

        //test end date is equal to the second date +1 day (+1 for the calender)
        Assert.Equal(
            new DateOnly(2025, 1, 3),
            planner.Events[0].end);

        //test if the overflow of hours is handled correctly
        Assert.Equal(
            4,
            workingHours.First(x => x.date == "2025-01-02").working_hours);
    }

    // Patrick Plechinger
    /// <summary>
    /// test if lessons with zero effort put the end date at the next day
    /// </summary>
    [Fact]
    public async Task GetPlanner_ZeroEffortLesson_EndIsNextDay() {
        SetupDefaults();

        //create lesson with zero effort
        var lesson = CreateLesson(0);

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>
            {
                CreateProgress(lesson, LessonStatus.Open)
            });

        _plannerRepo
            .Setup(x => x.GetActiveLessons(It.IsAny<Guid>()))
            .Returns(new List<Lesson> { lesson });

        var planner = await CreateService().GetPlanner(Guid.NewGuid());

        var item = Assert.Single(planner.Events);

        //test if the end date is start date +1 (+1 for the calendar)
        Assert.Equal(item.start.AddDays(1), item.end);
    }

    // Patrick Plechinger
    /// <summary>
    /// test if lessons with more than one day of work take multiple days
    /// </summary>
    [Fact]
    public async Task GetPlanner_MultipleDayLesson_LastsMoreThanOneDay() {
        SetupDefaults();

        //create lesson with 2 days effort
        var lesson = CreateLesson(effort: 2);

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>
            {
                CreateProgress(lesson, LessonStatus.Open)
            });

        _plannerRepo
            .Setup(x => x.GetActiveLessons(It.IsAny<Guid>()))
            .Returns(new List<Lesson> { lesson });

        var planner = await CreateService().GetPlanner(Guid.NewGuid());

        var item = Assert.Single(planner.Events);

        //test if enddate is higher than the start date +1 (+1 for callendar)
        Assert.True(item.end > item.start.AddDays(1));
    }

    // Patrick Plechinger
    /// <summary>
    /// test if a half day lesson leaves the other half of the day remaining working hours
    /// </summary>
    [Fact]
    public async Task GetPlanner_HalfDayLesson_HalfDayWorkingHoursRemaining() {
        SetupDefaults();

        //create lesson with half day effort and default day
        var lesson = CreateLesson(.5);

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>
            {
                CreateProgress(lesson, LessonStatus.Open)
            });

        _plannerRepo
            .Setup(x => x.GetActiveLessons(It.IsAny<Guid>()))
            .Returns(new List<Lesson> { lesson });

        var workingHours = new List<WorkingHoursEntryDto>
        {
                    new()
                    {
                        date = "2025-01-01",
                        working_hours = 8
                    }
        };
        _api
            .Setup(x => x.GetWorkinghours(
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync(new WorkingHoursResponseDto {
                entries = workingHours
            });
        var planner = await CreateService().GetPlanner(Guid.NewGuid());

        var item = Assert.Single(planner.Events);

        //test if remaining hours are handled correctly
        Assert.Equal(
            4,
            workingHours.First(x => x.date == "2025-01-01").working_hours);
    }

    // Patrick Plechinger
    /// <summary>
    /// test if the properties of a lesson are correcly transferred to the corresponding planner element
    /// </summary>
    [Fact]
    public async Task GetPlanner_CopiesLessonPropertiesToPlannerItem() {
        SetupDefaults();

        var lesson = CreateLesson();
        lesson.title = "Git";
        lesson.defaultPosition = 5;
        lesson.timeEstimate = 3;
        lesson.url = "https://example.com/git";

        var progress = CreateProgress(
            lesson,
            LessonStatus.Open
        );

        _plannerRepo
            .Setup(x => x.GetActiveLessons(It.IsAny<Guid>()))
            .Returns(new List<Lesson> { lesson });

        _plannerRepo
            .Setup(x => x.GetLessonProgress(It.IsAny<Guid>()))
            .Returns(new List<LessonProgress>
            {
            progress
            });


        var planner = await CreateService().GetPlanner(Guid.NewGuid());

        var item = Assert.Single(planner.Events);

        Assert.Equal(lesson.id.ToString(), item.id);
        Assert.Equal("Git", item.title);
        Assert.Equal(5, item.position);
        Assert.Equal(3, item.timeEstimate);
        Assert.Equal("https://example.com/git", item.url);
    }

    // Patrick Plechinger
    /// <summary>
    /// test if cached working hours are used as a backup
    /// </summary>
    [Fact]
    public async Task GetPlanner_ApiFailure_UsesCachedWorkingHours() {
        SetupDefaults();

        //stop default create of workinghours api
        _api
            .Setup(x => x.GetWorkinghours(
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ThrowsAsync(new Exception());

        await CreateService().GetPlanner(Guid.NewGuid());

        //test if cache is used
        _workingHoursRepo.Verify(
            x => x.GetLatestWorkingHoursByTraineeId(It.IsAny<Guid>()),
            Times.Once);
    }

    // Patrick Plechinger
    /// <summary>
    /// test if working hours are used to create a backup
    /// </summary>
    [Fact]
    public async Task GetPlanner_ApiSuccess_UpdatesCache() {
        SetupDefaults();

        await CreateService().GetPlanner(Guid.NewGuid());

        //test if cache is saved
        _workingHoursRepo.Verify(
            x => x.UpdateWorkingHours(
                It.IsAny<Guid>(),
                It.IsAny<List<WorkingHoursEntryDto>>()),
            Times.Once);
    }
}