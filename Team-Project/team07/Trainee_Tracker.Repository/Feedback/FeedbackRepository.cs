using Microsoft.EntityFrameworkCore;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Repository;

/// <summary>
/// Author: Carl Böhm-Sturm
/// </summary>
public class FeedbackRepository : IFeedbackRepository {
    private readonly TraineeTrackerContext _context;


    public FeedbackRepository(TraineeTrackerContext context) {
        _context = context;
    }

    /// <summary>
    /// Returns all feedbacks that were given by the given user.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The feedbacks of the user.</returns>
    public Feedback[] GetByUser(Guid userId) {
        return [.. _context.feedbacks.Where(f => f.associatedUser.guid == userId)];
    }

    /// <summary>
    /// Returns all feedbacks that belong to the given lesson.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="lessonId">The unique identifier of the lesson.</param>
    /// <returns>The feedbacks of the lesson.</returns>
    public Feedback[] GetByLesson(Guid lessonId) {
        return [.. _context.feedbacks.Where(f => f.associatedLesson.id == lessonId)];
    }

    /// <summary>
    /// Finds a feedback by its unique identifier.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="id">The unique identifier of the feedback.</param>
    /// <returns>The feedback, or null if it does not exist.</returns>
    public Feedback? GetById(Guid id) {
        return _context.feedbacks.Find(id);
    }

    /// <summary>
    /// Saves a new feedback to the database after resolving its associated lesson and user.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="feedback">The feedback entity to save.</param>
    /// <returns>True if the feedback was saved, false if an associated entity is missing or the database update fails.</returns>
    public bool Save(Feedback feedback) {
        var lesson = feedback.associatedLesson == null ? null : _context.lessons.Find(feedback.associatedLesson.id);
        var user = feedback.associatedUser == null ? null : _context.users.FirstOrDefault(u => u.guid == feedback.associatedUser.guid);
        if (lesson == null || user == null) {
            Console.Error.WriteLine($"[FeedbackRepository] save failed: associated {(lesson == null ? "lesson" : "user")} does not exist in the database.");
            return false;
        }
        feedback.associatedLesson = lesson;
        feedback.associatedUser = user;

        _context.feedbacks.Add(feedback);
        try {
            _context.SaveChanges();
        }
        catch (System.Exception ex) {
            Console.Error.WriteLine($"[FeedbackRepository] save failed: {ex.InnerException?.Message ?? ex.Message}");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Updates an existing feedback in the database.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="feedback">The feedback entity with the updated values.</param>
    /// <returns>True if the update succeeded, false if the database update fails.</returns>
    public bool Update(Feedback feedback) {
        _context.feedbacks.Update(feedback);
        try {
            _context.SaveChanges();
        }
        catch (System.Exception ex) {
            Console.Error.WriteLine($"[FeedbackRepository] update failed: {ex.InnerException?.Message ?? ex.Message}");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Removes a feedback from the database.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="feedback">The feedback entity to remove.</param>
    /// <returns>True if the deletion succeeded, false if the database update fails.</returns>
    public bool Delete(Feedback feedback) {
        _context.feedbacks.Remove(feedback);
        try {
            _context.SaveChanges();
        }
        catch (System.Exception ex) {
            Console.Error.WriteLine($"[FeedbackRepository] delete failed: {ex.InnerException?.Message ?? ex.Message}");
            return false;
        }
        return true;
    }
    /// <summary>
    /// Returns all feedbacks including their associated user and lesson.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <returns>The list of all feedbacks.</returns>
    public List<Feedback> GetAllFeedback() {
        return [.. _context.feedbacks.Include(f => f.associatedUser).Include(f => f.associatedLesson)];
    }
}