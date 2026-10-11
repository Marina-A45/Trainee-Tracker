using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.Core.Service;

// Author: Marina Achner

/// <summary>
/// Service interface responsible for trainee-related business logic.
/// Defines methods for retrieving trainee data.
/// </summary>
public interface ITraineeService {
    /// <summary>
    /// Retrieves all trainees.
    /// </summary>
    /// <returns>
    /// A list containing all available <see cref="Trainee"/> entities.
    /// </returns>
    public List<Trainee> GetAllTrainees();

    /// <summary>
    /// Retrieves a trainee by their unique identifier.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee.
    /// </param>
    /// <returns>
    /// The matching <see cref="Trainee"/> entity.
    /// </returns>
    public Trainee GetTraineeById(Guid traineeId);
}