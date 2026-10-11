using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trainee_Tracker.Core.Enums;
namespace Trainee_Tracker.Core.Entity {
    
    public class Trainee : User {
        //Individuall knowledge of the trainee
        public required string knowledge { get; set; }

        //The Curriculum the User should work out
        public Guid? curriculumId { get; set; }
        // The LessonProgresses contained of the User
        public virtual ICollection<LessonProgress> progresses { get; set; } = new List<LessonProgress>();
        public ICollection<MentorAssignment> MentorAssignments { get; set; }
            = new List<MentorAssignment>();
        //The start and end dates
        public DateOnly startDate { get; set; }
        public DateOnly endDate { get; set; }
        public Trainee() {
            this.role = Role.Trainee;
        }
        [SetsRequiredMembers]
        public Trainee(string EMail, string firstName, string lastName, DateOnly startDate, DateOnly endDate, bool active = true) : base(EMail, firstName, lastName, active) {
            knowledge = string.Empty;
            this.startDate = startDate;
            this.endDate = endDate;
            this.role = Role.Trainee;
        }

        public ICollection<TraineeProgressSnapshot> ProgressSnapshots { get; set; }
            = new List<TraineeProgressSnapshot>();
    }
}