using Trainee_Tracker.Core.Entity;
namespace Trainee_Tracker.Core.Repository {

    /// <summary>
    /// Author: Carl Böhm-Sturm
    /// </summary>
    public interface IFeedbackRepository {
        /// <summary>
        /// Returns all feedbacks that were given by the given user.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>The feedbacks of the user.</returns>
        public Feedback[] GetByUser(Guid userId);

        /// <summary>
        /// Returns all feedbacks that belong to the given lesson.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="lessonId">The unique identifier of the lesson.</param>
        /// <returns>The feedbacks of the lesson.</returns>
        public Feedback[] GetByLesson(Guid lessonId);

        /// <summary>
        /// Finds a feedback by its unique identifier.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="id">The unique identifier of the feedback.</param>
        /// <returns>The feedback, or null if it does not exist.</returns>
        public Feedback? GetById(Guid id);

        /// <summary>
        /// Saves a new feedback to the database.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="feedback">The feedback entity to save.</param>
        /// <returns>True if the feedback was saved, false otherwise.</returns>
        public bool Save(Feedback feedback);

        /// <summary>
        /// Updates an existing feedback in the database.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="feedback">The feedback entity with the updated values.</param>
        /// <returns>True if the update succeeded, false otherwise.</returns>
        public bool Update(Feedback feedback);

        /// <summary>
        /// Removes a feedback from the database.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="feedback">The feedback entity to remove.</param>
        /// <returns>True if the deletion succeeded, false otherwise.</returns>
        public bool Delete(Feedback feedback);

        /// <summary>
        /// Returns all feedbacks including their associated user and lesson.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <returns>The list of all feedbacks.</returns>
        public List<Feedback> GetAllFeedback();
    }
}
