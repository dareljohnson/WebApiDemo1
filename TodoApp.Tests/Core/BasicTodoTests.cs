using Microsoft.VisualStudio.TestTools.UnitTesting;
using TodoApp.Core;

namespace TodoApp.Tests.Core
{
    [TestClass]
    public class BasicTodoTests
    {
        [TestMethod]
        public void CreateTodoItem_ShouldSetDefaultValues()
        {
            // Act
            var todo = new TodoItem();

            // Assert
            Assert.AreEqual(0, todo.Id);
            Assert.IsFalse(todo.IsCompleted);
            Assert.AreEqual(TodoPriority.None, todo.Priority);
        }

        [TestMethod]
        public void SetTodoTitle_ShouldWork()
        {
            // Arrange
            var todo = new TodoItem();
            var title = "Test Todo";

            // Act
            todo.Title = title;

            // Assert
            Assert.AreEqual(title, todo.Title);
        }

        [TestMethod]
        public void SetTodoCompleted_ShouldWork()
        {
            // Arrange
            var todo = new TodoItem();

            // Act
            todo.IsCompleted = true;

            // Assert
            Assert.IsTrue(todo.IsCompleted);
        }
    }
}
