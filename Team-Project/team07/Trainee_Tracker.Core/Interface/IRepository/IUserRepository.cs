using Trainee_Tracker.Core.Entity;
namespace Trainee_Tracker.Core.Repository;

public interface IUserRepository : IRepository<User>
{
    public void Deactivate(Guid id);
    public void Activate(Guid id);
    public User GetByEmail(string email);
    /// <summary>
    /// Returns all persisted mentor-trainee assignments.
    /// Author: Dennis Axt.
    /// </summary>
    /// <returns>All persisted mentor-trainee assignments.</returns>
    public IEnumerable<MentorAssignment> GetAssignments();

    /// <summary>
    /// Creates a persisted assignment between a mentor and a trainee.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="traineeId">The public identifier of the trainee.</param>
    /// <param name="mentorId">The public identifier of the mentor.</param>
    public void AssignById(Guid traineeId, Guid mentorId);

    /// <summary>
    /// Removes a persisted assignment between a mentor and a trainee.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="traineeId">The public identifier of the trainee.</param>
    /// <param name="mentorId">The public identifier of the mentor.</param>
    public void UnassignById(Guid traineeId, Guid mentorId);
}