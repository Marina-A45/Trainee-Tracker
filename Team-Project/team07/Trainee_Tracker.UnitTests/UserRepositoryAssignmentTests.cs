using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Repository;

namespace Trainee_Tracker.UnitTests;

public class UserRepositoryAssignmentTests : IDisposable {
    private readonly TraineeTrackerContext context;
    private readonly UserRepository repository;

    /// <summary>
    /// Creates an isolated test database and the repository under test.
    /// </summary>
    public UserRepositoryAssignmentTests() {
        context = new TraineeTrackerContext(true);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        repository = new UserRepository(context);
    }

    /// <summary>
    /// Verifies that a Mentor-Trainee assignment is persisted
    /// and cannot be stored twice.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void AssignById_PersistsAssignmentAndPreventsDuplicate() {
        // Arrange
        var mentor = CreateMentor();
        var trainee = CreateTrainee();

        context.users.AddRange(mentor, trainee);
        context.SaveChanges();

        // Act
        repository.AssignById(trainee.guid, mentor.guid);
        repository.AssignById(trainee.guid, mentor.guid);

        // Assert
        var assignment = Assert.Single(context.assignments);

        Assert.Equal(mentor.Id, assignment.MentorId);
        Assert.Equal(trainee.Id, assignment.TraineeId);
    }

    /// <summary>
    /// Verifies that an existing Mentor-Trainee assignment is removed.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void UnassignById_RemovesPersistedAssignment() {
        // Arrange
        var mentor = CreateMentor();
        var trainee = CreateTrainee();

        context.users.AddRange(mentor, trainee);
        context.SaveChanges();

        repository.AssignById(trainee.guid, mentor.guid);

        // Act
        repository.UnassignById(trainee.guid, mentor.guid);

        // Assert
        Assert.Empty(context.assignments);
    }

    /// <summary>
    /// Verifies that persisted assignments return the associated
    /// mentor and trainee.
    /// Author: Dennis Axt
    /// </summary>
    [Fact]
    public void GetAssignments_ReturnsMentorAndTrainee() {
        // Arrange
        var mentor = CreateMentor();
        var trainee = CreateTrainee();

        context.users.AddRange(mentor, trainee);
        context.SaveChanges();

        repository.AssignById(trainee.guid, mentor.guid);

        // Act
        var assignment = Assert.Single(
            repository.GetAssignments());

        // Assert
        Assert.Equal(mentor.guid, assignment.mentor.guid);
        Assert.Equal(trainee.guid, assignment.trainee.guid);
    }

    /// <summary>
    /// Deletes and disposes the isolated test database.
    /// Author: Dennis Axt
    /// </summary>
    public void Dispose() {
        context.Database.EnsureDeleted();
        context.Dispose();
    }

    /// <summary>
    /// Creates a mentor for repository assignment tests.
    /// Author: Dennis Axt
    /// </summary>
    /// <returns>A mentor configured for the test scenario.</returns>
    private static Mentor CreateMentor() {
        return new Mentor(
            "mentor.assignment@test.com",
            "Test",
            "Mentor");
    }

    /// <summary>
    /// Creates a trainee with valid programme dates for repository tests.
    /// Author: Dennis Axt
    /// </summary>
    /// <returns>A trainee configured for the test scenario.</returns>
    private static Trainee CreateTrainee() {
        return new Trainee(
            "trainee.assignment@test.com",
            "Test",
            "Trainee",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(8)));
    }
}