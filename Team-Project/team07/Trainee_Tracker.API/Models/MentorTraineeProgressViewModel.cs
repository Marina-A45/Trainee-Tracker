using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.API.Models;

// Contains the trainees and lesson progress displayed on the shared
// Mentor/Admin trainee-progress page.
public class MentorTraineeProgressViewModel {
    // Trainees available to the current user.
    // Mentors receive assigned trainees; administrators receive all active trainees.
    public List<Trainee> AvailableTrainees { get; set; } = new();

    // Trainee currently selected in the progress view.
    public Trainee? SelectedTrainee { get; set; }

    // Lesson progress belonging only to the selected trainee.
    public List<LessonProgress> Progresses { get; set; } = new();

    // Indicates whether the page is being used by an administrator.
    public bool IsAdmin { get; set; }

    public TraineeProgress? TraineeProgress { get; set; }
}