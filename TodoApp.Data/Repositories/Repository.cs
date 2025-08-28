using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using log4net;

namespace TodoApp.Data.Repositories
{
    /// <summary>
    /// Generic repository implementation for Entity Framework
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ILog _logger;
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context, ILog logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
            _logger = logger ?? log4net.LogManager.GetLogger(typeof(Repository<T>));
        }

        public virtual IQueryable<T> GetAll()
        {
            _logger?.Info($"Getting all {typeof(T).Name} entities");
            return _dbSet;
        }

        public virtual T GetById(int id)
        {
            _logger?.Info($"Getting {typeof(T).Name} by id: {id}");
            return _dbSet.Find(id);
        }

        public virtual IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            _logger?.Info($"Finding {typeof(T).Name} with predicate: {predicate}");
            return _dbSet.Where(predicate);
        }

        public virtual T Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _logger?.Info($"Adding {typeof(T).Name}: {entity}");
            _dbSet.Add(entity);
            return entity;
        }

        public virtual T Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _logger?.Info($"Updating {typeof(T).Name}: {entity}");
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public virtual bool Delete(int id)
        {
            _logger?.Info($"Deleting {typeof(T).Name} by id: {id}");
            var entity = GetById(id);
            if (entity == null)
                return false;
            return Delete(entity);
        }

        public virtual bool Delete(T entity)
        {
            if (entity == null)
                return false;
            _logger?.Info($"Deleting {typeof(T).Name}: {entity}");
            if (_context.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);
            _dbSet.Remove(entity);
            return true;
        }

        public virtual void SaveChanges()
        {
            _logger?.Info($"Saving changes for {typeof(T).Name}");
            _context.SaveChanges();
        }
    }
}
