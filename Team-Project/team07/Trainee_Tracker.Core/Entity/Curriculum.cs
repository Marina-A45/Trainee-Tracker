using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Trainee_Tracker.Core.Entity;

public class Curriculum
{
    // Unique idetifier of the Curriculum.
    public Guid id { get; set; } = Guid.NewGuid();

    // Name of the Curriculum.
    [Required]
    public required string name { get; set; }

    // The Lessons contained in the Curriculum
    public virtual ICollection<Lesson> lessons { get; set; } = new List<Lesson>();

    // The Lessons contained in the Curriculum
    public DateTime updated { get; set; }

}