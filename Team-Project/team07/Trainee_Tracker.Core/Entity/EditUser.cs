using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Trainee_Tracker.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace Trainee_Tracker.Core.Entity;

    public class EditUser : IValidatableObject 
    {
        [Required(ErrorMessage = "Please enter an email adress")][EmailAddress(ErrorMessage = "Please enter a valid email address.")]public string Email { get; set; }
        // Unique idetifier of the User.
        public required Guid guid { get; set; }
        [Required(ErrorMessage = "Please enter a first name")]public string firstName { get; set; }
        [Required(ErrorMessage = "Please enter a last name")]public string lastName { get; set; }
        public required Role role { get; set; }
        public required bool active { get; set; }
        public DateOnly startDate { get; set; }
        public DateOnly endDate { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Only Trainees have start/end dates;
        if (role != Role.Trainee)
        {
            yield break;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (endDate <= today)
        {
            yield return new ValidationResult(
                "End date must be after today.",
                new[] { nameof(endDate) });
        }

        if (endDate <= startDate)
        {
            yield return new ValidationResult(
                "End date must be after the start date.",
                new[] { nameof(endDate) });
        }
    }


    [SetsRequiredMembers]
        public EditUser(Guid id, Role role, string EMail, string firstName, string lastName,  DateOnly startDate, DateOnly endDate,bool active)
    {
        this.guid = id;
        this.role = role;
        this.Email = EMail;
        this.firstName = firstName;
        this.lastName = lastName;
        this.active = active;
        this.startDate = startDate;
        this.endDate = endDate;
    }
    
    [SetsRequiredMembers]
        public EditUser(Guid id, Role role, string EMail, string firstName, string lastName, bool active)
    {
        this.guid = id;
        this.role = role;
        this.Email = EMail;
        this.firstName = firstName;
        this.lastName = lastName;
        this.active = active;
    }
    public EditUser()
    {
        this.Email = string.Empty;
        firstName = string.Empty;
        lastName = string.Empty;
        role = Role.Trainee;
        this.active = true;
    }

    }
