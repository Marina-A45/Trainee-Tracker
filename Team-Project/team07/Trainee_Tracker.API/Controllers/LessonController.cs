using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Service;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Trainee_Tracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LessonController : ControllerBase {
    private readonly ILessonProgressService service;
    private readonly IHubContext<CurriculumHub> hubContext;

    public LessonController(ILessonProgressService lessonProgressService, IHubContext<CurriculumHub> hubContext) {
        service = lessonProgressService;
        this.hubContext = hubContext;
    }

    /// <param name="request">
    /// The lesson progress identifier, requested status, and optional
    /// rejection reason.
    /// Author: Lukas Nowak
    /// </param>
    /// <returns>
    /// A success response or an error response when status validation fails.
    /// </returns>
    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request) {
        try {
            service.setStatus(
                request.LessonId,
                request.NewStatus,
                request.RejectionReason);
            var senderConnectionId = Request.Headers["X-SignalR-Connection-Id"].ToString();

            if (!string.IsNullOrEmpty(senderConnectionId)) {
                // Send to everyone EXCEPT the person dragging the card
                await hubContext.Clients.AllExcept(senderConnectionId)
                    .SendAsync("RefreshCurriculumData", request.LessonId);
            }
            else {
                await hubContext.Clients.All.SendAsync("RefreshCurriculumData", request.LessonId);
            }

            return Ok(new { success = true });
        }
        catch (Exception ex) {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    /// <summary>
    /// Saves an individual order of open lessons for one trainee without
    /// changing the standard curriculum order.
    /// Author: Dennis Axt.
    /// </summary>
    /// <param name="request">
    /// The trainee identifier and ordered lesson progress identifiers.
    /// </param>
    /// <returns>
    /// A success response or an error response when validation fails.
    /// </returns>
    [HttpPost("update-order")]
    public async Task<IActionResult> UpdateOrder(
        [FromBody] UpdateLessonOrderRequest request) {
        try {
            // Validate permissions and persist the new positions
            // through the lesson progress service.
            service.setIndividualOrder(
                request.TraineeId,
                request.OrderedProgressIds);

            // Notify other open views that the trainee data has changed.
            await hubContext.Clients.All.SendAsync(
                "RefreshCurriculumData",
                request.TraineeId);

            return Ok(new { success = true });
        }
        catch (UnauthorizedAccessException ex) {
            // A mentor may only reorder lessons of assigned trainees.
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex) {
            // Reject incomplete, duplicated, or foreign progress IDs.
            return BadRequest(
                new { success = false, message = ex.Message });
        }
    }
}

public class UpdateStatusRequest {
    public Guid LessonId { get; set; }

    public LessonStatus NewStatus { get; set; }

    // Required when a mentor changes the lesson status to Rejected.
    public string? RejectionReason { get; set; }
}

/// <summary>
/// Contains the complete individual lesson order of one trainee.
/// </summary>
public class UpdateLessonOrderRequest {
    // Trainee whose individual lesson order should be changed.
    public Guid TraineeId { get; set; }

    // Progress IDs in the requested order, from first to last.
    public List<Guid> OrderedProgressIds { get; set; } = new();
}