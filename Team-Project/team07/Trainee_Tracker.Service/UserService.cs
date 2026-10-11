using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.Exceptions;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Service;
using Microsoft.AspNetCore.Identity;

namespace Trainee_Tracker.Service;


public class UserService : IUserService {
    private IUserRepository _userRepository { get; set; }
    private ICurriculumService _curriculumService { get; set; }
    private readonly UserManager<User> _userManager;

    /// <summary>
    /// Author: Patrick Plechinger
    /// Initializes the user service with the required dependencies.
    /// </summary>
    public UserService(IUserRepository userRepository, ICurriculumService _curriculumService, UserManager<User> _userManager) {
        this._userRepository = userRepository;
        this._curriculumService = _curriculumService;
        this._userManager = _userManager;
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Retrieves all users from the repository.
    /// </summary>
    public IEnumerable<User> getAll() {

        return _userRepository.GetAll();
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Deactivates the specified user.
    /// </summary>
    public async Task<bool> Deactivate(Guid id) {
        _userRepository.Deactivate(id);
        return true;
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Activates the specified user.
    /// </summary>
    public async Task<bool> Activate(Guid id) {
        _userRepository.Activate(id);
        return true;
    }
    /// <summary>
    /// Author: Patrick Plechinger
    /// Creates a new user, assigns the appropriate role, and initializes
    /// trainee progress using the default curriculum when applicable.
    /// </summary>
    public async Task<IdentityResult> Create(CreateUser newUser) {
        // 1. Get the default curriculum ID
        Guid? firstCurriculumId = _curriculumService.getAll().FirstOrDefault()?.id;

        // 2. Instantiate the correct strongly-typed object
        User createdUser = newUser.role switch {
            "Admin" => new Admin(newUser.EMail, newUser.firstName, newUser.lastName),
            "Mentor" => new Mentor(newUser.EMail, newUser.firstName, newUser.lastName),
            "Trainee" => new Trainee(
                newUser.EMail,
                newUser.firstName,
                newUser.lastName,
                newUser.startDate,
                newUser.endDate),
            _ => throw new ArgumentException("Invalid role.")
        };

        createdUser.UserName = createdUser.Email;

        // add curriculum and corresponding lesson progress
        if (createdUser is Trainee trainee) {
            trainee.curriculumId = firstCurriculumId;

            if (trainee.curriculumId != null) {
                var curr = _curriculumService.getAll().FirstOrDefault(c => c.id == trainee.curriculumId);

                if (curr != null && curr.lessons != null) {
                    trainee.progresses = curr.lessons.Select(lesson => new LessonProgress {
                        traineeId = trainee.guid,
                        lessonId = lesson.id,
                        status = LessonStatus.Open,
                        title = lesson.title,
                        timeEstimate = lesson.timeEstimate,
                        url = lesson.url,
                        isActive = lesson.isActive,
                        position = lesson.defaultPosition
                    }).ToList();
                }
            }
        }

        // create user for the userManager
        var result = await _userManager.CreateAsync(createdUser);

        if (!result.Succeeded)
            return result;

        // add role for the user in the userManager
        await _userManager.AddToRoleAsync(createdUser, newUser.role);

        return IdentityResult.Success;
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Retrieves a user by their email address.
    /// </summary>
    public async Task<User> GetByEmail(string email) {
        return _userRepository.GetByEmail(email);
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Updates an existing user's information.
    /// </summary>
    public bool edit(EditUser newUser) {
        var user = _userRepository.GetById(newUser.guid);
        if (user == null) {
            return false;
        }
        switch (user.role) {
            case Role.Admin:
                user.Email = newUser.Email;
                user.UserName = newUser.Email;
                user.firstName = newUser.firstName;
                user.lastName = newUser.lastName;
                _userRepository.Edit(user);
                return true;
            case Role.Mentor:
                user.Email = newUser.Email;
                user.UserName = newUser.Email;
                user.firstName = newUser.firstName;
                user.lastName = newUser.lastName;
                _userRepository.Edit(user);
                return true;
            case Role.Trainee:
                // Casts the base User object to a Trainee so trainee-specific
                // fields such as start and end dates can also be modified.
                Trainee trainee = (Trainee)user;
                trainee.Email = newUser.Email;
                trainee.UserName = newUser.Email;
                trainee.firstName = newUser.firstName;
                trainee.lastName = newUser.lastName;
                trainee.startDate = newUser.startDate;
                trainee.endDate = newUser.endDate;
                _userRepository.Edit(trainee);
                return true;
            default:
                return false;
        }

    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Retrieves a users data in edit model format.
    /// </summary>
    public EditUser GetEditUser(Guid id) {
        var newUser = _userRepository.GetById(id);
        if (newUser == null) {
            return null;
        }
        switch (newUser.role) {
            case Role.Admin:
                return new EditUser(newUser.guid, newUser.role, newUser.Email, newUser.firstName, newUser.lastName, newUser.active);
            case Role.Mentor:
                return new EditUser(newUser.guid, newUser.role, newUser.Email, newUser.firstName, newUser.lastName, newUser.active);
            case Role.Trainee:
                Trainee newTrainee = (Trainee)newUser; //allows reading startDate and endDate
                return new EditUser(newTrainee.guid, newUser.role, newTrainee.Email, newTrainee.firstName, newTrainee.lastName, newTrainee.startDate, newTrainee.endDate, newUser.active);
            default:
                return null;
        }

    }




    public async Task<bool> removePassword(Guid user) {
        var newUser = _userRepository.GetById(user);
        if (newUser == null) {
            return false;
        }
        // Uses the Identity framework to safely remove the stored password hash.
        var success = await _userManager.RemovePasswordAsync(newUser);
        if (!success.Succeeded) {
            return false;
        }
        return true;

    }

    public async Task<string> setPassword(SetPassword setPassword) {
        // Verifies that both entered passwords are identical before continuing.
        if (!setPassword.password.Equals(setPassword.passwordRepeat)) {
            return "unequal";
        }
        var newUser = _userRepository.GetById(setPassword.guid);
        if (newUser == null) {
            return "inexistent";
        }
        // Prevents overwriting an already existing password.
        if (await _userManager.HasPasswordAsync(newUser)) {
            return "late";
        }
        // Delegates password validation and hashing to ASP.NET Identity.
        var success = await _userManager.AddPasswordAsync(newUser, setPassword.password);
        if (!success.Succeeded) {
            return "incorrect";
        }
        return "success";

    }



    /// <summary>
    /// Returns all persisted mentor-trainee assignments.
    /// Author: Dennis Axt.
    /// </summary>
    /// <returns>All persisted mentor-trainee assignments.</returns>
    public IEnumerable<MentorAssignment> getMentorAssignments() {
        return _userRepository.GetAssignments();
    }

    /// <summary>
    /// Creates a persisted assignment between a mentor and a trainee
    /// using their public identifiers.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="mentorId">The public identifier of the mentor.</param>
    /// <param name="traineeId">The public identifier of the trainee.</param>
    public void assignTrainee(Guid mentorId, Guid traineeId) {
        _userRepository.AssignById(
            traineeId,
            mentorId);
    }

    /// <summary>
    /// Removes the persisted assignment between a mentor and a trainee.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="mentorId">The public identifier of the mentor.</param>
    /// <param name="traineeId">The public identifier of the trainee.</param>
    public void unassignTrainee(Guid mentorId, Guid traineeId) {
        _userRepository.UnassignById(
            traineeId,
            mentorId);
    }

    /// <summary>
    /// Creates a persisted assignment from existing mentor and trainee
    /// objects. This overload is used by the development data seeder.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="mentor">The mentor to assign.</param>
    /// <param name="trainee">The trainee to assign to the mentor.</param>
    public void assignTrainee(Mentor mentor, Trainee trainee) {
        if (mentor == null) {
            throw new ArgumentNullException(nameof(mentor));
        }

        if (trainee == null) {
            throw new ArgumentNullException(nameof(trainee));
        }

        _userRepository.AssignById(
            trainee.guid,
            mentor.guid);
    }
}