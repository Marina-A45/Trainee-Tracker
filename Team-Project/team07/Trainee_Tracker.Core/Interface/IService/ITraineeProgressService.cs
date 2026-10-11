using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;

namespace Trainee_Tracker.Core.Service;

public interface ITraineeProgressService {

    public Task<List<WorkingHoursEntryDto>> GetWorkingHours(Guid traineeId, string email, DateOnly startDate, DateOnly endDate);

    public Task<TraineeProgress?> GetTraineeProgressByTraineeId(Guid traineeId, CancellationToken cancellationToken = default);

    public Task<TraineeProgress> calcAndSaveProgress(Guid traineeId, CancellationToken cancellationToken = default);

    public double calculateWorkSpeed(double weightedProgress, double attendence);

    public double calculateEstimatedBuffer(double buffer, double openWork, double workSpeed);

    public double calculateOpenWork(IEnumerable<LessonProgress> lessons, double weightedProgress);

    public double calculateBuffer(double weightedProgress, double attendence);

    public double calculateWeightedProgressEstimate(IEnumerable<LessonProgress> lessons);


}