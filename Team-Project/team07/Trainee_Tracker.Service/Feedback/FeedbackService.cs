using System;
using System.Collections.Generic;
using System.Linq;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Repository;
using System.Data;
namespace Trainee_Tracker.Service {
    /// <summary>
    /// Author: Carl Böhm-Sturm
    /// </summary>
    public class FeedbackService : IFeedbackService {
        private readonly IFeedbackRepository _rep;

        public FeedbackService(IFeedbackRepository rep) {
            _rep = rep;
        }
        /// <summary>
        /// Creates a new feedback and persists it.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="feedback">The feedback entity to save.</param>
        /// <returns>The unique identifier of the created feedback.</returns>
        /// <exception cref="Exception">Thrown if the feedback could not be saved to the database.</exception>
        public Guid CreateFeedback(Feedback feedback) {
            if (!_rep.Save(feedback)) {
                throw new Exception("Could not save feedback to the database.");
            }
            return feedback.id;
        }

        /// <summary>
        /// Deletes the feedback with the given identifier if it exists.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="feedbackId">The unique identifier of the feedback to delete.</param>
        public void DeleteFeedback(Guid feedbackId) {
            var feedback = _rep.GetById(feedbackId);
            if (feedback != null) {
                _rep.Delete(feedback);
            }
        }

        /// <summary>
        /// Saves the changes of an already existing feedback.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="feedback">The feedback entity with the updated values.</param>
        public void EditFeedback(Feedback feedback) {
            _rep.Update(feedback);
        }

        /// <summary>
        /// Returns all feedbacks that belong to the given lesson.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="lessonId">The unique identifier of the lesson.</param>
        /// <returns>The list of feedbacks for the lesson.</returns>
        public List<Feedback> GetFeedbackByLesson(Guid lessonId) {
            return _rep.GetByLesson(lessonId).ToList();
        }

        /// <summary>
        /// Returns all feedbacks that were given by the given trainee.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <param name="traineeId">The unique identifier of the trainee.</param>
        /// <returns>The list of feedbacks of the trainee.</returns>
        public List<Feedback> GetFeedbackByTrainee(Guid traineeId) {
            return _rep.GetByUser(traineeId).ToList();
        }

        /// <summary>
        /// Returns all feedbacks including their associated lesson and user.
        /// Author: Carl Böhm-Sturm
        /// </summary>
        /// <returns>The list of all feedbacks.</returns>
        public List<Feedback> GetLatestFeedback() {
            return _rep.GetAllFeedback();
        }
    }
}