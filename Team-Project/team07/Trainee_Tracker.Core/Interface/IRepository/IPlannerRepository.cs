using Trainee_Tracker.Core.Entity;

namespace Trainee_Tracker.Core.Repository;

// Author: Marina Achner

/// <summary>
/// Repository interface responsible for accessing planner-related data.
/// Defines methods for retrieving lessons, lesson progress, and blocked days.
/// </summary>
public interface IPlannerRepository {
    /// <summary>
    /// Retrieves all lessons that are currently active or in progress.
    /// </summary>
    /// <returns>
    /// A list containing all active <see cref="Lesson"/> entities.
    /// </returns>
    public List<Lesson> GetActiveLessons(Guid trainee);

    /// <summary>
    /// Retrieves all lesson progress entries for a specific trainee.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee.
    /// </param>
    /// <returns>
    /// A list containing all <see cref="LessonProgress"/> entries belonging to the trainee.
    /// </returns>
    public List<LessonProgress> GetLessonProgress(Guid traineeId);
}