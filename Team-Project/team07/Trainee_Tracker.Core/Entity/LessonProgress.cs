using Trainee_Tracker.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Trainee_Tracker.Core.Entity;

public class LessonProgress
{
    //Unique identifier of the lesson progress entry.
    public Guid id { get; set; } = Guid.NewGuid();

    //Identifier of the trainee who is working on the lesson.
    public Guid traineeId { get; set; }

    // Can be included optionally by database
    public Trainee? trainee { get; set; }

    // Identifier of the lesson this progress entry belongs to.
    public Guid lessonId { get; set; }

    //Can be included optionally by database
    public Lesson? lesson { get; set; }

    //Current status of the lesson progress in the learning workflow.
    public required LessonStatus status { get; set; }

    // Title of the lesson.
    public required string title { get; set; }

    //Estimated effort required to complete the lesson (in person days).
    public required double timeEstimate { get; set; }
    // URL to the external lesson content. 
    [Url]
    public required string url { get; set; }

    //Indicates wether the lesson is currently active in the curriculum.
    public required Boolean isActive { get; set; }

    // Position of the lesson within the curriculum order. 
    public required int position { get; set; }

    //Optional reason provided by a mentor when a lesson is rejected. 
    public string rejectionReason { get; set; } = string.Empty;
}
