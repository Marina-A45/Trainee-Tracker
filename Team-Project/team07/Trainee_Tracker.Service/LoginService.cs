using Trainee_Tracker.Core.Entity;
using Trainee_Tracker.Core.Enums;
using Trainee_Tracker.Core.Service;
using Trainee_Tracker.Core.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Linq; // Ensure Linq is included for projections

namespace Trainee_Tracker.Service;

/// <summary>
/// Author: Patrick Plechinger
/// </summary>
public class LoginService : ILoginService {
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IUserRepository _traineeRepository { get; set; }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Initializes the login service with the required Identity services
    /// and repository dependencies.
    /// </summary>
    public LoginService(
        IUserRepository traineeRepository,
        UserManager<User> _userManager,
        SignInManager<User> _signInManager,
        IHttpContextAccessor _httpContextAccessor) {
        _traineeRepository = traineeRepository;
        this._userManager = _userManager;
        this._signInManager = _signInManager;
        this._httpContextAccessor = _httpContextAccessor;
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Logs out the current user
    /// </summary>
    public async Task Logout() {
        await _signInManager.SignOutAsync();
    }

    
    /// <summary>
    /// Author: Patrick Plechinger
    /// Authenticates a user and returns their application role.
    /// </summary>
    public async Task<string> Login(LoginData loginData) {
        // Searches for a registered user with the supplied email address.
        var user = _userManager.Users.FirstOrDefault(u => u.Email == loginData.EMail);

        if (user != null) {
            // Prevents inactive accounts from authenticating.
            if (!user.active) {
                return "Inactive";
            }

            // Delegates credential validation and session creation
            // to the ASP.NET Identity framework.
            var result = await _signInManager.PasswordSignInAsync(
                user,
                loginData.password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded) {
                var role = user.role;

                switch (role) {
                    case Role.Admin:
                        return "Admin";

                    case Role.Mentor:
                        return "Mentor";

                    case Role.Trainee:
                        return "Trainee";

                    default:
                        return "loginError";
                }
            }
        }

        // Invalid credentials or user not found.
        return "loginError";
    }

    /// <summary>
    /// Author: Patrick Plechinger
    /// Returns the currently authenticated user.
    /// </summary>
    public async Task<User?> GetCurrentUser() {
        // Retrieves the authenticated user associated with the current HTTP request.
        var principal = _httpContextAccessor.HttpContext?.User;

        if (principal == null)
            return null;

        return await _userManager.GetUserAsync(principal);
    }
    /// <summary>
    /// Patrick Plechinger
    /// Fetches all Trainees assigned to the currently logged-in Mentor.
    /// </summary>
    public async Task<IEnumerable<Trainee>> GetAssignedTrainees() {
        // 1. Identify who is currently logged in
        var currentUser = await GetCurrentUser();

        // 2. Safeguard: If no user is logged in, or if they aren't a Mentor, return an empty list
        if (currentUser == null || currentUser.role != Role.Mentor) {
            return Enumerable.Empty<Trainee>();
        }

        // 3. Query the user manager, filtering by the current mentor's ID, 
        // and map through the MentorAssignment join table to extract the Trainees.
        var assignedTrainees = _userManager.Users
            .OfType<Mentor>()
            .Where(m => m.Id == currentUser.Id)
            .SelectMany(m => m.Assignments.Select(a => a.trainee))
            .ToList();

        return assignedTrainees;
    }
}