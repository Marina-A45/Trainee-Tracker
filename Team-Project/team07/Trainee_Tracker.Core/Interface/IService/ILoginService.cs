using Trainee_Tracker.Core.Entity;
namespace Trainee_Tracker.Core.Service;


    /// <summary>
    /// Author: Patrick Plechinger
    /// </summary>

public interface ILoginService {
    public Task<string> Login(LoginData loginData);

    public Task Logout();
    public Task<User> GetCurrentUser(); 
    public Task<IEnumerable<Trainee>> GetAssignedTrainees();
}