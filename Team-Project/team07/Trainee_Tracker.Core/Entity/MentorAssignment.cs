
namespace Trainee_Tracker.Core.Entity 
{
    public class MentorAssignment
{
    public string MentorId { get; set; } = null!;
    public Mentor mentor { get; set; } = null!;

    public string TraineeId { get; set; } = null!;
    public Trainee trainee { get; set; } = null!;
    public MentorAssignment()
    {
    }

    public MentorAssignment(Trainee trainee, Mentor mentor) {
            this.trainee = trainee;
            this.mentor = mentor;
        }
}
    
}
