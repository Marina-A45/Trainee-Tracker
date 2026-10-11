using Microsoft.AspNetCore.Mvc;
using Moq;
using Trainee_Tracker.API.Models;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.Service;
using TraineeTracker.Controllers;

namespace Trainee_Tracker.UnitTests;

public class MentorControllerAccessTests {

    /// <summary>
    /// Verifies that an administrator can load all active trainees
    /// in the shared trainee progress view.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public async Task TraineeProgress_AdminLoadsAllActiveTrainees() {
        // Arrange
        var loginService = new Mock<ILoginService>();
        var progressService = new Mock<ILessonProgressService>();
        var traineeService = new Mock<ITraineeService>();
        var traineeProgressService = new Mock<ITraineeProgressService>();

        var admin = new Admin(
            "admin@test.com",
            "Test",
            "Admin");

        var activeTrainee = CreateTrainee(
            "active@test.com",
            "Active",
            "Trainee");

        var inactiveTrainee = CreateTrainee(
            "inactive@test.com",
            "Inactive",
            "Trainee",
            false);

        var progress = CreateProgress(activeTrainee.guid);

        loginService
            .Setup(service => service.GetCurrentUser())
            .ReturnsAsync(admin);

        traineeService
            .Setup(service => service.GetAllTrainees())
            .Returns(new List<Trainee> {
                activeTrainee,
                inactiveTrainee
            });

        progressService
            .Setup(service => service.getAll(activeTrainee.guid))
            .Returns(new List<LessonProgress> { progress });

        var controller = new MentorController(
            loginService.Object,
            progressService.Object,
            traineeProgressService.Object,
            traineeService.Object
            );

        // Act
        var result = await controller.TraineeProgress(
            activeTrainee.guid);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model =
            Assert.IsType<MentorTraineeProgressViewModel>(
                viewResult.Model);

        Assert.True(model.IsAdmin);
        Assert.Same(activeTrainee, model.SelectedTrainee);
        Assert.Same(activeTrainee, Assert.Single(model.AvailableTrainees));
        Assert.Same(progress, Assert.Single(model.Progresses));

        loginService.Verify(
            service => service.GetAssignedTrainees(),
            Times.Never);
    }

    /// <summary>
    /// Verifies that a mentor can load only assigned trainees
    /// in the trainee progress view.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public async Task TraineeProgress_MentorLoadsOnlyAssignedTrainees() {
        // Arrange
        var loginService = new Mock<ILoginService>();
        var progressService = new Mock<ILessonProgressService>();
        var traineeService = new Mock<ITraineeService>();
        var traineeProgressService = new Mock<ITraineeProgressService>();

        var mentor = new Mentor(
            "mentor@test.com",
            "Test",
            "Mentor");

        var assignedTrainee = CreateTrainee(
            "assigned@test.com",
            "Assigned",
            "Trainee");

        loginService
            .Setup(service => service.GetCurrentUser())
            .ReturnsAsync(mentor);

        loginService
            .Setup(service => service.GetAssignedTrainees())
            .ReturnsAsync(new List<Trainee> { assignedTrainee });

        progressService
            .Setup(service => service.getAll(assignedTrainee.guid))
            .Returns(new List<LessonProgress>());

        var controller = new MentorController(
            loginService.Object,
            progressService.Object,
            traineeProgressService.Object,
            traineeService.Object);

        // Act
        var result = await controller.TraineeProgress(
            assignedTrainee.guid);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model =
            Assert.IsType<MentorTraineeProgressViewModel>(
                viewResult.Model);

        Assert.False(model.IsAdmin);
        Assert.Same(
            assignedTrainee,
            Assert.Single(model.AvailableTrainees));

        traineeService.Verify(
            service => service.GetAllTrainees(),
            Times.Never);
    }

    /// <summary>
    /// Verifies that an administrator can accept a finished lesson
    /// without a Mentor-Trainee assignment.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public async Task ReviewLesson_AdminAcceptsWithoutMentorAssignment() {
        // Arrange
        var loginService = new Mock<ILoginService>();
        var progressService = new Mock<ILessonProgressService>();
        var traineeService = new Mock<ITraineeService>();
        var traineeProgressService = new Mock<ITraineeProgressService>();

        var admin = new Admin(
            "admin@test.com",
            "Test",
            "Admin");

        var progressId = Guid.NewGuid();

        loginService
            .Setup(service => service.GetCurrentUser())
            .ReturnsAsync(admin);

        var controller = new MentorController(
            loginService.Object,
            progressService.Object,
            traineeProgressService.Object,
            traineeService.Object);

        // Act
        var result = await controller.ReviewLesson(
            progressId,
            "accept",
            null);

        // Assert
        var redirect =
            Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(
            nameof(MentorController.MissingFeedback),
            redirect.ActionName);

        progressService.Verify(
            service => service.setStatus(
                progressId,
                LessonStatus.Accepted,
                null),
            Times.Once);

        loginService.Verify(
            service => service.GetAssignedTrainees(),
            Times.Never);
    }

    /// <summary>
    /// Creates a trainee with valid programme dates for the test scenarios.
    /// Author: Dennis Axt
    /// </summary>
    /// <param name="email">The trainee email address.</param>
    /// <param name="firstName">The trainee first name.</param>
    /// <param name="lastName">The trainee last name.</param>
    /// <param name="active">
    /// Indicates whether the trainee account is active.
    /// </param>
    /// <returns>A trainee configured for a test scenario.</returns>
    private static Trainee CreateTrainee(
        string email,
        string firstName,
        string lastName,
        bool active = true) {
        return new Trainee(
            email,
            firstName,
            lastName,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)),
            active);
    }

    /// <summary>
    /// Creates a minimal finished lesson progress for the selected trainee.
    /// Author: Dennis Axt
    /// </summary>
    /// <param name="traineeId">
    /// The identifier of the trainee who owns the lesson progress.
    /// </param>
    /// <returns>A lesson progress configured for a test scenario.</returns>
    private static LessonProgress CreateProgress(Guid traineeId) {
        return new LessonProgress {
            traineeId = traineeId,
            status = LessonStatus.Finished,
            title = "Test Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };
    }
}