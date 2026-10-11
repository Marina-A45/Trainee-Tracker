using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Trainee_Tracker.Core.Enums;

namespace TraineeTracker.Controllers;

/// <summary>
/// Controller responsible for trainee-related views and API endpoints.
/// Provides pages such as Overview, Lesson Status, and Feedback,
/// as well as trainee data retrieval endpoints.
/// </summary>
[ApiController]
[Route("api/trainee")]
public class TraineeController : Controller {
    /// <summary>
    /// Navigation items displayed in the trainee UI navbar/dropdown.
    /// These links are passed to the view via ViewData.
    /// </summary>
    public static List<SelectListItem> navbar_items = new List<SelectListItem>
    {
        new SelectListItem { Value = "/Trainee/Overview", Text = "Overview" },
        new SelectListItem { Value = "/Trainee/LessonStatus", Text = "Lesson Status" },
    };

    // Service layer for trainee-related business logic
    private readonly ITraineeService _traineeService;

    // Used to fetch a real lesson as dummy data for the feedback page
    private ILessonProgressService lessonProgressService;
    private ILoginService login;
    private readonly IFeedbackService _feedbackService;

    private readonly ITraineeProgressService _traineeProgressService;
    private readonly IBurnDownChartService _burnDownChartService;

    /// <summary>
    /// Constructor with dependency injection of required services.
    /// </summary>
    public TraineeController(ITraineeService traineeService, ILessonProgressService lessonProgressService, ILoginService login, IFeedbackService feedbackService, ITraineeProgressService traineeProgressService, IBurnDownChartService burnDownChartService) {
        _traineeService = traineeService;
        _burnDownChartService = burnDownChartService;
        this.lessonProgressService = lessonProgressService;
        this.login = login;
        _feedbackService = feedbackService;
        _traineeProgressService = traineeProgressService;
    }
    /// <summary>
    /// Renders the trainee overview page.
    /// </summary>
    /// <returns>The Overview view with navigation data.</returns>
    [HttpGet("/Trainee/Overview")]
    public async Task<IActionResult> Overview() {
        // Pass navbar items to the view for rendering dropdown/navigation
        ViewData["DropdownOptions"] = navbar_items;

        var currentUser = await login.GetCurrentUser();
        if (currentUser == null) {
            return RedirectToAction("Login", "Home");
        }
        var trainee = _traineeService.GetTraineeById(currentUser.guid);
        if (trainee == null) {
            return NotFound("Current user is not a trainee.");
        }

        ViewBag.traineeId = trainee.guid;

        var traineeProgress = await _traineeProgressService.calcAndSaveProgress(trainee.guid);

        return View(traineeProgress);
    }

    /// <summary>
    /// Shows lesson progress for the authenticated trainee or for an active
    /// trainee selected by an administrator.
    /// Originally implemented by Lukas Nowak.
    /// Extended by Dennis Axt with role-based access and administrator
    /// trainee selection.
    /// </summary>
    /// <param name="traineeId">
    /// The optional identifier of the trainee selected by an administrator.
    /// </param>
    /// <returns>The lesson status view for the resolved trainee.</returns>
    [Authorize(Roles = "Trainee,Admin")]
    [HttpGet("/Trainee/LessonStatus")]
    public IActionResult LessonStatus(Guid? traineeId) {
        // Identify the authenticated user before resolving the trainee context.
        var currentUser = login.GetCurrentUser()
            .GetAwaiter()
            .GetResult();

        if (currentUser == null) {
            return Challenge();
        }

        Guid selectedTraineeId;

        if (currentUser.role == Role.Admin) {
            // Administrators must explicitly select the trainee whose progress
            // they want to inspect or modify.
            if (!traineeId.HasValue) {
                return RedirectToAction("Overview", "Admin");
            }

            // Validate that the requested ID belongs to an existing trainee.
            var selectedTrainee = _traineeService.GetTraineeById(
                traineeId.Value);
            // Closed trainee accounts are only visible in user management.
            if (!selectedTrainee.active) {
                return RedirectToAction("Overview", "Admin");
            }

            selectedTraineeId = selectedTrainee.guid;
            ViewBag.SelectedTraineeName =
                $"{selectedTrainee.firstName} {selectedTrainee.lastName}";
            ViewData["DropdownOptions"] = AdminController.navbar_items;
        }
        else if (currentUser.role == Role.Trainee) {
            // Trainees always see their own progress, regardless of query input.
            selectedTraineeId = currentUser.guid;
            ViewData["DropdownOptions"] = navbar_items;
        }
        else {
            return Forbid();
        }

        // Load only the lesson progress belonging to the resolved trainee.
        var progresses = lessonProgressService
            .getAll(selectedTraineeId)
            .ToList();

        // Provide the selected trainee context to the shared Razor view.
        ViewBag.SelectedTraineeId = selectedTraineeId;
        ViewBag.IsAdminView = currentUser.role == Role.Admin;

        return View(progresses);
    }

    /// <summary>
    /// Renders the feedback page for trainees.
    /// </summary>
    /// <returns>The Feedback view with navigation data.</returns>
    [HttpGet("/Trainee/Feedback")]
    public IActionResult Feedback(Guid lessonId, Guid progressId) {
        ViewData["DropdownOptions"] = navbar_items;
        var currentUser = login.GetCurrentUser().GetAwaiter().GetResult();
        ViewBag.traineeId = currentUser.guid;
        ViewBag.lessonId = lessonId;
        // Passed through the feedback form so the progress entry can be set to
        // Rated after the feedback is submitted.
        ViewBag.progressId = progressId;
        // The feedback form lives with the FeedbackController views
        return View("~/Views/Feedback/Feedback.cshtml");
    }

    /// <summary>
    /// Shows the feedback the current trainee has given for a rated lesson.
    /// Reuses the AllFeedbacks view, restricted to the trainee's own feedback,
    /// which they may also edit/delete.
    /// </summary>
    [HttpGet("/Trainee/SeeFeedback")]
    public IActionResult SeeFeedback(Guid lessonId) {
        ViewData["DropdownOptions"] = navbar_items;
        var currentUser = login.GetCurrentUser().GetAwaiter().GetResult();
        // GetLatestFeedback is used because it includes the associated lesson
        // and user needed for filtering and display
        var feedbacks = _feedbackService.GetLatestFeedback()
                                        .Where(f => f.associatedLesson?.id == lessonId
                                                    && f.associatedUser?.guid == currentUser.guid)
                                        .ToList();
        ViewBag.EditableFeedbackIds = feedbacks.Select(f => f.id).ToHashSet();
        return View("~/Views/Feedback/AllFeedbacks.cshtml", feedbacks);
    }

    // Author: Marina Achner
    /// <summary>
    /// Retrieves a single trainee by their unique identifier.
    /// </summary>
    /// <param name="traineeId">The unique ID of the trainee.</param>
    /// <returns>
    /// 200 OK with trainee data if found.  
    /// 404 Not Found if the trainee does not exist or an error occurs.
    /// </returns>
    [HttpGet("{traineeId}")]
    public IActionResult GetTraineeById(Guid traineeId) {
        try {
            // Fetch trainee from service layer
            var trainee = _traineeService.GetTraineeById(traineeId);

            return Ok(trainee);
        }
        catch (Exception ex) {
            // Return 404 if trainee is not found or an error occurs
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Reloads the lesson board with progress data accessible to the
    /// authenticated trainee, assigned mentor, or administrator.
    /// Originally implemented by Lukas Nowak.
    /// Extended by Dennis Axt with role-based progress access.
    /// </summary>
    /// <param name="traineeId">
    /// The identifier of the trainee whose lesson board is requested.
    /// </param>
    /// <returns>The lesson board partial view containing accessible progress.</returns>
    [Authorize(Roles = "Trainee,Mentor,Admin")]
    [HttpGet("/Trainee/LessonBoardPartial")]
    public IActionResult GetLessonBoardPartial(Guid traineeId) {
        // The service validates ownership for trainees, assignments for mentors,
        // and unrestricted trainee access for administrators.
        var progresses = lessonProgressService
            .getAll(traineeId)
            .ToList();

        return PartialView("LessonBoardPartial", progresses);
    }
    /// <summary>
    /// Fetches the Chartdata of the current Trainee
    /// </summary>
    [HttpGet("burndown-chart/{traineeId:guid}")]
    public async Task<ActionResult<BurnDownChartDto>> GetBurnDownChartData(Guid traineeId, CancellationToken cancellationToken) {

        try {
            var chartData = await _burnDownChartService.GetChartData(traineeId, cancellationToken);
            return Ok(chartData);
        }
        catch (Exception ex) {
            return StatusCode(500, ex.ToString());
        }

    }
}