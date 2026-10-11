using Moq;
using Trainee_Tracker.Service;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Enums;

namespace Trainee_Tracker.UnitTests;

/// <summary>
/// Author: Carl Böhm-Sturm
/// </summary>
public class TraineeProgressServiceContractTests {
    private readonly Mock<ITraineeProgressRepository> mockProgressRepo;
    private readonly Mock<ILessonProgressRepository> mockLessonRepo;
    private readonly Mock<ITraineeRepository> mockTraineeRepo;
    private readonly Mock<IDateTimeTrackingApiClient> mockApiClient;
    private readonly TraineeProgressService service;
    private readonly Mock<IWorkingHoursRepository> mockWorkHourRepo;

    public TraineeProgressServiceContractTests() {
        mockProgressRepo = new Mock<ITraineeProgressRepository>();
        mockLessonRepo = new Mock<ILessonProgressRepository>();
        mockTraineeRepo = new Mock<ITraineeRepository>();
        mockApiClient = new Mock<IDateTimeTrackingApiClient>();
        mockWorkHourRepo = new Mock<IWorkingHoursRepository>();

        service = new TraineeProgressService(
            mockProgressRepo.Object,
            mockLessonRepo.Object,
            mockTraineeRepo.Object,
            mockApiClient.Object,
            mockWorkHourRepo.Object
        );
    }

    /// <summary>
    /// Verifies the Vorbedingung "Ein Benutzer mit der ID traineeId existiert im System
    /// und hat die Rolle Trainee": when no such trainee exists, calcAndSaveProgress must
    /// fail without contacting the time tracking API or persisting a TraineeProgress record.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task CalcAndSaveProgress_ThrowsAndSavesNothing_WhenTraineeNotFound() {
        // Arrange
        var traineeId = Guid.NewGuid();
        mockTraineeRepo.Setup(r => r.GetTraineeById(traineeId)).Returns((Trainee?)null);

        // Act
        var act = () => service.calcAndSaveProgress(traineeId);

        // Assert
        await Assert.ThrowsAsync<Exception>(act);
        mockApiClient.Verify(
            api => api.GetWorkinghours(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()),
            Times.Never);
        mockProgressRepo.Verify(
            r => r.UpsertAsync(It.IsAny<TraineeProgress>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies the Vorbedingung "...und hat die Rolle Trainee": a User that exists but is
    /// not a Trainee (e.g. a Mentor) must be rejected the same way as a missing user, and
    /// no statistics may be calculated or saved for it.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    [Fact]
    public async Task CalcAndSaveProgress_ThrowsAndSavesNothing_WhenUserIsNotATrainee() {
        // Arrange
        var traineeId = Guid.NewGuid();
        var notATrainee = new Trainee {
            guid = traineeId,
            active = true,
            knowledge = "",
            Id = traineeId.ToString(),
            role = Role.Mentor,
            Email = "mentor@makandra.de",
            startDate = new DateOnly(2026, 3, 1)
        };
        mockTraineeRepo.Setup(r => r.GetTraineeById(traineeId)).Returns(notATrainee);

        // Act
        var act = () => service.calcAndSaveProgress(traineeId);

        // Assert
        await Assert.ThrowsAsync<Exception>(act);
        mockProgressRepo.Verify(
            r => r.UpsertAsync(It.IsAny<TraineeProgress>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
