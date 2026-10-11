using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Trainee_Tracker.Core.Repository;
namespace Trainee_Tracker.Repository;

/// <summary>
/// Author: Lukas Nowak
/// </summary>
public class CurriculumRepository : ICurriculumRepository
{
    private List<Curriculum> curriculums = new List<Curriculum>();
    private TraineeTrackerContext context;
    public CurriculumRepository(TraineeTrackerContext context)
    {
        this.context = context;
    }
    /// <summary>
    /// Retrieves all curriculums from the database, including their related lessons.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="Curriculum"/> entities.</returns>
    public IEnumerable<Curriculum> GetAll()
    {
        return context.curriculums
        .Include(c => c.lessons);
    }

    /// <summary>
    /// Retrieves a specific curriculum by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the curriculum.</param>
    /// <returns>The found <see cref="Curriculum"/> entity.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no curriculum matches the given ID.</exception>
    public Curriculum GetById(Guid id)
    {
        Curriculum? curr = context.curriculums.Find(id);
        if (curr == null)
        {
            throw new KeyNotFoundException($"Curriculum with ID '{id}' was not found.");
        }
        return curr;
    }
    /// <summary>
    /// Checks if a curriculum exists in the database by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) to check.</param>
    /// <returns>True if the curriculum exists; otherwise, false.</returns>
    public bool Exists(Guid id)
    {
        return context.curriculums.Any(l => l.id == id);
    }
    /// <summary>
    /// Checks if a specific curriculum instance exists within the DB context dataset.
    /// </summary>
    /// <param name="Curriculum">The curriculum entity to search for.</param>
    /// <returns>True if the exact entity is found; otherwise, false.</returns>
    public bool Exists(Curriculum Curriculum)
    {
        return context.curriculums.Contains(Curriculum);
    }
    /// <summary>
    /// Creates a new curriculum, safely checking and associating existing database lessons to prevent duplication.
    /// </summary>
    /// <param name="curriculum">The curriculum entity to save.</param>
    public void Create(Curriculum curriculum)
    {
        // Extract incoming lessons to process them individually and clear the collection temporarily
        var incomingLessons = curriculum.lessons.ToList();
        curriculum.lessons.Clear();

        // Fetch any lessons that already exist in the database to map them correctly
        var incomingIds = incomingLessons.Select(l => l.id).ToList();
        var existingLessonsMap = context.lessons
            .Where(l => incomingIds.Contains(l.id))
            .ToDictionary(l => l.id);

        foreach (var lesson in incomingLessons)
        {
            if (existingLessonsMap.TryGetValue(lesson.id, out var existingLesson))
            {
                // If the lesson already exists, update its values and attach the tracked entity
                context.Entry(existingLesson).CurrentValues.SetValues(lesson);
                curriculum.lessons.Add(existingLesson);
            }
            else
            {
                // If it's a completely new lesson, attach it directly
                curriculum.lessons.Add(lesson);
            }
        }
        context.curriculums.Add(curriculum);
        context.SaveChanges();
    }


    /// <summary>
    /// Updates the lesson list of an existing curriculum, matching incoming lessons with existing tracking states.
    /// </summary>
    /// <param name="curriculumId">The unique identifier of the target curriculum.</param>
    /// <param name="incomingLessons">The updated list of lessons to assign to the curriculum.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the curriculum does not exist.</exception>
    public void Edit(Guid curriculumId, List<Lesson> incomingLessons)
    {
        var dbCurriculum = context.curriculums
            .Include(c => c.lessons)
            .FirstOrDefault(c => c.id == curriculumId);

        if (dbCurriculum == null)
        {
            throw new KeyNotFoundException($"Curriculum with ID {curriculumId} not found.");
        }

        // 1. Identify which lessons are being removed
        var originalLessonIds = dbCurriculum.lessons.Select(l => l.id).ToList();
        var incomingIds = incomingLessons.Select(l => l.id).ToList();
        var removedLessonIds = originalLessonIds.Except(incomingIds).ToList();

        // 2. Update progresses per user
        // Filter specifically for users of the subclass Trainee
        var enrolledUserIds = context.users
            .OfType<Trainee>()
            .Where(u => u.curriculumId == curriculumId)
            .Select(u => u.guid)
            .ToList();

        foreach (var userId in enrolledUserIds)
        {
            EditUserProgress(userId, incomingLessons);
        }


        // 3. Update the curriculum's lesson list
        dbCurriculum.lessons.Clear();

        var existingLessonsMap = context.lessons
            .Where(l => incomingIds.Contains(l.id))
            .ToDictionary(l => l.id);

        foreach (var incomingLesson in incomingLessons)
        {
            if (existingLessonsMap.TryGetValue(incomingLesson.id, out var existingLesson))
            {
                context.Entry(existingLesson).CurrentValues.SetValues(incomingLesson);
                dbCurriculum.lessons.Add(existingLesson);
            }
            else
            {
                dbCurriculum.lessons.Add(incomingLesson);
            }
        }

        if (removedLessonIds.Any())
        {
            var removedLessons = context.lessons
                .Where(l => removedLessonIds.Contains(l.id))
                .ToList();

            foreach (var removedLesson in removedLessons)
            {
                removedLesson.isActive = false;
                dbCurriculum.lessons.Add(removedLesson);
            }

        }

        context.SaveChanges();
    }

    /// <summary>
    /// Synchronizes a user's progresses with the updated curriculum lessons.
    /// Deletes open progresses for lessons no longer in the curriculum, updates existing ones, and creates new ones for added lessons.
    /// </summary>
    /// <param name="userId">The unique identifier of the user (trainee).</param>
    /// <param name="incomingLessons">The updated list of lessons assigned to the curriculum.</param>
    private void EditUserProgress(Guid userId, List<Lesson> incomingLessons)
    {
        // 1. Split incoming lessons into active and inactive
        var activeIncomingLessonIds = incomingLessons
            .Where(l => l.isActive)
            .Select(l => l.id)
            .ToList();

        var inactiveIncomingLessonIds = incomingLessons
            .Where(l => !l.isActive)
            .Select(l => l.id)
            .ToList();

        // 2. DELETE: Find open progresses for this user that are EITHER:
        //    - Not in the incoming list at all
        //    - In the incoming list but marked as inactive
        var openProgressesToDelete = context.progresses
            .Where(p => p.traineeId == userId &&
                        p.status == LessonStatus.Open &&
                        (!activeIncomingLessonIds.Contains(p.lessonId) || inactiveIncomingLessonIds.Contains(p.lessonId)))
            .ToList();

        if (openProgressesToDelete.Any())
        {
            context.progresses.RemoveRange(openProgressesToDelete);
        }

        // 3. Fetch existing progresses ONLY for the active incoming lessons
        var existingProgressesMap = context.progresses
            .Where(p => p.traineeId == userId && activeIncomingLessonIds.Contains(p.lessonId))
            .ToDictionary(p => p.lessonId);

        // 4. UPDATE or ADD progresses ONLY for active incoming lessons
        foreach (var lesson in incomingLessons.Where(l => l.isActive))
        {
            if (existingProgressesMap.TryGetValue(lesson.id, out var existingProgress))
            {
                // UPDATE 
                existingProgress.title = lesson.title;
                existingProgress.timeEstimate = lesson.timeEstimate;
                existingProgress.url = lesson.url;
                existingProgress.isActive = lesson.isActive;
                existingProgress.position = lesson.defaultPosition;
            }
            else
            {
                // ADD: The curriculum added a new active lesson the user doesn't have a progress for yet.
                var newProgress = new LessonProgress
                {
                    lessonId = lesson.id,
                    status = LessonStatus.Open,
                    title = lesson.title,
                    timeEstimate = lesson.timeEstimate,
                    url = lesson.url,
                    isActive = lesson.isActive,
                    position = lesson.defaultPosition,
                    traineeId = userId,
                };
                context.progresses.Add(newProgress);
            }
        }
    }

    /// <summary>
    /// Updates a curriculum entity by completely deleting it and re-creating it.
    /// </summary>
    /// <param name="Curriculum">The curriculum entity to update.</param>
    public void Edit(Curriculum Curriculum)
    {
        Delete(Curriculum);
        Create(Curriculum);
    }
    /// <summary>
    /// Deletes a specific curriculum entity, handling both tracked and untracked contexts safely.
    /// </summary>
    /// <param name="Curriculum">The curriculum entity instance to delete.</param>
    public void Delete(Curriculum Curriculum)
    {
        var trackedEntity = context.curriculums.Find(Curriculum.id);

        if (trackedEntity != null)
        {
            // If the entity is already tracked by EF, remove the tracked reference
            context.curriculums.Remove(trackedEntity);
        }
        else
        {
            // If untracked, attach it to the context first, then issue the removal state
            context.curriculums.Attach(Curriculum);
            context.curriculums.Remove(Curriculum);
        }
        context.SaveChanges();
    }


    /// <summary>
    /// Deletes a curriculum entity directly by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the curriculum to remove.</param>    
    public void DeleteById(Guid id)
    {
        var trackedEntity = context.curriculums.Find(id);

        if (trackedEntity != null)
        {
            context.curriculums.Remove(trackedEntity);
        }
        context.SaveChanges();
    }

}