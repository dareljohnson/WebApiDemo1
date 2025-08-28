using System.Linq;
using TodoApp.Core;

namespace TodoApp.Data.Repositories
{
    /// <summary>
    /// Specific repository interface for TodoItem entities
    /// </summary>
    public interface ITodoRepository : IRepository<TodoItem>
    {
        // Add any TodoItem-specific repository methods here
        System.Linq.IQueryable<TodoItem> GetByTitle(string title);
        System.Linq.IQueryable<TodoItem> GetByPriority(TodoPriority priority);
        System.Linq.IQueryable<TodoItem> GetCompleted();
        System.Linq.IQueryable<TodoItem> GetPending();
    }

    /// <summary>
    /// Implementation of TodoItem repository
    /// </summary>
    public class TodoRepository : Repository<TodoItem>, ITodoRepository
    {
        public TodoRepository(TodoContext context, log4net.ILog logger = null) : base(context, logger)
        {
        }

        // Add any TodoItem-specific repository implementations here

        public override TodoItem Add(TodoItem entity)
        {
            if (entity == null)
                throw new System.ArgumentNullException(nameof(entity));
            if (entity.CreatedDate == default(System.DateTime))
                entity.CreatedDate = System.DateTime.Now;
            return base.Add(entity);
        }

        public IQueryable<TodoItem> GetByTitle(string title)
        {
            return this.GetAll().Where(t => t.Title != null && t.Title.Contains(title));
        }

        public IQueryable<TodoItem> GetByPriority(TodoPriority priority)
        {
            return this.GetAll().Where(t => t.Priority == priority);
        }

        public IQueryable<TodoItem> GetCompleted()
        {
            return this.GetAll().Where(t => t.IsCompleted);
        }

        public IQueryable<TodoItem> GetPending()
        {
            return this.GetAll().Where(t => !t.IsCompleted);
        }
    }
}
