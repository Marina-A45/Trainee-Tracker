using Microsoft.AspNetCore.Mvc;
using Moq;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.Service;
using TraineeTracker.Controllers;

namespace Trainee_Tracker.UnitTests;

/// <summary>
/// Verifies that an administrator can open the lesson progress
/// of a selected active trainee.
/// Author: Dennis Axt
/// </summary>
public class TraineeControllerTests {
    [Fact]
    public void LessonStatus_AdminLoadsSelectedActiveTraineeProgress() {
        // Arrange
        var traineeService = new Mock<ITraineeService>();
        var lessonProgressService = new Mock<ILessonProgressService>();
        var loginService = new Mock<ILoginService>();
        var feedbackService = new Mock<IFeedbackService>();
        var burnDownChartService = new Mock<IBurnDownChartService>();
        var traineeProgressService = new Mock<ITraineeProgressService>();

        var admin = new Admin(
            "admin@test.com",
            "Test",
            "Admin");

        var trainee = new Trainee(
            "trainee@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var progress = new LessonProgress {
            traineeId = trainee.guid,
            status = LessonStatus.Open,
            title = "Test Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };

        loginService
            .Setup(service => service.GetCurrentUser())
            .ReturnsAsync(admin);

        traineeService
            .Setup(service => service.GetTraineeById(trainee.guid))
            .Returns(trainee);

        lessonProgressService
            .Setup(service => service.getAll(trainee.guid))
            .Returns(new List<LessonProgress> { progress });

        var controller = new TraineeController(
            traineeService.Object,
            lessonProgressService.Object,
            loginService.Object,
            feedbackService.Object,
            traineeProgressService.Object,
            burnDownChartService.Object);

        // Act
        var result = controller.LessonStatus(trainee.guid);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<LessonProgress>>(
            viewResult.Model);

        Assert.Same(progress, Assert.Single(model));

        lessonProgressService.Verify(
            service => service.getAll(trainee.guid),
            Times.Once);
    }

    /// <summary>
    /// Verifies that an administrator cannot open lesson progress
    /// belonging to an inactive trainee.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void LessonStatus_AdminCannotOpenInactiveTraineeProgress() {
        // Arrange
        var traineeService = new Mock<ITraineeService>();
        var burnDownChartService = new Mock<IBurnDownChartService>();
        var lessonProgressService = new Mock<ILessonProgressService>();
        var loginService = new Mock<ILoginService>();
        var feedbackService = new Mock<IFeedbackService>();
        var traineeProgressService = new Mock<ITraineeProgressService>();

        var admin = new Admin(
            "admin@test.com",
            "Test",
            "Admin");

        var inactiveTrainee = new Trainee(
            "closed@test.com",
            "Closed",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)),
            false);

        loginService
            .Setup(service => service.GetCurrentUser())
            .ReturnsAsync(admin);

        traineeService
            .Setup(service =>
                service.GetTraineeById(inactiveTrainee.guid))
            .Returns(inactiveTrainee);

        var controller = new TraineeController(
            traineeService.Object,
            lessonProgressService.Object,
            loginService.Object,
            feedbackService.Object,
            traineeProgressService.Object,
            burnDownChartService.Object);

        // Act
        var result = controller.LessonStatus(inactiveTrainee.guid);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Overview", redirect.ActionName);
        Assert.Equal("Admin", redirect.ControllerName);

        lessonProgressService.Verify(
            service => service.getAll(It.IsAny<Guid>()),
            Times.Never);
    }
}