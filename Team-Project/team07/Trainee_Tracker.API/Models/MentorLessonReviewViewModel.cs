using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.API.Models;

// Contains the trainees and finished lessons available for review
// by the current mentor or administrator.
public class MentorLessonReviewViewModel {
    // Trainees visible to the current user.
    public List<Trainee> AvailableTrainees { get; set; } = new();

    // Finished lessons waiting for acceptance or rejection.
    public List<MentorLessonReviewItem> PendingReviews { get; set; } = new();

    // Indicates whether the review page is being used by an administrator.
    public bool IsAdmin { get; set; }
}

// Associates one pending lesson review with its trainee.
public class MentorLessonReviewItem {
    public required Trainee Trainee { get; set; }

    public required LessonProgress Progress { get; set; }
}