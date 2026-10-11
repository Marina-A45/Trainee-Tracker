using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Service;

namespace Trainee_Tracker.Core.Service;

public class BurnDownChartService : IBurnDownChartService
{
    private readonly ITraineeProgressSnapshotRepository _traineeProgressSnapshotRepository;
    private readonly ITraineeProgressService _traineeProgressService;
    private readonly ITraineeProgressRepository _traineeProgressRepository;
    private readonly ITraineeRepository _traineeRepository;

    public BurnDownChartService(ITraineeProgressSnapshotRepository traineeProgressSnapshotRepository, ITraineeProgressService traineeProgressService, ITraineeProgressRepository traineeProgressRepository, ITraineeRepository traineeRepository)
    {
        _traineeProgressSnapshotRepository = traineeProgressSnapshotRepository;
        _traineeProgressService = traineeProgressService;
        _traineeProgressRepository = traineeProgressRepository;
        _traineeRepository = traineeRepository;
    }

    /// By Tim Glaser
    /// <summary>
    /// Builds the burndown chart data for a trainee using the trainee's program duration,
    /// ideal remaining work, and saved weekly progress snapshots
    /// </summary>
    public async Task<BurnDownChartDto> GetChartData(Guid traineeId, CancellationToken cancellationToken = default)
    {
        var trainee = _traineeRepository.GetTraineeById(traineeId);
        var traineeProgress = await _traineeProgressRepository.GetByTraineeIdAsync(traineeId);

        if (trainee is null)
            throw new Exception("Trainee not found.");

        await EnsureCurrentWeekSnapshotExistsAsync(traineeId);

        var totalWork = traineeProgress.WeightedProgressEstimate + traineeProgress.OpenWork;

        var snapshots = await _traineeProgressSnapshotRepository.GetByTraineeIdAsync(traineeId);

        var labels = BuildWeekLabels(trainee.startDate, trainee.endDate);
        var idealProgress = BuildIdealProgress(labels.Length, totalWork);

        var actualProgress = new decimal?[labels.Length];
        actualProgress[0] = totalWork;

        foreach (var snapshot in snapshots)
        {
            if (snapshot.WeekIndex >= 0 && snapshot.WeekIndex < actualProgress.Length)
            {
                 actualProgress[snapshot.WeekIndex] = snapshot.ActualProgress;
            }
        }

        return new BurnDownChartDto
        {
            Labels = labels,
            IdealProgress = idealProgress,
            ActualProgress = actualProgress
        };

    }

    /// By Tim Glaser
    /// <summary>
    /// Ensures that a progress snapshot exists for the trainee's current program week
    /// If no snapshot exists yet, a new one is created using the trainee's current remaining work
    /// </summary>
    public async Task EnsureCurrentWeekSnapshotExistsAsync(Guid traineeId)
    {
        var traineeProgress = await _traineeProgressRepository.GetByTraineeIdAsync(traineeId);
        if (traineeProgress is null) {
            throw new Exception("Trainee progress not found.");
        }
        var trainee = _traineeRepository.GetTraineeById(traineeId);
        if (trainee is null) {
            throw new Exception("Trainee not found.");
        }
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentWeekIndex = GetCurrentWeekIndex(trainee.startDate, today);

        if (currentWeekIndex < 0)
            return;

        var alreadyExists = await _traineeProgressSnapshotRepository.ExistsForWeekAsync(
            traineeId,
            currentWeekIndex);

        if (alreadyExists)
            return;

        var openWork = traineeProgress.OpenWork;

        var snapshot = new TraineeProgressSnapshot
        {
            Id = Guid.NewGuid(),
            TraineeId = traineeId,
            WeekIndex = currentWeekIndex,
            WeekStartDate = trainee.startDate.AddDays(currentWeekIndex * 7),
            ActualProgress = openWork,
            CreatedAt = DateTime.UtcNow
        };

        await _traineeProgressSnapshotRepository.AddAsync(snapshot);
        await _traineeProgressSnapshotRepository.SaveChangesAsync();
    }

    /// By Tim Glaser
    /// <summary>
    /// Returns the Current Week since given startDate
    /// </summary>
    public int GetCurrentWeekIndex(DateOnly startDate, DateOnly today)
    {
        var daysSinceStart = today.DayNumber - startDate.DayNumber;

        if (daysSinceStart < 0)
            return -1;

        return daysSinceStart / 7;
    }

    /// By Tim Glaser
    /// <summary>
    /// Builds and returns the Labels for the BurnDownChart based on the given duration
    /// </summary>
    private static string[] BuildWeekLabels(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
            return ["Week 1"];

        var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
        var weekCount = Math.Max(1, (int)Math.Ceiling(totalDays / 7.0));

        return Enumerable.Range(1, weekCount)
            .Select(week => $"Week {week}")
            .ToArray();
    }

    /// By Tim Glaser
    /// <summary>
    /// Builds and returns the IdealProgress of the BurnDownChart based on the give duration
    /// </summary>
    private static decimal[] BuildIdealProgress(int weekCount, decimal totalWork)
    {
        if (weekCount <= 1)
            return [totalWork];

        return Enumerable.Range(0, weekCount)
        .Select(i =>
        {
            var progressRatio = (decimal)i / (weekCount - 1);
            var remainingWork = totalWork * (1 - progressRatio);

            return Math.Round(remainingWork, 2);
        })
        .ToArray();
    }


}    