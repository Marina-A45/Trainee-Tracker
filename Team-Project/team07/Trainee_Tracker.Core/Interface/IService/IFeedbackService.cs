using Trainee_Tracker.Core.Entity;
namespace Trainee_Tracker.Core.Service {
    /// <summary>
    /// Author: Carl Böhm-Sturm
    /// </summary>
    public interface IFeedbackService {
        /// <summary>
        /// Creates a new feedback and persists it.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="feedback">The feedback entity to save.</param>
        /// <returns>The unique identifier of the created feedback.</returns>
        Guid CreateFeedback(Feedback feedback);

        /// <summary>
        /// Saves the changes of an already existing feedback.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="feedback">The feedback entity with the updated values.</param>
        void EditFeedback(Feedback feedback);

        /// <summary>
        /// Deletes the feedback with the given identifier if it exists.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="feedbackId">The unique identifier of the feedback to delete.</param>
        void DeleteFeedback(Guid feedbackId);

        /// <summary>
        /// Returns all feedbacks that belong to the given lesson.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="lessonId">The unique identifier of the lesson.</param>
        /// <returns>The list of feedbacks for the lesson.</returns>
        List<Feedback> GetFeedbackByLesson(Guid lessonId);

        /// <summary>
        /// Returns all feedbacks that were given by the given trainee.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="traineeId">The unique identifier of the trainee.</param>
        /// <returns>The list of feedbacks of the trainee.</returns>
        List<Feedback> GetFeedbackByTrainee(Guid traineeId);

        /// <summary>
        /// Returns all feedbacks including their associated lesson and user.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <returns>The list of all feedbacks.</returns>
        List<Feedback> GetLatestFeedback();
    }
}
