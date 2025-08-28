using log4net;
using Moq;
using System;
using System.Linq;
using System.Web.Http.Results;
using TodoApp.Api.Controllers;
using TodoApp.Api.Models;
using TodoApp.Core;
using TodoApp.Data;
using TodoApp.Data.Repositories;
using TodoApp.Services;

namespace TodoApp.Tests.Integration
{
    /// <summary>
    /// Integration tests for the Web API controller
    /// </summary>
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ApiIntegrationTests
    {
        private readonly TodoApiController _controller;
        private readonly TodoContext _context;

        public ApiIntegrationTests()
        {
            _context = new TodoContext();
            var todoRepository = new TodoRepository(_context);
            var todoService = new TodoService((ITodoRepository)todoRepository);
            var mockLogger = new Mock<ILog>().Object;
            _controller = new TodoApiController(todoService, mockLogger);
        }

        [TestMethod]
        public void Test_Api_Controller_Can_Create_Todo()
        {
            // Arrange
            var request = new CreateTodoRequest
            {
                Title = "API Test Todo",
                Description = "Created via API integration test",
                Priority = TodoPriority.High
            };

            // Act
            var result = _controller.Create(request);

            // Assert
            if (result == null)
                throw new Exception("Create returned null");

            var createdResult = result as CreatedNegotiatedContentResult<TodoResponse>;
            if (createdResult == null)
                throw new Exception("Create did not return a Created result");

            Console.WriteLine($"✓ API integration test: Created todo with ID {createdResult.Content.Id}");
        }

        [TestMethod]
        public void Test_Api_Controller_Can_Get_All_Todos()
        {
            // Arrange - Create a test todo first
            var request = new CreateTodoRequest
            {
                Title = "Get All Test Todo",
                Description = "Test todo for GetAll",
                Priority = TodoPriority.Medium
            };
            _controller.Create(request);

            // Act
            var result = _controller.GetAll();

            // Assert
            if (result == null)
                throw new Exception("GetAll returned null");

            var okResult = result as OkNegotiatedContentResult<System.Collections.Generic.List<TodoResponse>>;
            if (okResult == null)
                throw new Exception("GetAll did not return an Ok result with list");

            if (okResult.Content == null || !okResult.Content.Any())
                throw new Exception("GetAll returned null or empty collection");

            Console.WriteLine($"✓ API integration test: Retrieved {okResult.Content.Count} todos");
        }

        [TestMethod]
        public void Test_Api_Controller_Can_Update_Todo()
        {
            // Arrange - Create a test todo first
            var createRequest = new CreateTodoRequest
            {
                Title = "Update Test Todo",
                Description = "Original description",
                Priority = TodoPriority.Low
            };
            // Ensure controller has Request and Configuration set for CreatedNegotiatedContentResult
            _controller.Request = new System.Net.Http.HttpRequestMessage();
            _controller.Configuration = new System.Web.Http.HttpConfiguration();
            var createResult = _controller.Create(createRequest) as CreatedNegotiatedContentResult<TodoResponse>;
            if (createResult == null)
                throw new Exception("Failed to create test todo for update test");

            var updateRequest = new UpdateTodoRequest
            {
                Title = "Updated Todo Title",
                Description = "Updated description",
                Priority = TodoPriority.High,
                Status = TodoStatus.Completed
            };

            // Act
            var result = _controller.Update(Convert.ToInt32(createResult.Content.Id), updateRequest);

            // Assert
            if (result == null)
                throw new Exception("Update returned null");

            var okResult = result as OkNegotiatedContentResult<TodoResponse>;
            if (okResult == null)
                throw new Exception("Update did not return an Ok result");

            if (okResult.Content.Title != updateRequest.Title)
                throw new Exception($"Title not updated. Expected: {updateRequest.Title}, Got: {okResult.Content.Title}");

            if (!okResult.Content.IsCompleted)
                throw new Exception("Status not updated to completed");

            Console.WriteLine($"✓ API integration test: Updated todo {okResult.Content.Id} successfully");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }
    }
}
