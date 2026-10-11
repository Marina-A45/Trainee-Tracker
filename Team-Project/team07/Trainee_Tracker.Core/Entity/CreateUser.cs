using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Trainee_Tracker.Core.Enums;

namespace Trainee_Tracker.Core.Entity;

    public class CreateUser : IValidatableObject {
        [Required(ErrorMessage = "Please enter an email adress")][EmailAddress(ErrorMessage = "Please enter a valid email address.")]public string EMail { get; set; }
        [Required(ErrorMessage = "Please enter a first name")]public string firstName { get; set; }
        [Required(ErrorMessage = "Please enter a last name")]public string lastName { get; set; }
        public string role { get; set; }
        public DateOnly startDate { get; set; } 
        public DateOnly endDate { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Only Trainees have meaningful start/end dates.
        if (role != "Trainee")
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
        public CreateUser( string EMail, string firstName, string lastName, DateOnly startDate, DateOnly endDate, string role)
    {
        this.EMail = EMail;
        this.firstName = firstName;
        this.lastName = lastName;
        this.startDate = startDate;
        this.endDate = endDate;
        this.role = role;
    }
        public CreateUser( string EMail, string firstName, string lastName, string role)
    {
        this.EMail = EMail;
        this.firstName = firstName;
        this.lastName = lastName;
        this.role = role;
    }
    public CreateUser()
    {
        EMail = string.Empty;
        firstName = string.Empty;
        lastName = string.Empty;
        role = string.Empty;
        startDate = DateOnly.FromDateTime(DateTime.Today);
        endDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(6));
    }


    }
