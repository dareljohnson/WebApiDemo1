using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TodoApp.Core;

namespace TodoApp.Tests.Core
{
    [TestClass]
    public class SimpleTodoTests
    {
        [TestMethod]
        public void TestTodoCreation()
        {
            var todo = new TodoItem();
            Assert.AreEqual(0, todo.Id, "Default ID should be 0");
            Assert.IsFalse(todo.IsCompleted, "Default IsCompleted should be false");
            Assert.AreEqual(0, (int)todo.Priority, "Default Priority should be 0 (unset)");
        }

        [TestMethod]
        public void TestTodoTitleSetting()
        {
            var todo = new TodoItem();
            var title = "Test Todo Item";
            todo.Title = title;
            Assert.AreEqual(title, todo.Title, "Title was not set correctly");
        }

        [TestMethod]
        public void TestTodoCompletion()
        {
            var todo = new TodoItem();
            todo.IsCompleted = true;
            Assert.IsTrue(todo.IsCompleted, "IsCompleted was not set to true");
        }

        [TestMethod]
        public void TestTodoPrioritySettings()
        {
            var todo = new TodoItem();
            todo.Priority = TodoPriority.High;
            Assert.AreEqual(TodoPriority.High, todo.Priority, "High priority was not set correctly");
            todo.Priority = TodoPriority.Low;
            Assert.AreEqual(TodoPriority.Low, todo.Priority, "Low priority was not set correctly");
            todo.Priority = TodoPriority.Critical;
            Assert.AreEqual(TodoPriority.Critical, todo.Priority, "Critical priority was not set correctly");
        }

        [TestMethod]
        public void TestTodoDateOperations()
        {
            var todo = new TodoItem();
            var testDate = new DateTime(2025, 1, 1);
            todo.CreatedDate = testDate;
            Assert.AreEqual(testDate, todo.CreatedDate, "CreatedDate was not set correctly");
            Assert.IsNull(todo.CompletedDate, "CompletedDate should be null by default");
            var completedDate = new DateTime(2025, 1, 2);
            todo.CompletedDate = completedDate;
            Assert.AreEqual(completedDate, todo.CompletedDate, "CompletedDate was not set correctly");
        }

        [TestMethod]
        public void TestTodoDescription()
        {
            var todo = new TodoItem();
            var description = "This is a test description for the todo item";
            Assert.IsNull(todo.Description, "Description should be null by default");
            todo.Description = description;
            Assert.AreEqual(description, todo.Description, "Description was not set correctly");
        }
    }
}
