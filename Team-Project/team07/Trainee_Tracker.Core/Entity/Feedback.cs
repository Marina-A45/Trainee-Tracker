using System.ComponentModel.DataAnnotations;

namespace Trainee_Tracker.Core.Entity;

/// <summary>
/// Author: Carl Böhm-Sturm
/// </summary>
public class Feedback {
    /// <summary>
    /// Unique identifier of the Feedback.
    /// </summary>
    public Guid id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The felt difficulty on a scale from 1 to 5, optional but validated when given.
    /// </summary>
    [Range(1, 5)]
    public int? difficulty { get; set; }

    /// <summary>
    /// How long the Lesson actually took.
    /// </summary>
    public double? actualEffort { get; set; }

    /// <summary>
    /// The previous knowledge of the Trainee.
    /// </summary>
    public string? knowledge { get; set; }

    /// <summary>
    /// The given Feedback-Text of the trainee.
    /// </summary>
    public string? text { get; set; }

    /// <summary>
    /// The point in time (UTC) at which the feedback was created.
    /// </summary>
    public DateTime creationDate { get; set; }

    /// <summary>
    /// The lesson this feedback refers to.
    /// </summary>
    public required Lesson associatedLesson { get; set; }

    /// <summary>
    /// The user who gave this feedback.
    /// </summary>
    public required User associatedUser { get; set; }
}
