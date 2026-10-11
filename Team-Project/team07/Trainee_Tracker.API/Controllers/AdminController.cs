using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using TraineeTracker.Models;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.API.Models;

namespace TraineeTracker.Controllers;

/// <summary>
/// Handles administrative user management operations.
/// </summary>
public class AdminController : Controller {
    // List of links to be displayed in the Navbar
    public static List<SelectListItem> navbar_items = new List<SelectListItem> {
        new SelectListItem {Value = "/Admin/Create", Text = "Create User"},
        new SelectListItem {Value = "/Admin/Overview", Text = "Overview"},
        new SelectListItem {Value = "/Admin/AssignTrainees", Text = "AssignTrainees"},

        // Shared Mentor/Admin progress and review functions.
        new SelectListItem {
            Value = "/Mentor/TraineeProgress",
            Text = "Trainee Progress"
        },
        new SelectListItem {
            Value = "/Mentor/MissingFeedback",
            Text = "Open Lessons for feedback"
        }
    };
    private readonly IUserService service;
    /// <summary>
    /// Author: Patrick Plechinger
    /// Initializes the controller with the required user service.
    /// </summary>
    public AdminController(IUserService service) {
        this.service = service;
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Displays the user creation form.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public IActionResult Create() {
        ViewData["DropdownOptions"] = navbar_items;
        return View(new CreateUser());
    }
    /// <summary>
    /// Author: Patrick Plechinger
    /// Validates the submitted form and creates a new user.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser(CreateUser createUser) {
        // Verifies that all validation attributes on the submitted model
        // have been satisfied before attempting to create a new user.
        if (!ModelState.IsValid) {
            // Returns to the creation page if the ModelState is invalid.
            return View("Create", createUser);
        }

        var created = await service.Create(createUser);

        if (!created.Succeeded) {
            // Returns to the creation page if the Identity framework
            // reports that user creation was unsuccessful.
            return View("Create", createUser);
        }

        // Redirects the administrator to the overview after
        // the user has been successfully created.
        return RedirectToAction("Overview");
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Displays the edit form for the selected user.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public IActionResult Edit(Guid id) {
        ViewData["DropdownOptions"] = navbar_items;

        // Retrieves the editable DTO used to populate the form.
        var model = service.GetEditUser(id);

        if (model == null) {
            ModelState.AddModelError("", "The selected user does not exist.");
            return RedirectToAction("Overview");
        }

        return View(model);
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Updates the information of an existing user.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult EditUser(EditUser editUser) {
        // Ensures that the submitted form satisfies all validation rules
        // before attempting to update the persisted user.
        if (!ModelState.IsValid) {
            // Returns to the edit page if the ModelState is invalid.
            return View("Edit", editUser);
        }

        // Delegates the update operation to the service layer, which
        // applies the changes based on the user's role.
        bool success = service.edit(editUser);

        if (!success) {
            // Handles the case where the selected user no longer exists,
            // or a wrong guid is used
            ModelState.AddModelError("", "The selected user does not exist.");

            return RedirectToAction("Overview");
        }

        // Redirects to the user overview after the update has been
        // successfully persisted.
        return RedirectToAction("Overview");
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Removes the password of the selected user.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RemovePassword(EditUser user) {
        // Delegates password removal to the service layer,
        // which uses ASP.NET Identity to remove the stored hash.
        bool success = await service.removePassword(user.guid);

        if (!success) {
            ModelState.AddModelError("", "The selected user does not have a password");

            return View("Edit", user);
        }

        ModelState.AddModelError("", "Password deleted");
        return View("Edit", user);
    }
    /// <summary>
    /// Author: Patrick Plechinger
    /// Displays an overview of all registered users.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public IActionResult Overview() {
        // Supplies the user list together with the shared navigation menu.
        ViewData["DropdownOptions"] = navbar_items;

        return View(service.getAll());
    }

    /// <summary>
    /// Loads trainees, mentors, and persisted assignments
    /// for the administrator assignment page.
    /// Author: Dennis Axt
    /// </summary>
    /// <returns>The Mentor-Trainee assignment view.</returns>
    [Authorize(Roles = "Admin")]
    public IActionResult AssignTrainees() {
        ViewData["DropdownOptions"] = navbar_items;

        var users = service.getAll().ToList();

        var mentors = users
            .OfType<Mentor>()
            .OrderBy(mentor => mentor.lastName)
            .ThenBy(mentor => mentor.firstName)
            .ToList();

        var trainees = users
            .OfType<Trainee>()
            .OrderBy(trainee => trainee.lastName)
            .ThenBy(trainee => trainee.firstName)
            .ToList();

        var assignments = service
            .getMentorAssignments()
            .ToList();

        var availableMentors = mentors
            .Select(mentor => new MentorOption {
                Id = mentor.guid,
                Name = $"{mentor.firstName} {mentor.lastName}"
            })
            .ToList();

        var model = trainees
            .Select(trainee => new MentorAssignmentViewModel {
                TraineeId = trainee.guid,
                TraineeName =
                    $"{trainee.firstName} {trainee.lastName}",
                AvailableMentors = availableMentors,
                AssignedMentors = assignments
                    .Where(assignment =>
                        assignment.trainee.guid == trainee.guid)
                    .Select(assignment => new MentorOption {
                        Id = assignment.mentor.guid,
                        Name =
                            $"{assignment.mentor.firstName} " +
                            $"{assignment.mentor.lastName}"
                    })
                    .OrderBy(mentor => mentor.Name)
                    .ToList()
            })
            .ToList();

        return View(model);
    }

    /// <summary>
    /// Persists a Mentor-Trainee assignment submitted by an administrator.
    /// Author: Dennis Axt
    /// </summary>
    /// <param name="mentorId">The identifier of the mentor.</param>
    /// <param name="traineeId">The identifier of the trainee.</param>
    /// <returns>
    /// A redirect to the Mentor-Trainee assignment page.
    /// </returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AssignMentor(Guid mentorId, Guid traineeId) {
        try {
            service.assignTrainee(
                mentorId,
                traineeId);

            TempData["AssignmentMessage"] =
                "The mentor was assigned successfully.";
        }
        catch (KeyNotFoundException exception) {
            TempData["AssignmentError"] = exception.Message;
        }

        return RedirectToAction(nameof(AssignTrainees));
    }

    /// <summary>
    /// Removes a persisted Mentor-Trainee assignment
    /// submitted by an administrator.
    /// Author: Dennis Axt
    /// </summary>
    /// <param name="mentorId">The identifier of the mentor.</param>
    /// <param name="traineeId">The identifier of the trainee.</param>
    /// <returns>
    /// A redirect to the Mentor-Trainee assignment page.
    /// </returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UnassignMentor(Guid mentorId, Guid traineeId) {
        try {
            service.unassignTrainee(
                mentorId,
                traineeId);

            TempData["AssignmentMessage"] =
                "The mentor assignment was removed successfully.";
        }
        catch (KeyNotFoundException exception) {
            TempData["AssignmentError"] = exception.Message;
        }

        return RedirectToAction(nameof(AssignTrainees));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Deactivate(Guid id) {
        service.Deactivate(id);
        return RedirectToAction("Overview");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Activate(Guid id) {
        service.Activate(id);
        return RedirectToAction("Overview");

    }
}
