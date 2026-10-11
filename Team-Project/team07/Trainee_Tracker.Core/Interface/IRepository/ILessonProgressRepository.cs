using Trainee_Tracker.Core.Entity;
namespace Trainee_Tracker.Core.Repository;

public interface ILessonProgressRepository 
{
    public IEnumerable<LessonProgress> GetAll();
    public LessonProgress GetById(Guid id);
    public void Create(LessonProgress entity);
    public void Edit(LessonProgress entity);
    public void Delete(LessonProgress entity);
    public IEnumerable<LessonProgress> GetAll(Guid userId);

}