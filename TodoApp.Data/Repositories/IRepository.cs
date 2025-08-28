using System;
using System.Linq;
using System.Linq.Expressions;

namespace TodoApp.Data.Repositories
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// </summary>
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        T GetById(int id);
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
        T Add(T entity);
        T Update(T entity);
        bool Delete(int id);
        bool Delete(T entity);
        void SaveChanges();
    }
}
