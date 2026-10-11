using Microsoft.EntityFrameworkCore;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.Repository;

namespace Trainee_Tracker.Repository;

// Author: Marina Achner

/// <summary>
/// Repository responsible for planner-related database operations.
/// Provides methods for retrieving lessons, lesson progress, and blocked days.
/// </summary>
public class PlannerRepository : IPlannerRepository {
    /// <summary>
    /// Database context used to access planner-related data.
    /// </summary>
    private readonly TraineeTrackerContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlannerRepository"/> class.
    /// </summary>
    /// <param name="context">
    /// The database context used for accessing planner data.
    /// </param>
    public PlannerRepository(TraineeTrackerContext context) {
        _context = context;
    }

    // Author: Marina Achner
    /// <summary>
    /// Retrieves active lessons ordered by their defaultPosition.
    /// Includes lessons that are currently started by a trainee.
    /// </summary>
    /// <returns>
    /// A list containing active <see cref="Lesson"/> entities.
    /// </returns>
public List<Lesson> GetActiveLessons(Guid traineeId)
{
    return _context.lessons
        .Include(l => l.LessonProgresses)
        .Where(l =>
            l.isActive ||
            l.LessonProgresses.Any(lp => lp.status == LessonStatus.Started))
        .OrderBy(l =>
            l.LessonProgresses
                .Where(lp => lp.traineeId == traineeId)
                .Select(lp => lp.position)
                .FirstOrDefault())
        .ToList();
}

    // Author: Marina Achner
    /// <summary>
    /// Retrieves lesson progress entries for a specific trainee.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee.
    /// </param>
    /// <returns>
    /// A list containing the trainee's <see cref="LessonProgress"/> entries.
    /// </returns>
    public List<LessonProgress> GetLessonProgress(Guid traineeId) {
        return _context.progresses
            .Where(p => p.traineeId == traineeId)
            .ToList();
    }
}