using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Trainee_Tracker.Core.Repository;
namespace Trainee_Tracker.Repository;

/// <summary>
/// Author: Lukas Nowak
/// </summary>
public class UserRepository : IUserRepository {
    private List<User> users = new List<User>();
    private TraineeTrackerContext context;
    public UserRepository(TraineeTrackerContext context) {
        this.context = context;
    }
    /// <summary>
    /// Retrieves all users from the database
    /// </summary>
    /// <returns>An enumerable collection of <see cref="User"/> entities.</returns>
    public IEnumerable<User> GetAll() {
        return context.users;
    }

    /// <summary>
    /// Retrieves a specific User by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the User.</param>
    /// <returns>The found <see cref="User"/> entity.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no User matches the given ID.</exception>
    public User GetById(Guid id) {
        User? curr = context.users.FirstOrDefault(m => m.guid == id);
        return curr;
    }
    /// <summary>
    /// Checks if a user exists in the database by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) to check.</param>
    /// <returns>True if the user exists; otherwise, false.</returns>
    public bool Exists(Guid id) {
        return context.users.Any(l => l.guid == id);
    }
    /// <summary>
    /// Checks if a specific user instance exists within the DB context dataset.
    /// </summary>
    /// <param name="User">The user entity to search for.</param>
    /// <returns>True if the exact entity is found; otherwise, false.</returns>
    public bool Exists(User user) {
        return context.users.Contains(user);
    }
    /// <summary>
    /// Creates a new useer.
    /// </summary>
    /// <param name="User">The user entity to save.</param>
    public void Create(User user) {
        Console.WriteLine($"[DEBUG] Create called for user {user.guid}");

        // 1. Pattern match: Check if user is a Trainee, and if so, cast it to a local 'trainee' variable
        if (user is Trainee trainee && trainee.curriculumId != null) {
            Console.WriteLine($"[DEBUG] Curriculum ID detected: {trainee.curriculumId}");

            Curriculum? curr = context.curriculums
                .Include(c => c.lessons)
                .FirstOrDefault(c => c.id == trainee.curriculumId);

            if (curr != null && curr.lessons != null) {
                Console.WriteLine($"[DEBUG] Found curriculum. Mapping {curr.lessons.Count()} lessons...");

                // 2. Assign the progresses directly to the typed 'trainee' object
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
                Console.WriteLine(trainee.guid);
            }
            else {
                Console.WriteLine("[DEBUG] Curriculum or lessons were NULL.");
            }
        }

        // EF Core tracks the reference, so saving 'user' will still save the Trainee data!
        context.users.Add(user);
        context.SaveChanges();
        Console.WriteLine("[DEBUG] Changes saved to database successfully.");
    }

    /// <summary>
    /// Updates a user parameter by parameter
    /// </summary>
    /// <param name="User">The user entity to update.</param>
    public void Edit(User user) {
        var existing = context.users.Find(user.Id);
        if (existing == null) {
            throw new KeyNotFoundException(
                $"User with ID '{user.Id}' was not found.");
        }
        existing.firstName = user.firstName;
        existing.lastName = user.lastName;
        existing.Email = user.Email;
        existing.UserName = user.UserName;
        existing.role = user.role;
        existing.active = user.active;
        context.SaveChanges();
    }
    /// <summary>
    /// Deletes a specific user entity, handling both tracked and untracked contexts safely.
    /// </summary>
    /// <param name="User">The user entity instance to delete.</param>
    public void Delete(User User) {
        var trackedEntity = context.users.FirstOrDefault(m => m.guid == User.guid);

        if (trackedEntity != null) {
            // If the entity is already tracked by EF, remove the tracked reference
            context.users.Remove(trackedEntity);
        }
        else {
            // If untracked, attach it to the context first, then issue the removal state
            context.users.Attach(User);
            context.users.Remove(User);
        }
        context.SaveChanges();
    }


    /// <summary>
    /// Deletes a user entity directly by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the user to remove.</param>    
    public void DeleteById(Guid id) {
        var trackedEntity = context.users.FirstOrDefault(m => m.guid == id);

        if (trackedEntity != null) {
            context.users.Remove(trackedEntity);
        }
        context.SaveChanges();
    }


    /// <summary>
    /// Author: Patrick Plechinger
    /// </summary>

    public void Deactivate(Guid id) {
        User user = GetById(id);
        var trackedEntity = context.users.FirstOrDefault(m => m.guid == id);

        if (trackedEntity != null) {
            user.active = false;
            Edit(user);
        }
        context.SaveChanges();
    }
    public void Activate(Guid id) {
        User user = GetById(id);
        var trackedEntity = context.users.FirstOrDefault(m => m.guid == id);

        if (trackedEntity != null) {
            user.active = true;
            Edit(user);
        }
        context.SaveChanges();
    }


    /// <summary>
    /// Retrieves a specific User by its email.
    /// </summary>
    /// <param name="email">The email of the User.</param>
    /// <returns>The found <see cref="User"/> entity.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no User matches the given email.</exception>
    public User GetByEmail(string email) {
        User? curr = context.users.FirstOrDefault(u => u.Email == email);
        if (curr == null) {
            throw new KeyNotFoundException($"User with ID '{email}' was not found.");
        }
        return curr;
    }

    /// <summary>
    /// Retrieves all Trainees mentored by a specific Mentor.
    /// </summary>
    /// <param name="mentorId">The Id of the Mentor.</param>
    /// <returns>A list of <see cref="User"/> entities.</returns>
    public IEnumerable<User> GetByMentor(Guid mentorId) {
        IEnumerable<Trainee> trainees = Enumerable.Empty<Trainee>();
        var assignments = context.assignments.Where(u => u.mentor.guid == mentorId);
        foreach (var assignment in assignments) {
            trainees.Append(assignment.trainee);
        }
        return trainees;
    }
    /// <summary>
    /// Returns all persisted mentor-trainee assignments, including
    /// the associated mentor and trainee entities.
    /// Author: Dennis Axt.
    /// </summary>
    /// <returns>All persisted mentor-trainee assignments.</returns>
    public IEnumerable<MentorAssignment> GetAssignments() {
        return context.assignments
            .Include(assignment => assignment.mentor)
            .Include(assignment => assignment.trainee)
            .AsNoTracking()
            .ToList();
    }
    /// <summary>
    /// Creates a persisted assignment between a mentor and a trainee.
    /// Existing assignments are not duplicated.
    /// Originally implemented by Patrick Plechinger.
    /// Extended by Dennis Axt with validation and duplicate protection.
    /// </summary>
    /// <param name="traineeId">The public identifier of the trainee.</param>
    /// <param name="mentorId">The public identifier of the mentor.</param>
    public void AssignById(Guid traineeId, Guid mentorId) {
        var trainee = context.users
            .OfType<Trainee>()
            .FirstOrDefault(user => user.guid == traineeId);

        if (trainee == null) {
            throw new KeyNotFoundException(
                $"Trainee with ID '{traineeId}' was not found.");
        }

        var mentor = context.users
            .OfType<Mentor>()
            .FirstOrDefault(user => user.guid == mentorId);

        if (mentor == null) {
            throw new KeyNotFoundException(
                $"Mentor with ID '{mentorId}' was not found.");
        }

        var alreadyAssigned = context.assignments.Any(
            assignment =>
                assignment.TraineeId == trainee.Id &&
                assignment.MentorId == mentor.Id);

        if (alreadyAssigned) {
            return;
        }

        context.assignments.Add(
            new MentorAssignment(trainee, mentor));

        context.SaveChanges();
    }

    /// <summary>
    /// Removes a persisted assignment between a mentor and a trainee.
    /// Originally implemented by Patrick Plechinger.
    /// Extended by Dennis Axt with entity validation and idempotent removal.
    /// </summary>
    /// <param name="traineeId">The public identifier of the trainee.</param>
    /// <param name="mentorId">The public identifier of the mentor.</param>
    public void UnassignById(Guid traineeId, Guid mentorId) {
        var trainee = context.users
            .OfType<Trainee>()
            .FirstOrDefault(user => user.guid == traineeId);

        if (trainee == null) {
            throw new KeyNotFoundException(
                $"Trainee with ID '{traineeId}' was not found.");
        }

        var mentor = context.users
            .OfType<Mentor>()
            .FirstOrDefault(user => user.guid == mentorId);

        if (mentor == null) {
            throw new KeyNotFoundException(
                $"Mentor with ID '{mentorId}' was not found.");
        }

        var assignment = context.assignments.FirstOrDefault(
            current =>
                current.TraineeId == trainee.Id &&
                current.MentorId == mentor.Id);

        if (assignment == null) {
            return;
        }

        context.assignments.Remove(assignment);
        context.SaveChanges();
    }
}