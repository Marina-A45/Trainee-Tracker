using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Trainee_Tracker.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace Trainee_Tracker.Core.Entity;

    public class User : IdentityUser
    {
        // Unique idetifier of the User.
        public required Guid guid { get; set; }
        [Required(ErrorMessage = "Please enter a first name")]public string firstName { get; set; }
        [Required(ErrorMessage = "Please enter a last name")]public string lastName { get; set; }
        public required Role role { get; set; }
        public required bool active { get; set; }


    [SetsRequiredMembers]
        public User(string EMail, string firstName, string lastName,  bool active = true)
    {
        this.guid = Guid.NewGuid();
        this.Email = EMail;
        this.UserName = EMail;
        this.firstName = firstName;
        this.lastName = lastName;
        this.active = active;
    }
    public User()
    {
        this.guid = Guid.NewGuid();
        this.Email = string.Empty;
        this.UserName = string.Empty;
        firstName = string.Empty;
        lastName = string.Empty;
        role = Role.Trainee;
        this.active = true;
    }

}
