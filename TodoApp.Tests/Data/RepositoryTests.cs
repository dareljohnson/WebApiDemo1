using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data.Entity;
using System.Linq;
using TodoApp.Core;
using TodoApp.Data.Repositories;

namespace TodoApp.Tests.Data
{
    /// <summary>
    /// TDD Unit tests for Generic Repository pattern
    /// </summary>
    [TestClass]
    public class RepositoryTests
    {
        private Mock<DbContext> _mockContext;
        private Mock<DbSet<TodoItem>> _mockDbSet;
        private Repository<TodoItem> _repository;

        public RepositoryTests()
        {
            _mockContext = new Mock<DbContext>();
            _mockDbSet = new Mock<DbSet<TodoItem>>();
            _mockContext.Setup(c => c.Set<TodoItem>()).Returns(_mockDbSet.Object);
            // NOTE: Moq cannot mock DbContext.Entry because it is not virtual. For full testability, consider refactoring Repository or use a real DbContext for integration tests.
            _repository = new Repository<TodoItem>(_mockContext.Object);
        }

        [TestMethod]
        public void Repository_Constructor_Should_Throw_When_Context_Is_Null()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new Repository<TodoItem>(null));
        }

        [TestMethod]
        public void GetAll_Should_Return_DbSet()
        {
            // Use real in-memory context for EF integration
            using (var context = new TestTodoContext())
            {
                var repo = new Repository<TodoItem>(context);
                var result = repo.GetAll();
                Assert.IsInstanceOfType(result, typeof(DbSet<TodoItem>));
            }
        }

        [TestMethod]
        public void GetById_Should_Call_DbSet_Find()
        {
            // Arrange
            var id = 1;
            var expectedTodo = new TodoItem { Id = id, Title = "Test" };
            _mockDbSet.Setup(d => d.Find(id)).Returns(expectedTodo);

            // Act
            var result = _repository.GetById(id);

            // Assert
            _mockDbSet.Verify(d => d.Find(id), Times.Once);
            Assert.AreEqual(expectedTodo, result);
        }

        [TestMethod]
        public void Add_Should_Call_DbSet_Add_And_SaveChanges()
        {
            // Arrange
            var todoItem = new TodoItem { Title = "Test Todo" };

            // Act
            var result = _repository.Add(todoItem);
            _repository.SaveChanges();

            // Assert
            _mockDbSet.Verify(d => d.Add(todoItem), Times.Once);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.AreEqual(todoItem, result);
        }

        [TestMethod]
        public void Update_Should_Mark_Entity_Modified_And_SaveChanges()
        {
            // Use real in-memory context for EF metadata
            using (var context = new TestTodoContext())
            {
                // Clean up any existing todos to ensure a clean state
                context.Todos.RemoveRange(context.Todos);
                context.SaveChanges();

                var repo = new Repository<TodoItem>(context);
                var todo = new TodoItem { Title = "Test", CreatedDate = DateTime.Now };
                context.Todos.Add(todo);
                context.SaveChanges();

                // Fetch the tracked entity
                var tracked = context.Todos.FirstOrDefault();
                Assert.IsNotNull(tracked, "Tracked entity is null after SaveChanges");
                tracked.Title = "Changed";
                var result = repo.Update(tracked);
                repo.SaveChanges();

                Assert.AreEqual("Changed", context.Todos.First().Title);
            }
        }

        [TestMethod]
        public void Delete_By_Id_Should_Find_And_Remove_Entity()
        {
            // Use real in-memory context for EF integration
            using (var context = new TestTodoContext())
            {
                // Clean up any existing todos to ensure a clean state
                context.Todos.RemoveRange(context.Todos);
                context.SaveChanges();

                var repo = new Repository<TodoItem>(context);
                var todoItem = new TodoItem { Title = "Test", CreatedDate = DateTime.Now };
                context.Todos.Add(todoItem);
                context.SaveChanges();

                // Use the actual ID assigned by EF
                var id = todoItem.Id;
                var result = repo.Delete(id);
                repo.SaveChanges();

                Assert.IsTrue(result);
                Assert.AreEqual(0, context.Todos.Count());
            }
        }

        [TestMethod]
        public void Delete_By_Id_Should_Return_False_When_Entity_Not_Found()
        {
            using (var context = new TestTodoContext())
            {
                var repository = new Repository<TodoItem>(context);
                // No items added, so ID 999 does not exist
                var result = repository.Delete(999);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void Delete_By_Entity_Should_Remove_Entity_And_SaveChanges()
        {
            // Use real in-memory context for EF integration
            using (var context = new TestTodoContext())
            {
                // Clean up any existing todos to ensure a clean state
                context.Todos.RemoveRange(context.Todos);
                context.SaveChanges();

                var repo = new Repository<TodoItem>(context);
                var todoItem = new TodoItem { Title = "Test", CreatedDate = DateTime.Now };
                context.Todos.Add(todoItem);
                context.SaveChanges();

                // Fetch the first entity from the context to ensure it is tracked
                var tracked = context.Todos.FirstOrDefault();
                Assert.IsNotNull(tracked, "Tracked entity is null after SaveChanges");
                System.Diagnostics.Debug.WriteLine($"Tracked entity state before delete: {context.Entry(tracked).State}");
                var result = repo.Delete(tracked);
                repo.SaveChanges();

                Assert.IsTrue(result, "Delete returned false");
                Assert.AreEqual(0, context.Todos.Count());
            }
        }

        [TestMethod]
        public void SaveChanges_Should_Call_Context_SaveChanges()
        {
            // Act
            _repository.SaveChanges();

            // Assert
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }
    }
}
