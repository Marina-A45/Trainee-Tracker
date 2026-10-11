using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.Core.Repository;

// Author: Marina Achner

/// <summary>
/// Repository interface responsible for accessing trainee data.
/// Defines methods for retrieving trainee entities.
/// </summary>
public interface ITraineeRepository {
    /// <summary>
    /// Retrieves a trainee by its unique identifier.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee.
    /// </param>
    /// <returns>
    /// The matching <see cref="Trainee"/> entity if found; otherwise, null.
    /// </returns>
    public Trainee? GetTraineeById(Guid traineeId);

    /// <summary>
    /// Retrieves all trainees.
    /// </summary>
    /// <returns>
    /// A list containing all available <see cref="Trainee"/> entities.
    /// </returns>
    List<Trainee> GetAllTrainees();
}
