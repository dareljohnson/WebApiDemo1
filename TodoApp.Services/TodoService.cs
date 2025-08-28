using System;
using System.Collections.Generic;
using System.Linq;
using TodoApp.Core;
using TodoApp.Data;
using TodoApp.Data.Repositories;
using log4net;

namespace TodoApp.Services
{
    /// <summary>
    /// Business logic service for Todo operations
    /// </summary>
    public interface ITodoService
    {
        IEnumerable<TodoItem> GetAll();
        TodoItem GetById(int id);
        IEnumerable<TodoItem> GetByPriority(TodoPriority priority);
        IEnumerable<TodoItem> GetCompleted();
        IEnumerable<TodoItem> GetPending();
        TodoItem Add(TodoItem item);
        TodoItem Update(TodoItem item);
        bool Delete(int id);
        int GetCompletedCount();
        int GetPendingCount();
    }
    
    /// <summary>
    /// Implementation of Todo business service
    /// </summary>
    public class TodoService : ITodoService
    {
    private readonly ILog _logger;
        // Custom validation delegate
        public Func<TodoItem, bool> CustomValidator { get; set; }

        // Async callback for when a todo is added
        public Func<TodoItem, System.Threading.Tasks.Task> OnTodoAddedAsync { get; set; }

        // Strategy pattern for sorting
        public Func<IEnumerable<TodoItem>, IEnumerable<TodoItem>> SortStrategy { get; set; }
        // Delegates for service layer callbacks
        public Action<TodoItem> OnTodoAdded { get; set; }
        public Action<TodoItem> OnTodoUpdated { get; set; }
        public Action<TodoItem> OnTodoDeleted { get; set; }

        // Event for when a todo is completed
        public event Action<TodoItem> TodoCompleted;
        private readonly ITodoRepository _repository;
        private readonly TodoContext _context;
        
        public TodoService(TodoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repository = new TodoRepository(_context);
            _logger = LogManager.GetLogger(typeof(TodoService));
        }
        
        public TodoService(ITodoRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = LogManager.GetLogger(typeof(TodoService));
        }

        // DI constructor for logger
        public TodoService(ITodoRepository repository, ILog logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? LogManager.GetLogger(typeof(TodoService));
        }
        
        public IEnumerable<TodoItem> GetAll()
        {
            var todos = _repository.GetAll();
            if (SortStrategy != null)
                return SortStrategy(todos).ToList();
            return todos.OrderByDescending(t => t.CreatedDate).ToList();
        }
        
        public TodoItem GetById(int id)
        {
            if (id <= 0) return null;
            return _repository.GetById(id);
        }
        
        public IEnumerable<TodoItem> GetByPriority(TodoPriority priority)
        {
            return _repository.Find(t => t.Priority == priority)
                             .OrderByDescending(t => t.CreatedDate)
                             .ToList();
        }
        
        public IEnumerable<TodoItem> GetCompleted()
        {
            return _repository.Find(t => t.IsCompleted)
                             .OrderByDescending(t => t.CompletedDate)
                             .ToList();
        }
        
        public IEnumerable<TodoItem> GetPending()
        {
            return _repository.Find(t => !t.IsCompleted)
                             .OrderByDescending(t => t.Priority)
                             .ThenByDescending(t => t.CreatedDate)
                             .ToList();
        }
        
        public TodoItem Add(TodoItem item)
        {
            _logger?.Info($"Adding todo: {item?.Title}");
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (CustomValidator != null && !CustomValidator(item))
                throw new ArgumentException("Custom validation failed.");

            if (string.IsNullOrWhiteSpace(item.Title))
                throw new ArgumentException("Title cannot be empty", nameof(item));

            item.CreatedDate = DateTime.Now;
            item.IsCompleted = false;
            item.CompletedDate = null;

            var result = _repository.Add(item);
            _repository.SaveChanges();
            OnTodoAdded?.Invoke(result);
            if (OnTodoAddedAsync != null)
                OnTodoAddedAsync(result).Wait();
            return result;
        }
        // Async version of Add for demonstration
        public async System.Threading.Tasks.Task<TodoItem> AddAsync(TodoItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (CustomValidator != null && !CustomValidator(item))
                throw new ArgumentException("Custom validation failed.");

            if (string.IsNullOrWhiteSpace(item.Title))
                throw new ArgumentException("Title cannot be empty", nameof(item));

            item.CreatedDate = DateTime.Now;
            item.IsCompleted = false;
            item.CompletedDate = null;

            var result = _repository.Add(item);
            _repository.SaveChanges();
            OnTodoAdded?.Invoke(result);
            if (OnTodoAddedAsync != null)
                await OnTodoAddedAsync(result);
            return result;
    }
        
        public TodoItem Update(TodoItem item)
        {
            _logger?.Info($"Updating todo: {item?.Id} - {item?.Title}");
            if (item == null)
                throw new ArgumentNullException(nameof(item));
                
            if (item.Id <= 0)
                throw new ArgumentException("Invalid item ID", nameof(item));
                
            if (string.IsNullOrWhiteSpace(item.Title))
                throw new ArgumentException("Title cannot be empty", nameof(item));
                
            var existingItem = _repository.GetById(item.Id);
            if (existingItem == null)
                throw new InvalidOperationException($"Todo item with ID {item.Id} not found.");
                
            // Update properties
            existingItem.Title = item.Title;
            existingItem.Description = item.Description;
            existingItem.Priority = item.Priority;
            
            // Handle completion status change
            if (!existingItem.IsCompleted && item.IsCompleted)
            {
                existingItem.CompletedDate = DateTime.Now;
            }
            else if (existingItem.IsCompleted && !item.IsCompleted)
            {
                existingItem.CompletedDate = null;
            }
            
            existingItem.IsCompleted = item.IsCompleted;
            
            var result = _repository.Update(existingItem);
            _repository.SaveChanges();
            OnTodoUpdated?.Invoke(result);
            // Notify if completed
            if (result.IsCompleted)
                TodoCompleted?.Invoke(result);
            return result;
        }
        
        public bool Delete(int id)
        {
            _logger?.Info($"Deleting todo: {id}");
            if (id <= 0) return false;
            
            var todo = _repository.GetById(id);
            var success = _repository.Delete(id);
            if (success)
            {
                _repository.SaveChanges();
                if (todo != null)
                    OnTodoDeleted?.Invoke(todo);
            }
            return success;
        }
        
        public int GetCompletedCount()
        {
            return _repository.Find(t => t.IsCompleted).Count();
        }
        
        public int GetPendingCount()
        {
            return _repository.Find(t => !t.IsCompleted).Count();
        }
    }
}
