using System.Text.Json;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using TraineeTracker.Controllers;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Author: Carl Böhm-Sturm
/// </summary>
public class FeedbackController : Controller {
    // Das Interface wird als schreibgeschütztes Feld hinterlegt
    private readonly IFeedbackService _service;
    private readonly ILessonProgressService _progressService;
    private readonly ILoginService _login;

    // Das Interface wird per Dependency Injection über den Konstruktor injiziert
    public FeedbackController(IFeedbackService service, ILessonProgressService progressService, ILoginService login) {
        _service = service;
        _progressService = progressService;
        _login = login;
    }

    /// <summary>
    /// Creates new feedback for a lesson and marks the associated
    /// lesson progress as rated.
    /// Originally implemented by Carl Böhm-Sturm.
    /// Extended by Dennis Axt with authenticated trainee resolution
    /// and the protected Accepted-to-Rated workflow.
    /// </summary>
    /// <param name="feedback">
    /// The feedback entity posted from the feedback form.
    /// </param>
    /// <param name="progressId">
    /// The lesson progress to move from Accepted to Rated;
    /// ignored when empty.
    /// </param>
    /// <returns>
    /// A redirect to the trainee lesson status page.
    /// </returns>
    [Authorize(Roles = "Trainee")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(
        Feedback feedback,
        Guid progressId) {
        // Use the authenticated trainee instead of trusting the user ID
        // submitted through the form.
        var currentUser = _login.GetCurrentUser()
            .GetAwaiter()
            .GetResult();

        if (currentUser == null) {
            return Challenge();
        }

        feedback.associatedUser = currentUser;
        feedback.creationDate = DateTime.UtcNow;

        // Save the feedback before moving the accepted lesson to Rated.
        _service.CreateFeedback(feedback);

        if (progressId != Guid.Empty) {
            _progressService.rateAfterFeedback(progressId);
        }

        return Redirect("/Trainee/LessonStatus");
    }

    /// <summary>
    /// Opens the feedback form in edit mode for an existing feedback.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="id">The unique identifier of the feedback to edit.</param>
    /// <returns>The feedback view in edit mode, or 404/403 if the feedback does not exist or is not editable by the current user.</returns>
    [Authorize]
    [HttpGet("Feedback/Edit/{id}")]
    public IActionResult Edit(Guid id) {
        // GetLatestFeedback loads the associated lesson/user, which the form
        // displays as read-only context
        var feedback = _service.GetLatestFeedback().FirstOrDefault(f => f.id == id);
        if (feedback == null) {
            return NotFound();
        }

        var currentUser = _login.GetCurrentUser().GetAwaiter().GetResult();
        if (currentUser == null || !CanModify(feedback, currentUser)) {
            return Forbid();
        }

        // Reuses the feedback form in edit mode instead of a separate view
        ViewBag.IsEdit = true;
        return View("Feedback", feedback);
    }

    /// <summary>
    /// Applies the edited values to an existing feedback and saves it.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="id">The unique identifier of the feedback to update.</param>
    /// <param name="difficulty">The felt difficulty on a scale from 1 to 5, optional.</param>
    /// <param name="actualEffort">How long the lesson actually took, optional.</param>
    /// <param name="knowledge">The previous knowledge of the trainee, optional.</param>
    /// <param name="text">The given feedback text, optional.</param>
    /// <returns>A redirect to the feedback overview, or 404/403 if the feedback does not exist or is not editable by the current user.</returns>
    [Authorize]
    [HttpPost("Feedback/Update")]
    [ValidateAntiForgeryToken]
    public IActionResult Update(Guid id, int? difficulty, double? actualEffort, string? knowledge, string? text) {
        // Only the feedback fields are editable; the associated lesson and
        // user of an existing feedback never change
        var feedback = _service.GetLatestFeedback().FirstOrDefault(f => f.id == id);
        if (feedback == null) {
            return NotFound();
        }

        var currentUser = _login.GetCurrentUser().GetAwaiter().GetResult();
        if (currentUser == null || !CanModify(feedback, currentUser)) {
            return Forbid();
        }

        feedback.difficulty = difficulty;
        feedback.actualEffort = actualEffort;
        feedback.knowledge = knowledge;
        feedback.text = text;
        _service.EditFeedback(feedback);
        return Redirect("/Feedback/AllFeedbacks");
    }

    /// <summary>
    /// Deletes the feedback with the given identifier.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="id">The unique identifier of the feedback to delete.</param>
    /// <returns>A 204 No Content result, or 404/403 if the feedback does not exist or is not deletable by the current user.</returns>
    [Authorize]
    [HttpDelete("Feedback/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id) {
        var feedback = _service.GetLatestFeedback().FirstOrDefault(f => f.id == id);
        if (feedback == null) {
            return NotFound();
        }

        var currentUser = _login.GetCurrentUser().GetAwaiter().GetResult();
        if (currentUser == null || !CanModify(feedback, currentUser)) {
            return Forbid();
        }

        _service.DeleteFeedback(id);
        return NoContent();
    }

    /// <summary>
    /// Shows all feedbacks and determines which of them the current user may edit or delete,
    /// depending on their role.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <returns>The overview view listing all feedbacks.</returns>
    [Authorize]
    public IActionResult AllFeedbacks() {
        var feedbacks = _service.GetLatestFeedback();

        // Everyone may see all feedbacks, but editing/deleting depends on the
        // role: Admins may edit all, Mentors only feedbacks of their assigned
        // trainees, Trainees only their own.
        var currentUser = _login.GetCurrentUser().GetAwaiter().GetResult();
        var editableIds = new HashSet<Guid>();
        switch (currentUser?.role) {
            case Role.Admin:
                editableIds = feedbacks.Select(f => f.id).ToHashSet();
                ViewData["DropdownOptions"] = AdminController.navbar_items;
                break;
            case Role.Mentor:
                var assignedGuids = _login.GetAssignedTrainees().GetAwaiter().GetResult().Select(t => t.guid).ToHashSet();
                editableIds = feedbacks.Where(f => f.associatedUser != null && assignedGuids.Contains(f.associatedUser.guid)).Select(f => f.id).ToHashSet();
                ViewData["DropdownOptions"] = MentorController.navbar_items;
                break;
            case Role.Trainee:
                editableIds = feedbacks.Where(f => f.associatedUser != null && f.associatedUser.guid == currentUser.guid).Select(f => f.id).ToHashSet();
                ViewData["DropdownOptions"] = TraineeController.navbar_items;
                break;
        }
        ViewBag.EditableFeedbackIds = editableIds;

        return View(feedbacks);
    }

    /// <summary>
    /// Returns all feedbacks that belong to the given lesson.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="lessonid">The unique identifier of the lesson.</param>
    /// <returns>The list of feedbacks for the lesson.</returns>
    [Authorize]
    public List<Feedback> GetByLesson(Guid lessonid) {
        return _service.GetFeedbackByLesson(lessonid);
    }

    /// <summary>
    /// Determines whether the given user may edit or delete the given feedback:
    /// Admins may modify any feedback, Mentors only feedback of their assigned
    /// trainees, Trainees only their own feedback.
    /// Author: Carl Böhm-Sturm
    /// </summary>
    /// <param name="feedback">The feedback being accessed.</param>
    /// <param name="currentUser">The currently authenticated user.</param>
    /// <returns>True if the user may edit or delete the feedback.</returns>
    private bool CanModify(Feedback feedback, User currentUser) {
        if (feedback.associatedUser == null) {
            return false;
        }

        return currentUser.role switch {
            Role.Admin => true,
            Role.Mentor => _login.GetAssignedTrainees()
                .GetAwaiter()
                .GetResult()
                .Any(trainee => trainee.guid == feedback.associatedUser.guid),
            Role.Trainee => feedback.associatedUser.guid == currentUser.guid,
            _ => false
        };
    }

}