using Trainee_Tracker.Core.Entity;
namespace Trainee_Tracker.Core.Repository;

public interface ICurriculumRepository : IRepository<Curriculum>
{
    public void Edit(Guid curriculumId, List<Lesson> lessons);
}