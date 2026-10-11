using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.Repository;

namespace Trainee_Tracker.Repository;

// Author: Marina Achner

/// <summary>
/// Repository responsible for accessing trainee data.
/// Provides methods for retrieving trainee entities from the database.
/// </summary>
public class TraineeRepository : ITraineeRepository {
    /// <summary>
    /// Database context used to access trainee data.
    /// </summary>
    private readonly TraineeTrackerContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraineeRepository"/> class.
    /// </summary>
    /// <param name="traineeTrackerContext">
    /// The database context used for accessing trainee information.
    /// </param>
    public TraineeRepository(TraineeTrackerContext traineeTrackerContext) {
        _context = traineeTrackerContext;

    }

    /// <summary>
    /// Retrieves a trainee by their unique identifier.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee.
    /// </param>
    /// <returns>
    /// The matching <see cref="Trainee"/> entity if found; otherwise, null.
    /// </returns>
    public Trainee? GetTraineeById(Guid traineeId) {
        return _context.users
            .OfType<Trainee>()
            .FirstOrDefault(t => t.guid == traineeId);
    }

    /// <summary>
    /// Retrieves all trainees from the database.
    /// </summary>
    /// <returns>
    /// A list containing all available <see cref="Trainee"/> entities.
    /// </returns>
    public List<Trainee> GetAllTrainees() {
        return _context.users.OfType<Trainee>().ToList();
    }
}