using Trainee_Tracker.Core.Entity;

// Author: Marina Achner

/// <summary>
/// Repository interface responsible for accessing and updating working hours data.
/// Defines methods for storing and retrieving cached working hours information.
/// </summary>
public interface IWorkingHoursRepository {
    /// <summary>
    /// Saves working hours data in the database cache.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee whose working hours are updated.
    /// </param>
    /// <param name="entries">
    /// A list of working hour entries that should be stored.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous update operation.
    /// </returns>
    public Task UpdateWorkingHours(
        Guid traineeId,
        List<WorkingHoursEntryDto> entries
    );

    /// <summary>
    /// Loads the latest cached working hours for a trainee.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee whose working hours are retrieved.
    /// </param>
    /// <returns>
    /// A task containing a list of working hour entries if available; otherwise, null.
    /// </returns>
    public Task<List<WorkingHoursEntryDto>?> GetLatestWorkingHoursByTraineeId(
        Guid traineeId
    );
}