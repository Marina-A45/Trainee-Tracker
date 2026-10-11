using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.Core.Repository;

public interface ITraineeProgressRepository {
    
    public Task<TraineeProgress?> GetByTraineeIdAsync(Guid traineeId, CancellationToken cancellationToken = default);

    public Task<TraineeProgress> UpsertAsync(TraineeProgress traineeProgress, CancellationToken cancellationToken = default);
}