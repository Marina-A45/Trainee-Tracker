using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.Core.Service;

public interface IBurnDownChartService
{
    public Task<BurnDownChartDto> GetChartData(Guid traineeId, CancellationToken cancellationToken = default);

    public Task EnsureCurrentWeekSnapshotExistsAsync(Guid traineeId);

    public int GetCurrentWeekIndex(DateOnly startDate, DateOnly today);

};