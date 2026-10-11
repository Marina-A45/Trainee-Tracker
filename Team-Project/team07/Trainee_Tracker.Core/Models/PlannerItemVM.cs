namespace Trainee_Tracker.Core.Models;

// Author: Marina Achner

/// <summary>
/// Represents a single item in the weekly planner view.
/// This model contains the information required to display a task, lesson,
/// or scheduled activity in the planner UI.
/// </summary>
public class PlannerItemVm {
    /// <summary>
    /// Unique identifier of the planner item.
    /// </summary>
    public required string id { get; set; }

    /// <summary>
    /// Display title of the planner item.
    /// </summary>
    public required string title { get; set; }

    /// <summary>
    /// Start date and time of the planner item.
    /// </summary>
    public DateOnly start { get; set; }

    /// <summary>
    /// End date and time of the planner item.
    /// </summary>
    public DateOnly end { get; set; }

    /// <summary>
    /// Position of the item within the planner.
    /// Used for ordering or layout purposes.
    /// </summary>
    public int position { get; set; }

    /// <summary>
    /// Estimated time required to complete the item.
    /// </summary>
    public double timeEstimate { get; set; }

    /// <summary>
    /// Current status of the planner item.
    /// </summary>
    public required string status { get; set; }

    /// <summary>
    /// Indicates whether the item is currently available to be started.
    /// </summary>
    public bool isStartable { get; set; }

    /// <summary>
    /// URL pointing to additional details or a related resource for the item.
    /// </summary>
    public required string url { get; set; }
}