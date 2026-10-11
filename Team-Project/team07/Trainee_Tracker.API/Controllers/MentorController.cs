using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TraineeTracker.Models;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Trainee_Tracker.API.Models;

namespace TraineeTracker.Controllers;

public class MentorController : Controller {
    private readonly ILoginService _login;
    private readonly ILessonProgressService _progressService;
    private readonly ITraineeService _traineeService;
    private readonly ITraineeProgressService _traineeProgressService;

    /// <summary>
    /// Initializes the controller services required for lesson progress,
    /// analytics, and role-specific trainee access.
    /// </summary>
    /// <param name="login">The current-user and assignment service.</param>
    /// <param name="progressService">The lesson progress service.</param>
    /// <param name="traineeProgressService">The trainee analytics service.</param>
    /// <param name="traineeService">The trainee access service.</param>
    public MentorController(
        ILoginService login,
        ILessonProgressService progressService,
        ITraineeProgressService traineeProgressService,
        ITraineeService traineeService) {
        _login = login;
        _progressService = progressService;
        _traineeProgressService = traineeProgressService;
        _traineeService = traineeService;

    }

    // List of links to be displayed in the Navbar
    public static List<SelectListItem> navbar_items = new List<SelectListItem> {
        new SelectListItem {Value = "/Mentor/MissingFeedback", Text = "Open Lessons for feedback"},
        new SelectListItem {Value = "/Mentor/TraineeProgress", Text = "Current TraineeProgress"},
        new SelectListItem {Value = "/Mentor/Curriculums", Text = "Import Curriculums"},
        new SelectListItem {Value = "/Feedback/AllFeedbacks", Text = "Feedbacks"},
    };

    /// <summary>
    /// Shows pending reviews for assigned trainees to mentors
    /// and for all active trainees to administrators.
    /// Originally implemented by Lukas Nowak.
    /// Extended by Dennis Axt with role-based trainee access and review loading.
    /// </summary>
    /// <returns>A task containing the pending lesson review view.</returns>
    [Authorize(Roles = "Mentor,Admin")]
    public async Task<IActionResult> MissingFeedback() {
        var currentUser = await _login.GetCurrentUser();

        if (currentUser == null) {
            return Challenge();
        }

        var isAdmin = currentUser.role == Role.Admin;
        var availableTrainees =
            await GetAvailableTrainees(currentUser);

        // Load only finished lessons that are waiting for a review.
        var pendingReviews = availableTrainees
            .SelectMany(trainee => _progressService
                .getAll(trainee.guid)
                .Where(progress =>
                    progress.status == LessonStatus.Finished)
                .OrderBy(progress => progress.position)
                .Select(progress => new MentorLessonReviewItem {
                    Trainee = trainee,
                    Progress = progress
                }))
            .ToList();

        var model = new MentorLessonReviewViewModel {
            AvailableTrainees = availableTrainees,
            PendingReviews = pendingReviews,
            IsAdmin = isAdmin
        };

        ViewData["DropdownOptions"] = isAdmin
            ? AdminController.navbar_items
            : navbar_items;

        return View(model);
    }

    /// <summary>
    /// Reviews a finished lesson by accepting it or rejecting it with a
    /// mandatory reason. Mentors are limited to their assigned trainees,
    /// while administrators may review lessons of all active trainees.
    /// Original review workflow by Carl Böhm-Sturm.
    /// Administrator access added by Dennis Axt.
    /// </summary>
    /// <param name="progressId">
    /// The unique identifier of the lesson progress to review.
    /// </param>
    /// <param name="decision">
    /// The review decision, either "accept" or "reject".
    /// </param>
    /// <param name="rejectionReason">
    /// The mandatory reason when rejecting a lesson.
    /// </param>
    /// <returns>
    /// A redirect to the MissingFeedback page.
    /// </returns>

    [Authorize(Roles = "Mentor,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewLesson(
        Guid progressId,
        string decision,
        string? rejectionReason) {
        var currentUser = await _login.GetCurrentUser();

        if (currentUser == null) {
            return Challenge();
        }

        // Mentors may review only lessons belonging to assigned trainees.
        // Administrators do not require a mentor-trainee assignment.
        if (currentUser.role == Role.Mentor) {
            var assignedTrainees =
                await _login.GetAssignedTrainees();

            var isAssigned = assignedTrainees.Any(trainee =>
                _progressService
                    .getAll(trainee.guid)
                    .Any(progress => progress.id == progressId));

            if (!isAssigned) {
                TempData["ReviewError"] =
                    "You can only review lessons of your assigned trainees.";

                return RedirectToAction(nameof(MissingFeedback));
            }
        }

        switch (decision) {
            case "accept":
                _progressService.setStatus(
                    progressId,
                    LessonStatus.Accepted);
                break;

            case "reject":
                if (string.IsNullOrWhiteSpace(rejectionReason)) {
                    TempData["ReviewError"] =
                        "Please enter a reason to reject a lesson.";
                }
                else {
                    _progressService.reject(
                        progressId,
                        rejectionReason);
                }
                break;

            default:
                TempData["ReviewError"] =
                    "Unknown review action.";
                break;
        }

        return RedirectToAction(nameof(MissingFeedback));
    }

    /// <summary>
    /// Shows assigned trainees to mentors and all active trainees
    /// to administrators.
    /// Originally implemented by Lukas Nowak.
    /// Extended by Dennis Axt with trainee selection, access control,
    /// and lesson progress loading.
    /// Extended by Tim Glaser with trainee analytics and additional routes.
    /// </summary>
    /// <param name="traineeId">The optional identifier of the trainee to display.</param>
    /// <returns>A task containing the selected trainee progress view.</returns>
    [Authorize(Roles = "Mentor,Admin")]
    [HttpGet("/Mentor/TraineeProgress")]
    [HttpGet("/Mentor/CurrentTraineeProgress")]
    public async Task<IActionResult> TraineeProgress(Guid? traineeId) {
        var currentUser = await _login.GetCurrentUser();
        var isAdmin = currentUser.role == Role.Admin;

    var availableTrainees = await GetAvailableTrainees(currentUser);

    var selectedTrainee = traineeId.HasValue
        ? availableTrainees.FirstOrDefault(trainee => trainee.guid == traineeId.Value)
        : null;

    selectedTrainee ??= availableTrainees.FirstOrDefault();

    var progresses = selectedTrainee == null
        ? new List<LessonProgress>()
        : _progressService
            .getAll(selectedTrainee.guid)
            .ToList();

    var traineeProgress = selectedTrainee == null
        ? null
        : await _traineeProgressService.calcAndSaveProgress(selectedTrainee.guid);

    var model = new MentorTraineeProgressViewModel {
        AvailableTrainees = availableTrainees,
        SelectedTrainee = selectedTrainee,
        Progresses = progresses,
        IsAdmin = isAdmin,
        TraineeProgress = traineeProgress
    };

    ViewData["DropdownOptions"] = isAdmin
        ? AdminController.navbar_items
        : navbar_items;

    return View(model);
}

    /// <summary>
    /// Resolves active trainees according to the current user's role.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="currentUser">
    /// The mentor or administrator requesting trainee access.
    /// </param>
    /// <returns>
    /// A task containing the active trainees accessible to the current user.
    /// </returns>
    private async Task<List<Trainee>> GetAvailableTrainees(
        User currentUser) {
        IEnumerable<Trainee> trainees;

        if (currentUser.role == Role.Admin) {
            // Administrators have unrestricted access to active trainees.
            trainees = _traineeService.GetAllTrainees();
        }
        else if (currentUser.role == Role.Mentor) {
            // Mentors remain restricted to their persisted assignments.
            trainees = await _login.GetAssignedTrainees();
        }
        else {
            return new List<Trainee>();
        }

        return trainees
            .Where(trainee => trainee.active)
            .OrderBy(trainee => trainee.lastName)
            .ThenBy(trainee => trainee.firstName)
            .ToList();
    }
    public IActionResult Curriculums() {
        ViewData["DropdownOptions"] = navbar_items;
        return View();
    }
}