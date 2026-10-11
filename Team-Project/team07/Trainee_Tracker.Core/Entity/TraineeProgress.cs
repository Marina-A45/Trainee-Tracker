using Trainee_Tracker.Core.Entity;
    /// <summary>
    /// Author: Tim Glaser
    /// Entity class representing the progress of a trainee
    /// </summary>
public class TraineeProgress {
    
    public Guid Id { get; set; }

    public Guid TraineeId { get; set; }

    public User Trainee { get; set; } = null!;

    public decimal WorkSpeed { get; set; }

    public decimal Buffer { get; set; }

    public decimal EstimatedBuffer { get; set; }

    public decimal OpenWork { get; set; }

    public decimal WeightedProgressEstimate { get; set; }
}