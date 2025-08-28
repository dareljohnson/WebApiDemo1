using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TodoApp.Api.Mappers;
using TodoApp.Api.Models;
using TodoApp.Core;

namespace TodoApp.Tests.Api
{
    /// <summary>
    /// TDD Unit tests for TodoMapper DTO conversions
    /// </summary>
    [TestClass]
    public class TodoMapperTests
    {
        [TestMethod]
        public void ToResponse_Should_Map_TodoItem_To_TodoResponse()
        {
            // Arrange
            var createdDate = DateTime.Now;
            var completedDate = DateTime.Now.AddHours(1);
            var todoItem = new TodoItem
            {
                Id = 1,
                Title = "Test Todo",
                Description = "Test Description",
                Priority = TodoPriority.High,
                IsCompleted = true,
                CreatedDate = createdDate,
                CompletedDate = completedDate
            };

            // Act
            var result = todoItem.ToResponse();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Test Todo", result.Title);
            Assert.AreEqual("Test Description", result.Description);
            Assert.AreEqual(TodoPriority.High, result.Priority);
            Assert.AreEqual(TodoStatus.Completed, result.Status);
            Assert.AreEqual(createdDate, result.CreatedAt);
            Assert.AreEqual(createdDate, result.UpdatedAt); // Same as created in current model
            Assert.IsTrue(result.IsCompleted);
            Assert.AreEqual(completedDate, result.CompletedAt);
        }

        [TestMethod]
        public void ToResponse_Should_Return_Null_When_TodoItem_Is_Null()
        {
            // Arrange
            TodoItem todoItem = null;

            // Act
            var result = todoItem.ToResponse();

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ToResponse_Should_Map_Pending_Status_For_Incomplete_Todo()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Id = 1,
                Title = "Pending Todo",
                IsCompleted = false,
                CreatedDate = DateTime.Now
            };

            // Act
            var result = todoItem.ToResponse();

            // Assert
            Assert.AreEqual(TodoStatus.Pending, result.Status);
            Assert.IsFalse(result.IsCompleted);
            Assert.IsNull(result.CompletedAt);
        }

        [TestMethod]
        public void ToEntity_Should_Map_CreateTodoRequest_To_TodoItem()
        {
            // Arrange
            var dueDate = DateTime.Now.AddDays(7);
            var request = new CreateTodoRequest
            {
                Title = "New Todo",
                Description = "New Description",
                Priority = TodoPriority.Medium,
                DueDate = dueDate
            };

            // Act
            var result = request.ToEntity();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("New Todo", result.Title);
            Assert.AreEqual("New Description", result.Description);
            Assert.AreEqual(TodoPriority.Medium, result.Priority);
            Assert.IsFalse(result.IsCompleted);
            Assert.IsTrue(result.CreatedDate > DateTime.MinValue);
        }

        [TestMethod]
        public void ToEntity_Should_Return_Null_When_Request_Is_Null()
        {
            // Arrange
            CreateTodoRequest request = null;

            // Act
            var result = request.ToEntity();

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ToEntity_Should_Trim_Title_And_Description()
        {
            // Arrange
            var request = new CreateTodoRequest
            {
                Title = "  Trimmed Title  ",
                Description = "  Trimmed Description  ",
                Priority = TodoPriority.Low
            };

            // Act
            var result = request.ToEntity();

            // Assert
            Assert.AreEqual("Trimmed Title", result.Title);
            Assert.AreEqual("Trimmed Description", result.Description);
        }

        [TestMethod]
        public void UpdateFrom_Should_Update_TodoItem_From_UpdateRequest()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Id = 1,
                Title = "Original Title",
                Description = "Original Description",
                Priority = TodoPriority.Low,
                IsCompleted = false,
                CreatedDate = DateTime.Now.AddDays(-1)
            };

            var updateRequest = new UpdateTodoRequest
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Priority = TodoPriority.High,
                Status = TodoStatus.Completed
            };

            // Act
            todoItem.UpdateFrom(updateRequest);

            // Assert
            Assert.AreEqual("Updated Title", todoItem.Title);
            Assert.AreEqual("Updated Description", todoItem.Description);
            Assert.AreEqual(TodoPriority.High, todoItem.Priority);
            Assert.IsTrue(todoItem.IsCompleted);
            Assert.IsNotNull(todoItem.CompletedDate);
        }

        [TestMethod]
        public void UpdateFrom_Should_Handle_Completion_Status_Change()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Id = 1,
                Title = "Test Todo",
                IsCompleted = false,
                CompletedDate = null,
                CreatedDate = DateTime.Now
            };

            var updateRequest = new UpdateTodoRequest
            {
                Title = "Test Todo",
                Status = TodoStatus.Completed
            };

            var beforeUpdate = DateTime.Now;

            // Act
            todoItem.UpdateFrom(updateRequest);
            var afterUpdate = DateTime.Now;

            // Assert
            Assert.IsTrue(todoItem.IsCompleted);
            Assert.IsNotNull(todoItem.CompletedDate);
            Assert.IsTrue(todoItem.CompletedDate >= beforeUpdate && todoItem.CompletedDate <= afterUpdate);
        }

        [TestMethod]
        public void UpdateFrom_Should_Handle_Uncompleting_Todo()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Id = 1,
                Title = "Test Todo",
                IsCompleted = true,
                CompletedDate = DateTime.Now,
                CreatedDate = DateTime.Now.AddDays(-1)
            };

            var updateRequest = new UpdateTodoRequest
            {
                Title = "Test Todo",
                Status = TodoStatus.Pending
            };

            // Act
            todoItem.UpdateFrom(updateRequest);

            // Assert
            Assert.IsFalse(todoItem.IsCompleted);
            Assert.IsNull(todoItem.CompletedDate);
        }

        [TestMethod]
        public void UpdateFrom_Should_Not_Throw_When_TodoItem_Is_Null()
        {
            // Arrange
            TodoItem todoItem = null;
            var updateRequest = new UpdateTodoRequest { Title = "Test" };

            // Act & Assert - Should not throw
            todoItem.UpdateFrom(updateRequest);
        }

        [TestMethod]
        public void UpdateFrom_Should_Not_Throw_When_UpdateRequest_Is_Null()
        {
            // Arrange
            var todoItem = new TodoItem { Title = "Test" };
            UpdateTodoRequest updateRequest = null;

            // Act & Assert - Should not throw
            todoItem.UpdateFrom(updateRequest);
        }

        [TestMethod]
        public void UpdateFrom_Should_Trim_Title_And_Description()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Title = "Original",
                Description = "Original",
                CreatedDate = DateTime.Now
            };

            var updateRequest = new UpdateTodoRequest
            {
                Title = "  Updated Title  ",
                Description = "  Updated Description  "
            };

            // Act
            todoItem.UpdateFrom(updateRequest);

            // Assert
            Assert.AreEqual("Updated Title", todoItem.Title);
            Assert.AreEqual("Updated Description", todoItem.Description);
        }
    }
}
