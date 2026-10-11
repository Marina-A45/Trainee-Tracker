using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Trainee_Tracker.Core.Entity;

public class Lesson
{
    // Unique idetifier of the lesson.
    public required Guid id { get; set; } = Guid.NewGuid();

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
    public required int defaultPosition { get; set; }

    //References, so EFCore can connect lessons correctly to curriculums
    [JsonIgnore]
    public virtual ICollection<Curriculum> Curriculums { get; set; } = new List<Curriculum>();

    //References, so EFCore can connect lessons correctly to lessonprogress
    [JsonIgnore]
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}
