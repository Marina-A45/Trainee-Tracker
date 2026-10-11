using Trainee_Tracker.Core.Models;

namespace Trainee_Tracker.Core.Service {
    // Author: Marina Achner

    /// <summary>
    /// Service interface responsible for planner-related business logic.
    /// Defines methods for generating planner data.
    /// </summary>
    public interface IPlannerService {
        /// <summary>
        /// Generates and returns the weekly planner for a specific trainee.
        /// </summary>
        /// <param name="traineeId">
        /// The unique identifier (GUID) of the trainee whose planner should be generated.
        /// </param>
        /// <returns>
        /// A task containing the weekly planner view model.
        /// </returns>
        public Task<WeeklyPlannerVm> GetPlanner(Guid traineeId);
    }
}