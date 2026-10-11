namespace Trainee_Tracker.Core.Exceptions;

// Author: Marina Achner

/// <summary>
/// Exception that is thrown when a requested trainee cannot be found.
/// </summary>
public class TraineeNotFoundException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="TraineeNotFoundException"/> class
    /// with the identifier of the missing trainee.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee that could not be found.
    /// </param>
    public TraineeNotFoundException(Guid traineeId)
        : base($"Trainee with id {traineeId} does not exist!") {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TraineeNotFoundException"/> class
    /// without a specific trainee identifier.
    /// </summary>
    public TraineeNotFoundException()
        : base($"No Trainee was Found") {
    }
}