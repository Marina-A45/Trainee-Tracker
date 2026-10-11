using Moq;
using Trainee_Tracker.Service;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Enums;


namespace Trainee_Tracker.UnitTests;

/// <summary>
/// Contains unit tests for the lesson progress service.
/// Verifies lesson progress retrieval and status changes for trainees.
/// </summary>
public class LessonProgressServiceTests {

    /// <summary>
    /// Verifies that a trainee can retrieve their own lesson progress.
    /// Author: Marina Achner
    /// </summary>
    [Fact]
    public void GetAllForTrainee_ReturnsOwnLessonProgresses() {
        // Create mocked repository and authentication service dependencies.
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        // Create a trainee who requests their own lesson progresses.
        var trainee = new Trainee(
            "trainee@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        // Create test lesson progress entries assigned to the trainee.
        var testList = new List<LessonProgress>
        {
            new LessonProgress
            {
                traineeId = trainee.guid,
                status = LessonStatus.Open,
                title = "Test Lesson 1",
                timeEstimate = 2,
                url = "test-url-1",
                isActive = true,
                position = 1
            },
            new LessonProgress
            {
                traineeId = trainee.guid,
                status = LessonStatus.Started,
                title = "Test Lesson 2",
                timeEstimate = 3,
                url = "test-url-2",
                isActive = true,
                position = 2
            }
        };

        // Configure authentication mock to return the created trainee.
        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(trainee);

        // Configure repository mock to return the trainee's lesson progresses.
        mockRepo
            .Setup(repo => repo.GetAll(trainee.guid))
            .Returns(testList);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Retrieve lesson progresses for the trainee.
        var result = service.getAll(trainee.guid).ToList();

        // Verify that the returned progresses match the expected data.
        Assert.Equal(2, result.Count);
        Assert.Equal("Test Lesson 1", result[0].title);
        Assert.Equal("Test Lesson 2", result[1].title);

        mockRepo.Verify(
            repo => repo.GetAll(trainee.guid),
            Times.Once);
    }

    // Author: Marina Achner

    /// <summary>
    /// Verifies that changing the status of a lesson progress updates the status correctly.
    /// </summary>
    /// <returns>
    /// A completed test task.
    /// </returns>
    [Fact]
    public void SetStatus_ChangesLessonProgressStatus() {
        // Create mocked repository and authentication service dependencies.
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        // Create a trainee who updates their lesson progress.
        var trainee = new Trainee(
            "trainee@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        // Create a lesson progress entry with an initial status.
        var progressId = Guid.NewGuid();

        var progress = new LessonProgress {
            id = progressId,
            traineeId = trainee.guid,
            status = LessonStatus.Open,
            title = "Test Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };

        // Configure authentication mock to return the trainee.
        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(trainee);

        // Configure repository mock to return the lesson progress.
        mockRepo
            .Setup(repo => repo.GetById(progressId))
            .Returns(progress);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Change the lesson progress status from Open to Started.
        service.setStatus(progressId, LessonStatus.Started);

        // Verify that the status was updated correctly.
        Assert.Equal(LessonStatus.Started, progress.status);

        // Verify that the changed progress was saved.
        mockRepo.Verify(
            repo => repo.Edit(progress),
            Times.Once);
    }

    /// <summary>
    /// Verifies that a trainee cannot use a forbidden lesson status transition.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void SetStatus_RejectsForbiddenTraineeTransition() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var trainee = new Trainee(
            "trainee@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var progressId = Guid.NewGuid();

        var progress = new LessonProgress {
            id = progressId,
            traineeId = trainee.guid,
            status = LessonStatus.Open,
            title = "Test Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(trainee);

        mockRepo
            .Setup(repo => repo.GetById(progressId))
            .Returns(progress);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => service.setStatus(
                progressId,
                LessonStatus.Accepted));

        // Assert
        Assert.Contains(
            "Trainees cannot change",
            exception.Message);

        Assert.Equal(LessonStatus.Open, progress.status);

        mockRepo.Verify(
            repo => repo.Edit(It.IsAny<LessonProgress>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that a trainee cannot modify lesson progress owned
    /// by another trainee.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void SetStatus_RejectsProgressOwnedByAnotherTrainee() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var currentTrainee = new Trainee(
            "current.trainee@test.com",
            "Current",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var otherTrainee = new Trainee(
            "other.trainee@test.com",
            "Other",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var progressId = Guid.NewGuid();

        var progress = new LessonProgress {
            id = progressId,
            traineeId = otherTrainee.guid,
            status = LessonStatus.Open,
            title = "Another Trainee's Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(currentTrainee);

        mockRepo
            .Setup(repo => repo.GetById(progressId))
            .Returns(progress);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => service.setStatus(
                progressId,
                LessonStatus.Started));

        // Assert
        Assert.Contains(
            "their own lesson progress",
            exception.Message);

        Assert.Equal(LessonStatus.Open, progress.status);

        mockRepo.Verify(
            repo => repo.Edit(It.IsAny<LessonProgress>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that an assigned mentor can reject a finished lesson
    /// when a rejection reason is provided.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void SetStatus_AssignedMentorCanRejectLessonWithReason() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var mentor = new Mentor(
            "mentor@test.com",
            "Test",
            "Mentor");

        var trainee = new Trainee(
            "trainee@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var progressId = Guid.NewGuid();

        var progress = new LessonProgress {
            id = progressId,
            traineeId = trainee.guid,
            status = LessonStatus.Finished,
            title = "Test Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(mentor);

        mockAuth
            .Setup(auth => auth.GetAssignedTrainees())
            .ReturnsAsync(new[] { trainee });

        mockRepo
            .Setup(repo => repo.GetById(progressId))
            .Returns(progress);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        service.setStatus(
            progressId,
            LessonStatus.Rejected,
            "The exercise requires further improvements.");

        // Assert
        Assert.Equal(LessonStatus.Rejected, progress.status);
        Assert.Equal(
            "The exercise requires further improvements.",
            progress.rejectionReason);

        mockRepo.Verify(
            repo => repo.Edit(progress),
            Times.Once);
    }

    /// <summary>
    /// Verifies that rejecting a lesson without a reason is refused.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void SetStatus_RejectsLessonRejectionWithoutReason() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var mentor = new Mentor(
            "mentor@test.com",
            "Test",
            "Mentor");

        var trainee = new Trainee(
            "trainee@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var progressId = Guid.NewGuid();

        var progress = new LessonProgress {
            id = progressId,
            traineeId = trainee.guid,
            status = LessonStatus.Finished,
            title = "Test Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(mentor);

        mockAuth
            .Setup(auth => auth.GetAssignedTrainees())
            .ReturnsAsync(new[] { trainee });

        mockRepo
            .Setup(repo => repo.GetById(progressId))
            .Returns(progress);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => service.setStatus(
                progressId,
                LessonStatus.Rejected));

        // Assert
        Assert.Contains(
            "rejection reason is required",
            exception.Message);

        Assert.Equal(LessonStatus.Finished, progress.status);
        Assert.Empty(progress.rejectionReason);

        mockRepo.Verify(
            repo => repo.Edit(It.IsAny<LessonProgress>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that a mentor cannot use a forbidden lesson status transition.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void SetStatus_RejectsForbiddenMentorTransition() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var mentor = new Mentor(
            "mentor@test.com",
            "Test",
            "Mentor");

        var trainee = new Trainee(
            "trainee@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var progressId = Guid.NewGuid();

        var progress = new LessonProgress {
            id = progressId,
            traineeId = trainee.guid,
            status = LessonStatus.Open,
            title = "Test Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(mentor);

        mockAuth
            .Setup(auth => auth.GetAssignedTrainees())
            .ReturnsAsync(new[] { trainee });

        mockRepo
            .Setup(repo => repo.GetById(progressId))
            .Returns(progress);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => service.setStatus(
                progressId,
                LessonStatus.Accepted));

        // Assert
        Assert.Contains(
            "Mentors cannot change",
            exception.Message);

        Assert.Equal(LessonStatus.Open, progress.status);

        mockRepo.Verify(
            repo => repo.Edit(It.IsAny<LessonProgress>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that a trainee cannot read lesson progress belonging
    /// to another trainee.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void GetAllForTrainee_ThrowsWhenReadingAnotherTrainee() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var currentTrainee = new Trainee(
            "current@test.com",
            "Current",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var otherTrainee = new Trainee(
            "other@test.com",
            "Other",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(currentTrainee);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => service.getAll(otherTrainee.guid));

        // Assert
        Assert.Contains(
            "only read their own",
            exception.Message);

        mockRepo.Verify(
            repo => repo.GetAll(It.IsAny<Guid>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that a mentor can read lesson progress belonging
    /// to an assigned trainee.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void GetAllForMentor_ReturnsProgressOfAssignedTrainee() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var mentor = new Mentor(
            "mentor@test.com",
            "Test",
            "Mentor");

        var assignedTrainee = new Trainee(
            "assigned@test.com",
            "Assigned",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var progress = new LessonProgress {
            traineeId = assignedTrainee.guid,
            status = LessonStatus.Rejected,
            rejectionReason = "Please revise the solution.",
            title = "Test Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(mentor);

        mockAuth
            .Setup(auth => auth.GetAssignedTrainees())
            .ReturnsAsync(new[] { assignedTrainee });

        mockRepo
            .Setup(repo => repo.GetAll(assignedTrainee.guid))
            .Returns(new[] { progress });

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        var result = service.getAll(assignedTrainee.guid).Single();

        // Assert
        Assert.Equal(LessonStatus.Rejected, result.status);
        Assert.Equal(
            "Please revise the solution.",
            result.rejectionReason);
    }

    /// <summary>
    /// Verifies that a mentor cannot read lesson progress belonging
    /// to an unassigned trainee.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void GetAllForMentor_ThrowsWhenTraineeIsNotAssigned() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var mentor = new Mentor(
            "mentor@test.com",
            "Test",
            "Mentor");

        var unassignedTrainee = new Trainee(
            "unassigned@test.com",
            "Unassigned",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(mentor);

        mockAuth
            .Setup(auth => auth.GetAssignedTrainees())
            .ReturnsAsync(Array.Empty<Trainee>());

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => service.getAll(unassignedTrainee.guid));

        // Assert
        Assert.Contains(
            "assigned trainees",
            exception.Message);

        mockRepo.Verify(
            repo => repo.GetAll(It.IsAny<Guid>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that an administrator can read lesson progress
    /// belonging to all trainees.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void GetAllForAdmin_ReturnsProgressOfAllTrainees() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var admin = new Admin(
            "admin@test.com",
            "Test",
            "Admin");

        var progressList = new[]
        {
            new LessonProgress {
                traineeId = Guid.NewGuid(),
                status = LessonStatus.Open,
                title = "First Lesson",
                timeEstimate = 1,
                url = "https://example.com/first",
                isActive = true,
                position = 1
            },
            new LessonProgress {
                traineeId = Guid.NewGuid(),
                status = LessonStatus.Rejected,
                rejectionReason = "Revise this lesson.",
                title = "Second Lesson",
                timeEstimate = 2,
                url = "https://example.com/second",
                isActive = true,
                position = 2
            }
        };

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(admin);

        mockRepo
            .Setup(repo => repo.GetAll())
            .Returns(progressList);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        var result = service.getAll().ToList();

        // Assert
        Assert.Equal(2, result.Count);

        mockRepo.Verify(
            repo => repo.GetAll(),
            Times.Once);
    }

    /// <summary>
    /// Verifies that a mentor can save an individual lesson order
    /// for an assigned trainee.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void SetIndividualOrder_UpdatesAssignedTraineePositions() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var mentor = new Mentor(
            "mentor@test.com",
            "Test",
            "Mentor");

        var trainee = new Trainee(
            "trainee@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var firstProgress = new LessonProgress {
            traineeId = trainee.guid,
            status = LessonStatus.Open,
            title = "First Lesson",
            timeEstimate = 1,
            url = "https://example.com/first",
            isActive = true,
            position = 1
        };

        var secondProgress = new LessonProgress {
            traineeId = trainee.guid,
            status = LessonStatus.Open,
            title = "Second Lesson",
            timeEstimate = 1,
            url = "https://example.com/second",
            isActive = true,
            position = 2
        };

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(mentor);

        mockAuth
            .Setup(auth => auth.GetAssignedTrainees())
            .ReturnsAsync(new[] { trainee });

        mockRepo
            .Setup(repo => repo.GetAll(trainee.guid))
            .Returns(new[] { firstProgress, secondProgress });

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        service.setIndividualOrder(
            trainee.guid,
            new[] { secondProgress.id, firstProgress.id });

        // Assert
        Assert.Equal(1, secondProgress.position);
        Assert.Equal(2, firstProgress.position);

        mockRepo.Verify(
            repo => repo.Edit(It.IsAny<LessonProgress>()),
            Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that a mentor cannot change the individual lesson order
    /// of an unassigned trainee.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void SetIndividualOrder_ThrowsForUnassignedTrainee() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var mentor = new Mentor(
            "mentor@test.com",
            "Test",
            "Mentor");

        var unassignedTrainee = new Trainee(
            "unassigned@test.com",
            "Unassigned",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(mentor);

        mockAuth
            .Setup(auth => auth.GetAssignedTrainees())
            .ReturnsAsync(Array.Empty<Trainee>());

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => service.setIndividualOrder(
                unassignedTrainee.guid,
                Array.Empty<Guid>()));

        // Assert
        Assert.Contains(
            "assigned trainees",
            exception.Message);

        mockRepo.Verify(
            repo => repo.Edit(It.IsAny<LessonProgress>()),
            Times.Never);
    }
    /// <summary>
    /// Verifies that submitting feedback changes an accepted lesson
    /// to Rated.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void RateAfterFeedback_ChangesAcceptedLessonToRated() {
        // Arrange
        var mockRepo = new Mock<ILessonProgressRepository>();
        var mockAuth = new Mock<ILoginService>();

        var trainee = new Trainee(
            "trainee@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));

        var progressId = Guid.NewGuid();

        var progress = new LessonProgress {
            id = progressId,
            traineeId = trainee.guid,
            status = LessonStatus.Accepted,
            title = "Test Lesson",
            timeEstimate = 2,
            url = "https://example.com/lesson",
            isActive = true,
            position = 1
        };

        mockAuth
            .Setup(auth => auth.GetCurrentUser())
            .ReturnsAsync(trainee);

        mockRepo
            .Setup(repo => repo.GetById(progressId))
            .Returns(progress);

        var service = new LessonProgressService(
            mockRepo.Object,
            mockAuth.Object);

        // Act
        service.rateAfterFeedback(progressId);

        // Assert
        Assert.Equal(LessonStatus.Rated, progress.status);

        mockRepo.Verify(
            repo => repo.Edit(progress),
            Times.Once);
    }
}