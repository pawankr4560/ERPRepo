
namespace WebApp.Data.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        void Delete(object id);
        IEnumerable<T> GetAll();
        T GetById(object id);
        void Insert(T entity);
        void Save();
        void Update(T entity);
    }
}