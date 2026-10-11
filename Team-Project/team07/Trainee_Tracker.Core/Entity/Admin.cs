
using System.Diagnostics.CodeAnalysis;
using Trainee_Tracker.Core.Enums;

namespace Trainee_Tracker.Core.Entity {
    
    public class Admin : User {
        
    public ICollection<MentorAssignment> Assignments { get; set; }
        = new List<MentorAssignment>();
    public Admin()
    {
        role = Role.Admin;
    }
        [SetsRequiredMembers]
        public Admin(string EMail, string firstName, string lastName, bool active = true) : base(EMail, firstName, lastName, active) {
            this.role = Role.Admin;
        }
    }
    }