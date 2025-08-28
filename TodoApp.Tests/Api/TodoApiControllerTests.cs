using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using TodoApp.Api.Controllers;
using TodoApp.Api.Models;
using TodoApp.Core;
using TodoApp.Services;

namespace TodoApp.Tests.Api
{
    /// <summary>
    /// TDD Unit tests for TodoApiController
    /// </summary>
    [TestClass]
    public class TodoApiControllerTests
    {
        private readonly Mock<ITodoService> _mockTodoService;
        private readonly TodoApiController _controller;

        public TodoApiControllerTests()
        {
            _mockTodoService = new Mock<ITodoService>();
            var mockLogger = new Mock<ILog>().Object;
            _controller = new TodoApiController(_mockTodoService.Object, mockLogger);
        }

        [TestMethod]
        public void Constructor_Should_Throw_When_Service_Is_Null()
        {
            // Act & Assert
            var mockLogger = new Mock<ILog>().Object;
            Assert.ThrowsExactly<ArgumentNullException>(() => new TodoApiController(null, mockLogger));
        }

        [TestMethod]
        public void GetAll_Should_Return_All_Todos()
        {
            // Arrange
            var todos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Todo 1", CreatedDate = DateTime.Now },
                new TodoItem { Id = 2, Title = "Todo 2", CreatedDate = DateTime.Now }
            };
            _mockTodoService.Setup(s => s.GetAll()).Returns(todos);

            // Act
            var result = _controller.GetAll();

            // Assert
            var okResult = result as OkNegotiatedContentResult<List<TodoResponse>>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Content.Count);
            _mockTodoService.Verify(s => s.GetAll(), Times.Once);
        }

        [TestMethod]
        public void GetById_Should_Return_Specific_Todo()
        {
            // Arrange
            var todo = new TodoItem { Id = 1, Title = "Test Todo", CreatedDate = DateTime.Now };
            _mockTodoService.Setup(s => s.GetById(1)).Returns(todo);

            // Act
            var result = _controller.GetById(1);

            // Assert
            var okResult = result as OkNegotiatedContentResult<TodoResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Content.Id);
            Assert.AreEqual("Test Todo", okResult.Content.Title);
            _mockTodoService.Verify(s => s.GetById(1), Times.Once);
        }

        [TestMethod]
        public void GetById_Should_Return_NotFound_When_Todo_Not_Exists()
        {
            // Arrange
            _mockTodoService.Setup(s => s.GetById(999)).Returns((TodoItem)null);

            // Act
            var result = _controller.GetById(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockTodoService.Verify(s => s.GetById(999), Times.Once);
        }

        [TestMethod]
        public void Post_Should_Create_New_Todo()
        {
            // Arrange
            var request = new CreateTodoRequest
            {
                Title = "New Todo",
                Description = "Description",
                Priority = TodoPriority.Medium
            };

            var createdTodo = new TodoItem
            {
                Id = 1,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                CreatedDate = DateTime.Now
            };

            _mockTodoService.Setup(s => s.Add(It.IsAny<TodoItem>())).Returns(createdTodo);

            // Act
            var result = _controller.Create(request);

            // Assert
            var createdResult = result as CreatedNegotiatedContentResult<TodoResponse>;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual("New Todo", createdResult.Content.Title);
            Assert.AreEqual(TodoPriority.Medium, createdResult.Content.Priority);
            _mockTodoService.Verify(s => s.Add(It.IsAny<TodoItem>()), Times.Once);
        }

        [TestMethod]
        public void Post_Should_Return_BadRequest_When_Request_Is_Null()
        {
            // Act
            var result = _controller.Create(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Post_Should_Return_BadRequest_When_ModelState_Invalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Title is required");
            var request = new CreateTodoRequest();

            // Act
            var result = _controller.Create(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public void Put_Should_Update_Existing_Todo()
        {
            // Arrange
            var request = new UpdateTodoRequest
            {
                Title = "Updated Todo",
                Description = "Updated Description",
                Priority = TodoPriority.High,
                Status = TodoStatus.Completed
            };

            var existingTodo = new TodoItem { Id = 1, Title = "Original Todo", CreatedDate = DateTime.Now };
            var updatedTodo = new TodoItem
            {
                Id = 1,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                IsCompleted = true,
                CreatedDate = DateTime.Now
            };

            _mockTodoService.Setup(s => s.GetById(1)).Returns(existingTodo);
            _mockTodoService.Setup(s => s.Update(It.IsAny<TodoItem>())).Returns(updatedTodo);

            // Act
            var result = _controller.Update(1, request);

            // Assert
            var okResult = result as OkNegotiatedContentResult<TodoResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Updated Todo", okResult.Content.Title);
            Assert.AreEqual(TodoPriority.High, okResult.Content.Priority);
            _mockTodoService.Verify(s => s.GetById(1), Times.Once);
            _mockTodoService.Verify(s => s.Update(It.IsAny<TodoItem>()), Times.Once);
        }

        [TestMethod]
        public void Put_Should_Return_NotFound_When_Todo_Not_Exists()
        {
            // Arrange
            var request = new UpdateTodoRequest { Title = "Updated Todo" };
            _mockTodoService.Setup(s => s.GetById(999)).Returns((TodoItem)null);

            // Act
            var result = _controller.Update(999, request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockTodoService.Verify(s => s.GetById(999), Times.Once);
        }

        [TestMethod]
        public void Delete_Should_Remove_Existing_Todo()
        {
            // Arrange
            _mockTodoService.Setup(s => s.Delete(1)).Returns(true);

            // Act
            var result = _controller.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            _mockTodoService.Verify(s => s.Delete(1), Times.Once);
        }

        [TestMethod]
        public void Delete_Should_Return_NotFound_When_Todo_Not_Exists()
        {
            // Arrange
            _mockTodoService.Setup(s => s.Delete(999)).Returns(false);

            // Act
            var result = _controller.Delete(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockTodoService.Verify(s => s.Delete(999), Times.Once);
        }

        [TestMethod]
        public void GetByPriority_Should_Return_Todos_With_Specific_Priority()
        {
            // Arrange
            var todos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "High Priority Todo", Priority = TodoPriority.High, CreatedDate = DateTime.Now }
            };
            _mockTodoService.Setup(s => s.GetByPriority(TodoPriority.High)).Returns(todos);

            // Act
            var result = _controller.GetByPriority(3);

            // Assert
            var okResult = result as OkNegotiatedContentResult<List<TodoResponse>>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Content.Count);
            Assert.AreEqual(TodoPriority.High, okResult.Content.First().Priority);
            _mockTodoService.Verify(s => s.GetByPriority(TodoPriority.High), Times.Once);
        }

        [TestMethod]
        public void GetCompleted_Should_Return_Only_Completed_Todos()
        {
            // Arrange
            var completedTodos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Completed Todo", IsCompleted = true, CreatedDate = DateTime.Now }
            };
            _mockTodoService.Setup(s => s.GetCompleted()).Returns(completedTodos);

            // Act
            var result = _controller.GetCompleted();

            // Assert
            var okResult = result as OkNegotiatedContentResult<List<TodoResponse>>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Content.Count);
            Assert.IsTrue(okResult.Content.First().IsCompleted);
            _mockTodoService.Verify(s => s.GetCompleted(), Times.Once);
        }

        [TestMethod]
        public void GetPending_Should_Return_Only_Pending_Todos()
        {
            // Arrange
            var pendingTodos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Pending Todo", IsCompleted = false, CreatedDate = DateTime.Now }
            };
            _mockTodoService.Setup(s => s.GetPending()).Returns(pendingTodos);

            // Act
            var result = _controller.GetPending();

            // Assert
            var okResult = result as OkNegotiatedContentResult<List<TodoResponse>>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Content.Count);
            Assert.IsFalse(okResult.Content.First().IsCompleted);
            _mockTodoService.Verify(s => s.GetPending(), Times.Once);
        }

        [TestMethod]
        public void GetStats_Should_Return_Todo_Statistics()
        {
            // Arrange
            _mockTodoService.Setup(s => s.GetCompletedCount()).Returns(5);
            _mockTodoService.Setup(s => s.GetPendingCount()).Returns(3);
            _mockTodoService.Setup(s => s.GetAll()).Returns(new List<TodoItem>());

            // Act
            var result = _controller.GetStats();

            // Debug: log the actual result type
            System.Diagnostics.Debug.WriteLine($"Result type: {result?.GetType().Name}");

            // Assert
            if (result is InternalServerErrorResult)
            {
                Assert.Fail("Controller returned InternalServerErrorResult. Check service mocks and controller logic.");
            }
            // Accept OkNegotiatedContentResult<T> for any T (anonymous type)
            if (result == null)
                Assert.Fail("Result is null");
            var resultType = result.GetType();
            if (!resultType.IsGenericType || resultType.GetGenericTypeDefinition().Name != "OkNegotiatedContentResult`1")
                Assert.Fail($"Expected OkNegotiatedContentResult<T> but got {resultType.Name}");
            Assert.IsNotNull(result, $"Expected OkNegotiatedContentResult<T> but got {resultType.Name}");
            var contentProp = resultType.GetProperty("Content");
            Assert.IsNotNull(contentProp, "Result does not have a Content property");
            var stats = contentProp.GetValue(result);
            _mockTodoService.Verify(s => s.GetCompletedCount(), Times.Once);
            _mockTodoService.Verify(s => s.GetPendingCount(), Times.Once);
        }
    }
}
