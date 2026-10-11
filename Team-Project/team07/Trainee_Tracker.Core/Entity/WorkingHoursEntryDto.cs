namespace Trainee_Tracker.Core.Entity;

/// <summary>
/// Represents a single working hours record for a specific day.
/// </summary>
public class WorkingHoursEntryDto {
    /// <summary>
    /// Date of the working hours entry (stored as string, e.g. "2026-07-04").
    /// </summary>
    public string date { get; set; } = "";

    /// <summary>
    /// Number of working hours recorded for the given date.
    /// Have to be named like in the api
    /// </summary>
    public double working_hours { get; set; }
}