namespace Trainee_Tracker.API.Models;

// Data for assigning and unassigning mentors for a trainee.
// Supports both the existing popup and multiple persisted mentor assignments.
public class MentorAssignmentViewModel {
    public required Guid TraineeId { get; set; }

    public required string TraineeName { get; set; }

    // Currently selected mentor in the existing assignment popup.
    public Guid? SelectedMentorId { get; set; }

    // Name of the mentor selected in the existing assignment popup.
    public string? CurrentMentorName { get; set; }

    // All mentors available for assignment.
    public List<MentorOption> AvailableMentors { get; set; } = new();

    // Mentors currently assigned to this trainee.
    public List<MentorOption> AssignedMentors { get; set; } = new();

    // True if the trainee has at least one assigned mentor.
    public bool IsAssigned =>
        SelectedMentorId.HasValue || AssignedMentors.Count > 0;
}

// Represents a mentor displayed in an assignment selection or assignment list.
public class MentorOption {
    public required Guid Id { get; set; }

    public required string Name { get; set; }
}