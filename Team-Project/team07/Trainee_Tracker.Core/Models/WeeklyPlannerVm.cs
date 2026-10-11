namespace Trainee_Tracker.Core.Models;

// Author: Marina Achner

/// <summary>
/// Represents the weekly planner view model containing scheduled events
/// and blocked (unavailable) days for a trainee.
/// </summary>
public class WeeklyPlannerVm {
    /// <summary>
    /// List of scheduled planner events for the week.
    /// </summary>
    public List<PlannerItemVm> Events { get; set; } = new();

    /// <summary>
    /// List of days where the trainee is unavailable or blocked.
    /// </summary>
    public List<DateOnly> BlockedDays { get; set; } = new();
}