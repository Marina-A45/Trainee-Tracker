namespace Trainee_Tracker.Core.Repository;

public interface IRepository<T> where T : class
{
    public IEnumerable<T> GetAll();
    public T GetById(Guid id);
    public bool Exists(Guid id);
    public bool Exists(T entity);
    public void Create(T entity);
    public void Edit(T entity);
    public void Delete(T entity);
    public void DeleteById(Guid id);
}