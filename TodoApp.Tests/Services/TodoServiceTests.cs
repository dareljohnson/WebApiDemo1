using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TodoApp.Core;
using TodoApp.Data.Repositories;
using TodoApp.Services;

namespace TodoApp.Tests.Services
{
    /// <summary>
    /// TDD Unit tests for TodoService business logic
    /// </summary>
    [TestClass]
    public class TodoServiceTests
    {
        private readonly Mock<ITodoRepository> _mockRepository;
        private readonly TodoService _todoService;

        public TodoServiceTests()
        {
            _mockRepository = new Mock<ITodoRepository>();
            _todoService = new TodoService(_mockRepository.Object);
        }

        [TestMethod]
        public void Add_Should_Invoke_OnTodoAdded_Callback()
        {
            // Arrange
            var todo = new TodoItem { Id = 1, Title = "Test" };
            _mockRepository.Setup(r => r.Add(It.IsAny<TodoItem>())).Returns(todo);
            _mockRepository.Setup(r => r.SaveChanges());
            bool callbackInvoked = false;
            _todoService.OnTodoAdded = t => { callbackInvoked = true; };

            // Act
            _todoService.Add(todo);

            // Assert
            Assert.IsTrue(callbackInvoked);
        }

        //[TestMethod]
        //public void TodoService_Constructor_Should_Throw_When_Repository_Is_Null()
        //    {
        //        // Act & Assert
        //        Assert.ThrowsExactly<ArgumentNullException>(() => new TodoService(null));
        //    }

        [TestMethod]
        public void GetAll_Should_Return_All_Todos()
        {
            // Arrange
            var todos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Todo 1" },
                new TodoItem { Id = 2, Title = "Todo 2" }
            }.AsQueryable();

            _mockRepository.Setup(r => r.GetAll()).Returns(todos);

            // Act
            var result = _todoService.GetAll().ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [TestMethod]
        public void GetById_Should_Return_Specific_Todo()
        {
            // Arrange
            var expectedTodo = new TodoItem { Id = 1, Title = "Test Todo" };
            _mockRepository.Setup(r => r.GetById(1)).Returns(expectedTodo);

            // Act
            var result = _todoService.GetById(1);

            // Assert
            Assert.AreEqual(expectedTodo, result);
            _mockRepository.Verify(r => r.GetById(1), Times.Once);
        }

        [TestMethod]
        public void GetByPriority_Should_Return_Todos_With_Specific_Priority()
        {
            // Arrange
            var highPriorityTodos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "High Priority Todo", Priority = TodoPriority.High }
            }.AsQueryable();


            // Mock Find for predicate t => t.Priority == TodoPriority.High
            _mockRepository.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<System.Func<TodoItem, bool>> predicate) =>
                    highPriorityTodos.Where(predicate.Compile()).AsQueryable());

            // Act
            var result = _todoService.GetByPriority(TodoPriority.High).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(TodoPriority.High, result.First().Priority);
            _mockRepository.Verify(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>()), Times.Once);
        }

        [TestMethod]
        public void GetCompleted_Should_Return_Only_Completed_Todos()
        {
            // Arrange
            var completedTodos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Completed Todo", IsCompleted = true }
            }.AsQueryable();


            // Mock Find for predicate t => t.IsCompleted
            _mockRepository.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<System.Func<TodoItem, bool>> predicate) =>
                    completedTodos.Where(predicate.Compile()).AsQueryable());

            // Act
            var result = _todoService.GetCompleted().ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.First().IsCompleted);
            _mockRepository.Verify(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>()), Times.Once);
        }

        [TestMethod]
        public void GetPending_Should_Return_Only_Pending_Todos()
        {
            // Arrange
            var pendingTodos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Pending Todo", IsCompleted = false }
            }.AsQueryable();

            // Mock Find for predicate t => !t.IsCompleted
            _mockRepository.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<System.Func<TodoItem, bool>> predicate) =>
                    pendingTodos.Where(predicate.Compile()).AsQueryable());

            // Act
            var result = _todoService.GetPending().ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsFalse(result.First().IsCompleted);
            _mockRepository.Verify(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>()), Times.Once);
        }

        [TestMethod]
        public void Add_Should_Validate_And_Add_Todo()
        {
            // Arrange
            var todoItem = new TodoItem { Title = "New Todo", Description = "Description" };
            _mockRepository.Setup(r => r.Add(It.IsAny<TodoItem>())).Returns(todoItem);

            // Act
            var result = _todoService.Add(todoItem);

            // Assert
            Assert.AreEqual(todoItem, result);
            _mockRepository.Verify(r => r.Add(todoItem), Times.Once);
        }

        [TestMethod]
        public void Add_Should_Throw_When_Todo_Is_Null()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => _todoService.Add(null));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("   ")]
        public void Add_Should_Throw_When_Title_Is_Invalid(string invalidTitle)
        {
            // Arrange
            var todoItem = new TodoItem { Title = invalidTitle };

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => _todoService.Add(todoItem));
        }

        [TestMethod]
        public void Update_Should_Update_Existing_Todo()
        {
            // Arrange
            var existingTodo = new TodoItem { Id = 1, Title = "Original Title" };
            var updatedTodo = new TodoItem { Id = 1, Title = "Updated Title" };

            _mockRepository.Setup(r => r.GetById(1)).Returns(existingTodo);
            _mockRepository.Setup(r => r.Update(It.IsAny<TodoItem>())).Returns(updatedTodo);

            // Act
            var result = _todoService.Update(updatedTodo);

            // Assert
            Assert.AreEqual(updatedTodo, result);
            _mockRepository.Verify(r => r.GetById(1), Times.Once);
            _mockRepository.Verify(r => r.Update(It.Is<TodoItem>(t => t.Id == 1 && t.Title == "Updated Title")), Times.Once);
        }

        [TestMethod]
        public void Update_Should_Throw_When_Todo_Not_Found()
        {
            // Arrange
            var todoItem = new TodoItem { Id = 999, Title = "Non-existent" };
            _mockRepository.Setup(r => r.GetById(999)).Returns((TodoItem)null);

            // Act & Assert
            Assert.ThrowsExactly<InvalidOperationException>(() => _todoService.Update(todoItem));
        }

        [TestMethod]
        public void Delete_Should_Remove_Existing_Todo()
        {
            // Arrange
            _mockRepository.Setup(r => r.Delete(1)).Returns(true);

            // Act
            var result = _todoService.Delete(1);

            // Assert
            Assert.IsTrue(result);
            _mockRepository.Verify(r => r.Delete(1), Times.Once);
        }

        [TestMethod]
        public void Delete_Should_Return_False_When_Todo_Not_Found()
        {
            // Arrange
            _mockRepository.Setup(r => r.Delete(999)).Returns(false);

            // Act
            var result = _todoService.Delete(999);

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(r => r.Delete(999), Times.Once);
        }

        [TestMethod]
        public void GetCompletedCount_Should_Return_Count_Of_Completed_Todos()
        {
            // Arrange
            var completedTodos = new List<TodoItem>
            {
                new TodoItem { IsCompleted = true },
                new TodoItem { IsCompleted = true }
            }.AsQueryable();


            // Mock Find for predicate t => t.IsCompleted
            _mockRepository.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<System.Func<TodoItem, bool>> predicate) =>
                    completedTodos.Where(predicate.Compile()).AsQueryable());

            // Act
            var count = _todoService.GetCompletedCount();

            // Assert
            Assert.AreEqual(2, count);
            _mockRepository.Verify(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>()), Times.Once);
        }

        [TestMethod]
        public void GetPendingCount_Should_Return_Count_Of_Pending_Todos()
        {
            // Arrange
            var pendingTodos = new List<TodoItem>
            {
                new TodoItem { IsCompleted = false },
                new TodoItem { IsCompleted = false },
                new TodoItem { IsCompleted = false }
            }.AsQueryable();

            _mockRepository.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>())).Returns(pendingTodos);

            // Act
            var count = _todoService.GetPendingCount();

            // Assert
            Assert.AreEqual(3, count);
            _mockRepository.Verify(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<TodoItem, bool>>>()), Times.Once);
        }
    }
}
