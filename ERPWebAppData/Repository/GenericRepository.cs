using Microsoft.EntityFrameworkCore;

namespace WebApp.Data.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly WebAppDbContext _context;
        private readonly DbSet<T> _table;

        public GenericRepository(WebAppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _table = _context.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return _table.ToList();
        }

        public T GetById(object id)
        {
            return _table.Find(id);
        }

        public void Insert(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _table.Add(entity);
        }

        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _table.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(object id)
        {
            T entity = _table.Find(id);
            if (entity == null) throw new KeyNotFoundException("Entity not found");
            _table.Remove(entity);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

