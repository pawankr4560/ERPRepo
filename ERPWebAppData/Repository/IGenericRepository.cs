
namespace WebApp.Data.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task DeleteAsync(object id);
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(object id);
        Task InsertAsync(T entity);
        Task SaveAsync();
        Task UpdateAsync(T entity);
    }
}
