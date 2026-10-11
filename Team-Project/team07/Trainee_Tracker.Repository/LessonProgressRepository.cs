using Trainee_Tracker.Core.Entity;
using Microsoft.EntityFrameworkCore;
using Trainee_Tracker.Core.Repository;
using System.Linq;
namespace Trainee_Tracker.Repository;

/// <summary>
/// Author: Lukas Nowak
/// </summary>
public class LessonProgressRepository : ILessonProgressRepository
{
    private List<LessonProgress> progresses = new List<LessonProgress>();

    private TraineeTrackerContext context;
    public LessonProgressRepository(TraineeTrackerContext context)
    {
        this.context = context;
    }
    /// <summary>
    /// Retrieves all progresses from the database, including their related lessons.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="LessonProgress"/> entities.</returns>
    public IEnumerable<LessonProgress> GetAll()
    {
        return context.progresses;
    }

    /// <summary>
    /// Retrieves all progresses from the database, including their related lessons.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="LessonProgress"/> entities.</returns>
    public IEnumerable<LessonProgress> GetAll(Guid id)
    {   
        return context.progresses.Where(p => p.traineeId == id);
    }
    /// <summary>
    /// Retrieves a specific Progress by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the Progress.</param>
    /// <returns>The found <see cref="Progress"/> entity.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no Progress matches the given ID.</exception>
    public LessonProgress GetById(Guid id)
    {
        LessonProgress? curr = context.progresses.Find(id);
        if (curr == null)
        {
            throw new KeyNotFoundException($"Progress with ID '{id}' was not found.");
        }
        return curr;
    }
    public void Create(LessonProgress progress)
    {
    }

    public void Edit(LessonProgress progress)
    {
        if (progress == null)
            throw new ArgumentNullException(nameof(progress));

        var existing = context.progresses.Find(progress.id);
        if (existing == null)
            throw new KeyNotFoundException($"Progress with ID '{progress.id}' was not found.");

        // copy values from incoming entity to tracked entity
        context.Entry(existing).CurrentValues.SetValues(progress);
        context.SaveChanges();
    }
    public void Delete(LessonProgress progress)
    {
    }
    public void DeleteById(Guid id)
    {

    }
}
