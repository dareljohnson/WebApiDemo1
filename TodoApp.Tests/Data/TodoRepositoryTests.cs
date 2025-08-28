using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TodoApp.Core;
using TodoApp.Data;
using TodoApp.Data.Repositories;

namespace TodoApp.Tests.Data
{
    /// <summary>
    /// TDD Integration tests for TodoRepository with in-memory database
    /// </summary>
    [TestClass]
    public class TodoRepositoryTests : IDisposable
    {
        private readonly TodoContext _context;
        private readonly TodoRepository _repository;

        public TodoRepositoryTests()
        {
            // Use in-memory database for testing
            _context = new TodoContext();
            _context.Database.CreateIfNotExists();
            _repository = new TodoRepository(_context);

            // Clean database before each test
            _context.TodoItems.RemoveRange(_context.TodoItems);
            _context.SaveChanges();
        }

        [TestMethod]
        public void TodoRepository_Should_Inherit_From_Repository()
        {
            // Assert
            Assert.IsInstanceOfType(_repository, typeof(Repository<TodoItem>));
        }

        [TestMethod]
        public void GetByPriority_Should_Return_Todos_With_Specific_Priority()
        {
            // Arrange
            var highPriorityTodo = new TodoItem { Title = "High Priority", Priority = TodoPriority.High, CreatedDate = DateTime.Now };
            var lowPriorityTodo = new TodoItem { Title = "Low Priority", Priority = TodoPriority.Low, CreatedDate = DateTime.Now };
            var mediumPriorityTodo = new TodoItem { Title = "Medium Priority", Priority = TodoPriority.Medium, CreatedDate = DateTime.Now };

            _repository.Add(highPriorityTodo);
            _repository.Add(lowPriorityTodo);
            _repository.Add(mediumPriorityTodo);
            _context.SaveChanges();

            // Act
            var highPriorityResults = _repository.GetByPriority(TodoPriority.High).ToList();
            var lowPriorityResults = _repository.GetByPriority(TodoPriority.Low).ToList();

            // Assert
            Assert.AreEqual(1, highPriorityResults.Count);
            Assert.AreEqual("High Priority", highPriorityResults.First().Title);
            Assert.AreEqual(1, lowPriorityResults.Count);
            Assert.AreEqual("Low Priority", lowPriorityResults.First().Title);
        }

        [TestMethod]
        public void GetCompleted_Should_Return_Only_Completed_Todos()
        {
            // Arrange
            var completedTodo = new TodoItem
            {
                Title = "Completed Todo",
                IsCompleted = true,
                CompletedDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            var pendingTodo = new TodoItem
            {
                Title = "Pending Todo",
                IsCompleted = false,
                CreatedDate = DateTime.Now
            };

            _repository.Add(completedTodo);
            _repository.Add(pendingTodo);
            _context.SaveChanges();

            // Act
            var completedResults = _repository.GetCompleted().ToList();

            // Assert
            Assert.AreEqual(1, completedResults.Count);
            Assert.AreEqual("Completed Todo", completedResults.First().Title);
            Assert.IsTrue(completedResults.First().IsCompleted);
        }

        [TestMethod]
        public void GetPending_Should_Return_Only_Pending_Todos()
        {
            // Arrange
            var completedTodo = new TodoItem
            {
                Title = "Completed Todo",
                IsCompleted = true,
                CompletedDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            var pendingTodo = new TodoItem
            {
                Title = "Pending Todo",
                IsCompleted = false,
                CreatedDate = DateTime.Now
            };

            _repository.Add(completedTodo);
            _repository.Add(pendingTodo);
            _context.SaveChanges();

            // Act
            var pendingResults = _repository.GetPending().ToList();

            // Assert
            Assert.AreEqual(1, pendingResults.Count);
            Assert.AreEqual("Pending Todo", pendingResults.First().Title);
            Assert.IsFalse(pendingResults.First().IsCompleted);
        }

        [TestMethod]
        public void GetByTitle_Should_Return_Todos_Matching_Title()
        {
            // Arrange
            var todo1 = new TodoItem { Title = "Buy groceries", CreatedDate = DateTime.Now };
            var todo2 = new TodoItem { Title = "Buy coffee", CreatedDate = DateTime.Now };
            var todo3 = new TodoItem { Title = "Sell items", CreatedDate = DateTime.Now };

            _repository.Add(todo1);
            _repository.Add(todo2);
            _repository.Add(todo3);
            _context.SaveChanges();

            // Act
            var buyResults = _repository.GetByTitle("Buy").ToList();

            // Assert
            Assert.AreEqual(2, buyResults.Count);
            foreach (var todo in buyResults) Assert.IsTrue(todo.Title.Contains("Buy"));
        }

        [TestMethod]
        public void Add_Should_Set_CreatedDate()
        {
            // Arrange
            var beforeAdd = DateTime.Now;
            var todoItem = new TodoItem { Title = "Test Todo" };

            // Act
            var result = _repository.Add(todoItem);
            var afterAdd = DateTime.Now;

            // Assert
            Assert.IsTrue(result.CreatedDate >= beforeAdd && result.CreatedDate <= afterAdd);
        }

        [TestMethod]
        public void Repository_Should_Handle_Null_Searches_Gracefully()
        {
            // Arrange
            var todo = new TodoItem { Title = "Test Todo", CreatedDate = DateTime.Now };
            _repository.Add(todo);
            _context.SaveChanges();

            // Act & Assert - Should not throw
            var nullTitleResults = _repository.GetByTitle(null).ToList();
            Assert.IsEmpty(nullTitleResults);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
