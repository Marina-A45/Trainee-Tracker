using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;

namespace Trainee_Tracker.Core.Service;

public interface ILessonProgressService {
    public IEnumerable<LessonProgress> getAll();

    /// <summary>
    /// Changes a lesson status after validating access permissions,
    /// allowed transitions, rejection reasons, and the feedback workflow.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="progressId">The identifier of the lesson progress.</param>
    /// <param name="status">The requested new lesson status.</param>
    /// <param name="rejectionReason">
    /// The mandatory reason when the requested status is Rejected.
    /// </param>
    public void setStatus(
        Guid progressId,
        LessonStatus status,
        string? rejectionReason = null);

    public void reject(Guid progressId, string reason);

    /// <summary>
    /// Marks an accepted lesson as rated after its trainee has submitted
    /// feedback for the associated lesson progress.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="progressId">
    /// The identifier of the lesson progress associated with the feedback.
    /// </param>
    public void rateAfterFeedback(Guid progressId);

    public IEnumerable<LessonProgress> getAll(Guid traineeId);

    /// <summary>
    /// Stores a trainee-specific order of open lessons without changing
    /// the standard order of the imported curriculum.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="traineeId">
    /// The identifier of the trainee whose lessons are reordered.
    /// </param>
    /// <param name="orderedProgressIds">
    /// All open lesson progress identifiers in the requested order.
    /// </param>
    public void setIndividualOrder(
        Guid traineeId,
        IEnumerable<Guid> orderedProgressIds);
}