using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Trainee_Tracker.Core.Entity;

public class LoginData 
{
    [Required(ErrorMessage = "Please enter an email.")][JsonPropertyName("EMail")] public String EMail { get; set; }
    [Required(ErrorMessage = "Please enter a password.")][JsonPropertyName("Password")] public string password { get; set; }


    public LoginData(String EMail, string password)
    {
        this.EMail = EMail;
        this.password = password;
    }
    public LoginData()
    {
        EMail = string.Empty;
        password = string.Empty;
    }
 
}
