using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Repository;
using Microsoft.EntityFrameworkCore;

namespace Trainee_Tracker.Core.Entity;

public class TraineeProgressSnapshotRepository : ITraineeProgressSnapshotRepository {
    private readonly TraineeTrackerContext _context;

    public TraineeProgressSnapshotRepository(TraineeTrackerContext context) {
        _context = context;
    }

    /// By Tim Glaser
    /// <summary>
    /// Returns a ordered List of TraineeProgressSnapshots by TraineeId
    /// </summary>
    public async Task<List<TraineeProgressSnapshot>> GetByTraineeIdAsync(Guid traineeId) {
        return await _context.TraineeProgressSnapshots
            .Where(s => s.TraineeId == traineeId)
            .OrderBy(s => s.WeekIndex)
            .ToListAsync();
    }

    /// By Tim Glaser
    /// <summary>
    /// Returns the latest TraineeProgressSnapshot by TraineeId
    /// </summary>
    public async Task<TraineeProgressSnapshot?> GetLatestByTraineeIdAsync(Guid traineeId) {
        return await _context.TraineeProgressSnapshots
            .Where(s => s.TraineeId == traineeId)
            .OrderByDescending(s => s.WeekIndex)
            .FirstOrDefaultAsync();
    }

    /// By Tim Glaser
    /// <summary>
    /// Checks whether a progress snapshot already exists for the given trainee and week
    /// </summary>
    public async Task<bool> ExistsForWeekAsync(Guid traineeId, int weekIndex) {
        return await _context.TraineeProgressSnapshots
            .AnyAsync(s => s.TraineeId == traineeId && s.WeekIndex == weekIndex);
    }

    /// By Tim Glaser
    /// <summary>
    /// Adds a single trainee progress snapshot to the database context
    /// </summary>
    public async Task AddAsync(TraineeProgressSnapshot snapshot) {
        await _context.TraineeProgressSnapshots.AddAsync(snapshot);
    }

    /// By Tim Glaser
    /// <summary>
    /// Adds multiple trainee progress snapshots to the database context
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<TraineeProgressSnapshot> snapshots) {
        await _context.TraineeProgressSnapshots.AddRangeAsync(snapshots);
    }

    /// By Tim Glaser
    /// <summary>
    /// Persists all pending progress snapshot changes to the database
    /// </summary>
    public async Task SaveChangesAsync() {
        await _context.SaveChangesAsync();
    }
}