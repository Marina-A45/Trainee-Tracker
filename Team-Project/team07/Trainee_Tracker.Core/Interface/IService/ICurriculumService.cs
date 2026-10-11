using Trainee_Tracker.Core.Entity;
namespace Trainee_Tracker.Core.Service;


public interface ICurriculumService
{
    public void create(Curriculum curriculum);
    public IEnumerable<Curriculum> getAll();
    public Task<Curriculum> CreateWithJson(string name, Stream jsonStream);
    public Task<Boolean> EditWithJson(Guid curriculumId, Stream jsonStream);
    public Task<Boolean> Delete(Guid id);

}