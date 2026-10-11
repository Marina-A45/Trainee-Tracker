using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Repository;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.DTO;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Trainee_Tracker.Service;
/// <summary>
/// Author: Lukas Nowak
/// </summary>
public class LessonProgressService : ILessonProgressService {
    private ILessonProgressRepository repo;
    private ILoginService login;

    // Defines all status transitions that a trainee is allowed to perform.
    private static readonly HashSet<(LessonStatus From, LessonStatus To)>
        TraineeTransitions = new()
        {
            (LessonStatus.Open, LessonStatus.Started),
            (LessonStatus.Started, LessonStatus.Open),
            (LessonStatus.Started, LessonStatus.Finished),
            (LessonStatus.Finished, LessonStatus.Started),
            (LessonStatus.Rejected, LessonStatus.Finished),
            (LessonStatus.Accepted, LessonStatus.Rated)
        };

    // Defines additional status transitions that an assigned mentor is allowed to perform.
    private static readonly HashSet<(LessonStatus From, LessonStatus To)>
        MentorTransitions = new()
        {
            (LessonStatus.Open, LessonStatus.Skipped),
            (LessonStatus.Skipped, LessonStatus.Open),
            (LessonStatus.Finished, LessonStatus.Accepted),
            (LessonStatus.Accepted, LessonStatus.Finished),
            (LessonStatus.Finished, LessonStatus.Rejected),
            (LessonStatus.Accepted, LessonStatus.Rated)
        };

    public LessonProgressService(ILessonProgressRepository repo, ILoginService login) {
        this.repo = repo;
        this.login = login;
    }

    /// <summary>
    /// Returns all lesson progress records accessible to the current user.
    /// Originally implemented by Lukas Nowak.
    /// Extended by Dennis Axt with role-based access control.
    /// </summary>
    /// <returns>The lesson progress records accessible to the current user.</returns>
    public IEnumerable<LessonProgress> getAll() {
        var currentUser = login.GetCurrentUser()
            .GetAwaiter()
            .GetResult();

        if (currentUser == null) {
            throw new UnauthorizedAccessException(
                "The user must be logged in.");
        }

        if (currentUser.role == Role.Trainee) {
            return repo.GetAll(currentUser.guid).ToList();
        }

        if (currentUser.role == Role.Mentor) {
            var assignedTrainees = login.GetAssignedTrainees()
                .GetAwaiter()
                .GetResult();

            return assignedTrainees
                .SelectMany(trainee => repo.GetAll(trainee.guid))
                .ToList();
        }

        if (currentUser.role == Role.Admin) {
            return repo.GetAll().ToList();
        }

        throw new UnauthorizedAccessException(
            "The user is not allowed to read lesson progress.");
    }

    /// <summary>
    /// Returns the lesson progress records of a specific trainee after
    /// validating the current user's access.
    /// Originally implemented by Lukas Nowak.
    /// Extended by Dennis Axt with ownership and assignment validation.
    /// </summary>
    /// <param name="traineeId">
    /// The identifier of the trainee whose progress is requested.
    /// </param>
    /// <returns>The accessible lesson progress records of the trainee.</returns>
    public IEnumerable<LessonProgress> getAll(Guid traineeId) {
        var currentUser = login.GetCurrentUser()
            .GetAwaiter()
            .GetResult();

        if (currentUser == null) {
            throw new UnauthorizedAccessException(
                "The user must be logged in.");
        }

        if (currentUser.role == Role.Trainee &&
            currentUser.guid != traineeId) {
            throw new UnauthorizedAccessException(
                "Trainees may only read their own lesson progress.");
        }

        if (currentUser.role == Role.Mentor) {
            var assignedTrainees = login.GetAssignedTrainees()
                .GetAwaiter()
                .GetResult();

            if (!assignedTrainees.Any(trainee => trainee.guid == traineeId)) {
                throw new UnauthorizedAccessException(
                    "Mentors may only read lesson progress of assigned trainees.");
            }
        }
        else if (currentUser.role != Role.Trainee &&
                 currentUser.role != Role.Admin) {
            throw new UnauthorizedAccessException(
                "The user is not allowed to read lesson progress.");
        }

        return repo.GetAll(traineeId).ToList();
    }

    /// <summary>
    /// Changes the status of a lesson progress entry after validating
    /// the current user's role, permissions, and requested transition.
    /// Originally implemented by Lukas Nowak.
    /// Extended by Dennis Axt with authorization, transition validation,
    /// rejection reasons, and protected feedback-based rating.
    /// </summary>
    /// <param name="progressId">The identifier of the lesson progress.</param>
    /// <param name="status">The requested new lesson status.</param>
    /// <param name="rejectionReason">
    /// The mandatory reason when the requested status is Rejected.
    /// </param>
    public void setStatus(
        Guid progressId,
        LessonStatus status,
        string? rejectionReason = null) {
        // Identify the currently authenticated user.
        var currentUser = login.GetCurrentUser()
            .GetAwaiter()
            .GetResult();

        if (currentUser == null) {
            throw new UnauthorizedAccessException(
                "The user must be logged in.");
        }

        // Load the progress entry that should be changed.
        var progress = repo.GetById(progressId);

        if (currentUser.role == Role.Trainee) {
            // A trainee may only modify their own progress.
            if (progress.traineeId != currentUser.guid) {
                throw new UnauthorizedAccessException(
                    "Trainees may only modify their own lesson progress.");
            }

            // Trainees may only use transitions defined by the customer.
            var transition = (progress.status, status);

            if (!TraineeTransitions.Contains(transition)) {
                throw new InvalidOperationException(
                    $"Trainees cannot change a lesson from {progress.status} to {status}.");
            }
        }
        else if (currentUser.role == Role.Mentor) {
            // A mentor may only modify progress belonging to an assigned trainee.
            var assignedTrainees = login.GetAssignedTrainees()
                .GetAwaiter()
                .GetResult();

            if (!assignedTrainees.Any(t => t.guid == progress.traineeId)) {
                throw new UnauthorizedAccessException(
                    "Mentors may only modify assigned trainees.");
            }

            // An assigned mentor may use trainee transitions and mentor-specific transitions.
            var transition = (progress.status, status);

            if (!TraineeTransitions.Contains(transition) &&
                !MentorTransitions.Contains(transition)) {
                throw new InvalidOperationException(
                    $"Mentors cannot change a lesson from {progress.status} to {status}.");
            }
        }
        else if (currentUser.role != Role.Admin) {
            throw new UnauthorizedAccessException(
                "The user is not allowed to modify lesson progress.");
        }

        // A rejection must always include a non-empty reason.
        if (status == LessonStatus.Rejected) {
            if (string.IsNullOrWhiteSpace(rejectionReason)) {
                throw new ArgumentException(
                    "A rejection reason is required.",
                    nameof(rejectionReason));
            }

            progress.rejectionReason = rejectionReason.Trim();
        }

        // Persist the validated status change.
        progress.status = status;
        repo.Edit(progress);
    }
    /// <summary>
    /// Stores an individual order of open lessons for one trainee without
    /// changing the standard order of the imported curriculum.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="traineeId">
    /// The identifier of the trainee whose open lessons are reordered.
    /// </param>
    /// <param name="orderedProgressIds">
    /// All open lesson progress identifiers in the requested order.
    /// </param>
    public void setIndividualOrder(
        Guid traineeId,
        IEnumerable<Guid> orderedProgressIds) {
        // Identify the user who requested the order change.
        var currentUser = login.GetCurrentUser()
            .GetAwaiter()
            .GetResult();

        if (currentUser == null) {
            throw new UnauthorizedAccessException(
                "The user must be logged in.");
        }

        // Mentors may only individualize the curriculum of assigned trainees.
        if (currentUser.role == Role.Mentor) {
            var assignedTrainees = login.GetAssignedTrainees()
                .GetAwaiter()
                .GetResult();

            if (!assignedTrainees.Any(trainee => trainee.guid == traineeId)) {
                throw new UnauthorizedAccessException(
                    "Mentors may only reorder lessons of assigned trainees.");
            }
        }
        else if (currentUser.role != Role.Admin) {
            throw new UnauthorizedAccessException(
                "Only mentors and admins may individualize lesson order.");
        }

        if (orderedProgressIds == null) {
            throw new ArgumentNullException(nameof(orderedProgressIds));
        }
        // Load the trainee-specific progress entries without modifying
        // the standard lessons stored in the curriculum.
        var progresses = repo.GetAll(traineeId).ToList();

        // Only open lessons represent future curriculum work and may be reordered.
        var reorderableProgresses = progresses
            .Where(progress => progress.status == LessonStatus.Open)
            .ToList();

        var orderedIds = orderedProgressIds.ToList();

        // Require every open progress ID exactly once to prevent foreign,
        // missing, or duplicated lessons from being stored.
        var containsDuplicates =
            orderedIds.Distinct().Count() != orderedIds.Count;

        var containsExactlyOpenProgresses =
            orderedIds.Count == reorderableProgresses.Count &&
            orderedIds.ToHashSet().SetEquals(
                reorderableProgresses.Select(progress => progress.id));

        if (containsDuplicates || !containsExactlyOpenProgresses) {
            throw new ArgumentException(
                "The order must contain every open lesson progress of the trainee exactly once.",
                nameof(orderedProgressIds));
        }

        var progressesById = reorderableProgresses.ToDictionary(
            progress => progress.id);

        // Reuse the existing open-lesson defaultPosition slots so completed lessons
        // and the standard curriculum order remain unchanged.
        var availablePositions = reorderableProgresses
            .Select(progress => progress.position)
            .OrderBy(position => position)
            .ToList();

        // Assign the available positions according to the requested order.
        for (var index = 0; index < orderedIds.Count; index++) {
            var progress = progressesById[orderedIds[index]];
            var newPosition = availablePositions[index];

            // Avoid unnecessary database updates when a defaultPosition is unchanged.
            if (progress.position == newPosition) {
                continue;
            }

            progress.position = newPosition;
            repo.Edit(progress);
        }

    }

    /// <summary>
    /// Marks an accepted lesson as rated after its trainee has submitted
    /// feedback for it.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="progressId">
    /// The identifier of the lesson progress associated with the feedback.
    /// </param>
    public void rateAfterFeedback(Guid progressId) {
        // Identify the trainee who submitted the feedback.
        var currentUser = login.GetCurrentUser()
            .GetAwaiter()
            .GetResult();

        if (currentUser == null ||
            currentUser.role != Role.Trainee) {
            throw new UnauthorizedAccessException(
                "Only trainees may rate a lesson by submitting feedback.");
        }

        // Load the progress associated with the submitted feedback.
        var progress = repo.GetById(progressId);

        if (progress.traineeId != currentUser.guid) {
            throw new UnauthorizedAccessException(
                "Trainees may only rate their own lesson progress.");
        }

        if (progress.status != LessonStatus.Accepted) {
            throw new InvalidOperationException(
                "Only an accepted lesson can be rated.");
        }

        // Persist the status reached through the feedback workflow.
        progress.status = LessonStatus.Rated;
        repo.Edit(progress);
    }

    /// <summary>
    /// Rejects a lesson after applying the regular status validation.
    /// </summary>
    public void reject(Guid progressId, string reason) {
        setStatus(
            progressId,
            LessonStatus.Rejected,
            reason);
    }
}
