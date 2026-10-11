using Microsoft.EntityFrameworkCore;
using Trainee_Tracker.Core.Repository;

namespace Trainee_Tracker.Repository;

public class TraineeProgressRepository : ITraineeProgressRepository {
    // Database context used to access trainee progress data
    private readonly TraineeTrackerContext _dbContext;
    // Initializes the repository with the database context
    public TraineeProgressRepository(TraineeTrackerContext dbContext) {
        _dbContext = dbContext;
    }
    /// By Tim Glaser
    /// <summary>
    /// Returns TraineeProgress by TraineeId
    /// </summary>
    public async Task<TraineeProgress?> GetByTraineeIdAsync(Guid traineeId, CancellationToken cancellationToken = default) {
        return await _dbContext.TraineeProgress.Include(progress => progress.Trainee)
        .FirstOrDefaultAsync(
            progress => progress.TraineeId == traineeId,
            cancellationToken);
    }
    /// By Tim Glaser
    /// <summary>
    /// Inserts or updates the trainee progress in the database
    /// </summary>
    public async Task<TraineeProgress> UpsertAsync(TraineeProgress traineeProgress, CancellationToken cancellationToken = default) {
        var existingProgress = await _dbContext.TraineeProgress
            .FirstOrDefaultAsync(
                existing => existing.TraineeId == traineeProgress.TraineeId,
                cancellationToken);

        if (existingProgress == null) {

            if (traineeProgress.Id == Guid.Empty) {

                traineeProgress.Id = Guid.NewGuid();
            }

            _dbContext.TraineeProgress.Add(traineeProgress);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return traineeProgress;
        }

        existingProgress.WorkSpeed = traineeProgress.WorkSpeed;
        existingProgress.Buffer = traineeProgress.Buffer;
        existingProgress.EstimatedBuffer = traineeProgress.EstimatedBuffer;
        existingProgress.OpenWork = traineeProgress.OpenWork;
        existingProgress.WeightedProgressEstimate = traineeProgress.WeightedProgressEstimate;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return existingProgress;
    }
}    
