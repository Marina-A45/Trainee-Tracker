using Trainee_Tracker.Core.Entity;

public class BurnDownChartDto {
    public Guid id {get; set; }

    public string[] Labels { get; set; } = [];

    public decimal[] IdealProgress { get; set; } = [];

    public decimal?[] ActualProgress { get; set; } = [];
}