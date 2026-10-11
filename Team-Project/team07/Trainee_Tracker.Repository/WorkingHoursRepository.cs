using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.Repository;

// Author: Marina Achner

/// <summary>
/// Repository responsible for storing and retrieving working hours data.
/// Uses the database context to manage cached working hours entries.
/// </summary>
public class WorkingHoursRepository : IWorkingHoursRepository
{
    private readonly TraineeTrackerContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkingHoursRepository"/> class.
    /// </summary>
    /// <param name="context">
    /// The database context used to access working hours cache data.
    /// </param>
    public WorkingHoursRepository(TraineeTrackerContext context) {
        this.context = context;
    }


    // Author: Marina Achner
    /// <summary>
    /// Saves working hours entries for a trainee.
    /// Updates existing cached data or creates a new cache entry if none exists.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee whose working hours are stored.
    /// </param>
    /// <param name="entries">
    /// A list of working hour entries that should be saved.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous save operation.
    /// </returns>
    public async Task UpdateWorkingHours(
    Guid traineeId,
    List<WorkingHoursEntryDto> entries) {
        var cache = await context.workingHoursCache
            .FirstOrDefaultAsync(x => x.TraineeId == traineeId);

        if (cache == null) {
            cache = new WorkingHoursCache {
                TraineeId = traineeId,
                JsonData = JsonSerializer.Serialize(entries),
                LastUpdated = DateTime.UtcNow
            };

            await context.workingHoursCache.AddAsync(cache);
        }
        else {
            cache.JsonData = JsonSerializer.Serialize(entries);
            cache.LastUpdated = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }



    // Author: Marina Achner
    /// <summary>
    /// Retrieves the latest cached working hours entries for a trainee.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee whose working hours are retrieved.
    /// </param>
    /// <returns>
    /// A task containing a list of working hour entries if available; otherwise, null.
    /// </returns>
    public async Task<List<WorkingHoursEntryDto>?> GetLatestWorkingHoursByTraineeId(
    Guid traineeId) {
        var cache = await context.workingHoursCache
            .FirstOrDefaultAsync(x => x.TraineeId == traineeId);

        if (cache == null)
            return null;

        return JsonSerializer.Deserialize<List<WorkingHoursEntryDto>>(
            cache.JsonData
        );
    }
}