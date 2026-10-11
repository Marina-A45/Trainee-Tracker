using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Trainee_Tracker.Core.Entity;
public class SetPassword 
{
    [Required]public Guid guid { get; set; }
    [Required(ErrorMessage = "Please enter a password.")][JsonPropertyName("Password")] public string password { get; set; }
    [Required(ErrorMessage = "Please enter a password.")][JsonPropertyName("Password")] public string passwordRepeat { get; set; }


    public SetPassword(Guid guid, string password, string passwordRepeat)
    {
        this.guid = guid;
        this.password = password;
        this.passwordRepeat = passwordRepeat;
    }
    public SetPassword()
    {
        guid = Guid.Empty;
        password = string.Empty;
        passwordRepeat = string.Empty;
    }
 
}
