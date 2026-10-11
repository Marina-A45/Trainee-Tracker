namespace Trainee_Tracker.Core.Entity;

/// <summary>
/// Represents a cached snapshot of working hours data for a trainee.
/// This is used to improve performance by avoiding repeated data fetching/calculations.
/// </summary>
public class WorkingHoursCache
{
    /// <summary>
    /// Unique identifier for the cache entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the trainee this cached data belongs to.
    /// </summary>
    public Guid TraineeId { get; set; }

    /// <summary>
    /// Serialized JSON string containing the cached working hours data.
    /// </summary>
    public string JsonData { get; set; } = "";
    /// <summary>
    /// Timestamp indicating when the cache entry was last updated (UTC).
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}