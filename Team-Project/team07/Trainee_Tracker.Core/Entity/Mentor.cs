
using System.Diagnostics.CodeAnalysis;
using Trainee_Tracker.Core.Enums;

namespace Trainee_Tracker.Core.Entity {
    
    public class Mentor : User {
        
    public ICollection<MentorAssignment> Assignments { get; set; }
        = new List<MentorAssignment>();
    public Mentor()
    {
        role = Role.Mentor;
    }
        [SetsRequiredMembers]
        public Mentor(string EMail, string firstName, string lastName, bool active = true) : base(EMail, firstName, lastName, active) {
            this.role = Role.Mentor;
        }
    }
    }