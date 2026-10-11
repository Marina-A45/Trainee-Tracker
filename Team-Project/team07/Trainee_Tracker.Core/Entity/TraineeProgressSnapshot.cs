using Trainee_Tracker.Core.Entity;

public class TraineeProgressSnapshot {
    public Guid Id { get; set; }

    public Guid TraineeId { get; set; }

    public Trainee Trainee { get; set; } = null!;

    public int WeekIndex { get; set; }

    public decimal ActualProgress { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public DateTime CreatedAt { get; set; }
}