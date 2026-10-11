using Microsoft.AspNetCore.Mvc;
using Trainee_Tracker.Core.Service;

namespace TraineeTracker.Controllers;

// Author: Marina Achner
/// <summary>
/// API controller responsible for planner-related operations.
/// Provides endpoints to retrieve planner data for trainees.
/// </summary>
[ApiController]
[Route("api/planner")]
public class PlannerController : ControllerBase {
    private readonly IPlannerService _plannerService;

    // Author: Marina Achner
    /// <summary>
    /// Initializes a new instance of the <see cref="PlannerController"/> class.
    /// </summary>
    /// <param name="plannerService">
    /// Service responsible for handling planner-related business logic.
    /// </param>
    public PlannerController(IPlannerService plannerService) {
        _plannerService = plannerService;
    }

    // Author: Marina Achner
    /// <summary>
    /// Retrieves planner data for a specific trainee.
    /// </summary>
    /// <param name="traineeId">
    /// The unique identifier (GUID) of the trainee whose planner should be retrieved.
    /// </param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with planner data if the planner exists.
    /// Returns <see cref="NotFoundResult"/> if no planner data can be found.
    /// Returns <see cref="StatusCodeResult"/> with status code 500 if an unexpected error occurs.
    /// </returns>
    [HttpGet("{traineeId}")]
    public async Task<IActionResult> GetPlanner(Guid traineeId) {
        try {
            var result = await _plannerService.GetPlanner(traineeId);
            return Ok(result);
        }
        catch (Exception ex) {
            return StatusCode(500, ex.ToString());
        }
    }
}