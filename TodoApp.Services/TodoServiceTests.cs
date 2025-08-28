using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Moq;
using TodoApp.Core;
using TodoApp.Data;
using Xunit;

namespace TodoApp.Services.Tests
{
    public class TodoServiceTests
    {
        [Fact]
        public void GetAll_ReturnsAllTodos()
        {
            var data = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Test 1", IsCompleted = false },
                new TodoItem { Id = 2, Title = "Test 2", IsCompleted = true }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<TodoItem>>();
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<TodoContext>();
            mockContext.Setup(c => c.TodoItems).Returns(mockSet.Object);

            var service = new TodoService(mockContext.Object);
            var todos = service.GetAll().ToList();

            Assert.Equal(2, todos.Count);
            Assert.Equal("Test 1", todos[0].Title);
        }
    }
}
