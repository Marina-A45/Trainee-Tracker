using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.Core.Repository;

public interface ITraineeProgressSnapshotRepository {

    public Task<List<TraineeProgressSnapshot>> GetByTraineeIdAsync(Guid traineeId);

    public Task<TraineeProgressSnapshot?> GetLatestByTraineeIdAsync(Guid traineeId);

    public Task<bool> ExistsForWeekAsync(Guid traineeId, int weekIndex);

    public Task AddAsync(TraineeProgressSnapshot snapshot);

    public Task AddRangeAsync(IEnumerable<TraineeProgressSnapshot> snapshots);

    public Task SaveChangesAsync();
}