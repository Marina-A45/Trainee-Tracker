namespace Trainee_Tracker.Core.Entity;

/// <summary>
/// DTO that represents a response containing multiple working hours entries.
/// </summary>
public class WorkingHoursResponseDto
{
    /// <summary>
    /// List of working hours entries.
    /// Each entry represents working time for a specific date.
    /// </summary>
    public List<WorkingHoursEntryDto> entries { get; set; } = new();
}

