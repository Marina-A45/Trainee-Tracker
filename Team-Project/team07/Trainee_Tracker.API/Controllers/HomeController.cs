using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using TraineeTracker.Models;
using Trainee_Tracker.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Trainee_Tracker.Service;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TraineeTracker.Controllers;

public class HomeController : Controller {

    private readonly ILoginService service;
    private readonly IUserService userService;
    /// <summary>
    /// Author: Patrick Plechinger
    /// Initializes the home controller with the required login and user services.
    /// </summary>
    public HomeController(ILoginService service, IUserService userService) {
        this.service = service;
        this.userService = userService;
    }
    // List of links to be displayed in the Navbar
    private List<SelectListItem> navbar_items = new List<SelectListItem> {
    };

    public IActionResult Index() {
        // Navbar items
        ViewData["DropdownOptions"] = navbar_items;
        return View("Login");
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Displays the login page.
    /// </summary>
    public IActionResult Login() {
        ViewData["DropdownOptions"] = navbar_items;

        return View();
    }

    public async Task<IActionResult> Logout() {
        await service.Logout();
        return RedirectToAction("Login");
    }


    /// <summary>
    /// Author: Patrick Plechinger
    /// Displays the password creation page for the selected user.
    /// </summary>
    public IActionResult SetPassword(Guid id) {
        ViewData["DropdownOptions"] = navbar_items;

        // Creates the model containing the user identifier that will
        // be submitted together with the new password.
        var model = new SetPassword { guid = id };

        model.guid = id;

        if (model == null) {
            ModelState.AddModelError("", "The selected user does not exist.");
            return View("Login");
        }

        return View(model);
    }
    /// <summary>
    /// Author: Patrick Plechinger
    /// Validates and stores the initial password for a user.
    /// </summary>
    public async Task<IActionResult> SetPasswordData(SetPassword setPassword) {
        // Verifies that all submitted password data satisfies
        // the validation rules before continuing.
        if (!ModelState.IsValid) {
            ModelState.AddModelError("", "Add Passwords");
            return View("SetPassword");
        }
        // Delegates password creation to the service layer, which
        // performs password validation and integrates with ASP.NET Identity.
        string result = await userService.setPassword(setPassword);
        // Handles the different outcomes returned by the service
        // and provides appropriate feedback to the user.
        switch (result) {
            case "success":
                return View("Login", null);
            case "inexistent":
                ModelState.AddModelError("", "The User you tried to add a Password for does not exist");
                return View("Login", null);
            case "unequal":
                ModelState.AddModelError("", "The Passwords have to be identical");
                return View("SetPassword", setPassword);
            case "incorrect":
                ModelState.AddModelError("", "Error setting Password");
                return View("SetPassword", setPassword);
            case "late":
                ModelState.AddModelError("", "The User already has a Password");
                return View("SetPassword", setPassword);
            default:
                ModelState.AddModelError("", "Unexpected error at setting password");
                return View("Login", null);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Authenticates the user and redirects them according to their role.
    /// </summary>
    public async Task<IActionResult> LoginUser(LoginData model) {
        // Delegates authentication to the login service, which
        // validates the supplied credentials using ASP.NET Identity.
        var login = await service.Login(model);

        // Redirects authenticated users to the appropriate
        // area of the application based on their role.
        switch (login) {
            case "Admin":
                return RedirectToAction("Overview", "Admin");

            case "Mentor":
                return RedirectToAction("Curriculums", "Mentor");

            case "Trainee":
                return RedirectToAction("Overview", "Trainee");

            case "Inactive":
                // Prevents inactive accounts from accessing the application.
                ModelState.AddModelError("", "Account is inactive");
                return View("Login");

            default:
                // Handles invalid login credentials.
                ModelState.AddModelError("", "Invalid email or password");
                return View("Login");
        }
    }
}

