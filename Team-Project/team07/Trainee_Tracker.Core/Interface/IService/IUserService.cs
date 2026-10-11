using Trainee_Tracker.Core.Entity;
namespace Trainee_Tracker.Core.Service;

using Microsoft.AspNetCore.Identity;


/// <summary>
/// Author: Patrick Plechinger
/// </summary>

public interface IUserService
{
    public Task<IdentityResult> Create(CreateUser newUser);
    public bool edit(EditUser user);
    public Task<bool> removePassword(Guid user);
    public Task<string> setPassword(SetPassword password);
    public EditUser GetEditUser(Guid id);
    public IEnumerable<User> getAll();
    public Task<User> GetByEmail(string email);
    public Task<bool> Deactivate(Guid id);
    public Task<bool> Activate(Guid id);
    public void assignTrainee(Mentor mentor, Trainee trainee);
    /// <summary>
    /// Returns all persisted mentor-trainee assignments.
    /// Author: Dennis Axt.
    /// </summary>
    /// <returns>All persisted mentor-trainee assignments.</returns>
    public IEnumerable<MentorAssignment> getMentorAssignments();

    /// <summary>
    /// Creates an assignment using the public identifiers of a mentor
    /// and a trainee.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="mentorId">The public identifier of the mentor.</param>
    /// <param name="traineeId">The public identifier of the trainee.</param>
    public void assignTrainee(Guid mentorId, Guid traineeId);

    /// <summary>
    /// Removes an assignment using the public identifiers of a mentor
    /// and a trainee.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="mentorId">The public identifier of the mentor.</param>
    /// <param name="traineeId">The public identifier of the trainee.</param>
    public void unassignTrainee(Guid mentorId, Guid traineeId);
}