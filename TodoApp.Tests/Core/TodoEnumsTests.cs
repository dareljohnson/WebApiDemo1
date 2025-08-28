using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using TodoApp.Core;

namespace TodoApp.Tests.Core
{
    /// <summary>
    /// TDD Unit tests for TodoApp enumerations
    /// </summary>
    [TestClass]
    public class TodoEnumsTests
    {
        [TestMethod]
        public void TodoPriority_Should_Have_Correct_Values()
        {
            // Assert
            Assert.AreEqual(1, (int)TodoPriority.Low);
            Assert.AreEqual(2, (int)TodoPriority.Medium);
            Assert.AreEqual(3, (int)TodoPriority.High);
            Assert.AreEqual(4, (int)TodoPriority.Critical);
        }

        [TestMethod]
        public void TodoStatus_Should_Have_Correct_Values()
        {
            // Assert
            Assert.AreEqual(1, (int)TodoStatus.Pending);
            Assert.AreEqual(2, (int)TodoStatus.InProgress);
            Assert.AreEqual(3, (int)TodoStatus.Completed);
            Assert.AreEqual(4, (int)TodoStatus.Cancelled);
        }

        [TestMethod]
        [DataRow(TodoPriority.Low, "Low")]
        [DataRow(TodoPriority.Medium, "Medium")]
        [DataRow(TodoPriority.High, "High")]
        [DataRow(TodoPriority.Critical, "Critical")]
        public void TodoPriority_ToString_Should_Return_Correct_Names(TodoPriority priority, string expected)
        {
            // Act
            var result = priority.ToString();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow(TodoStatus.Pending, "Pending")]
        [DataRow(TodoStatus.InProgress, "InProgress")]
        [DataRow(TodoStatus.Completed, "Completed")]
        [DataRow(TodoStatus.Cancelled, "Cancelled")]
        public void TodoStatus_ToString_Should_Return_Correct_Names(TodoStatus status, string expected)
        {
            // Act
            var result = status.ToString();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TodoPriority_Should_Be_Orderable_By_Value()
        {
            // Arrange
            var priorities = new[] { TodoPriority.Critical, TodoPriority.Low, TodoPriority.High, TodoPriority.Medium };

            // Act
            var ordered = System.Linq.Enumerable.OrderBy(priorities, p => (int)p).ToArray();

            // Assert
            Assert.AreEqual(TodoPriority.Low, ordered[0]);
            Assert.AreEqual(TodoPriority.Medium, ordered[1]);
            Assert.AreEqual(TodoPriority.High, ordered[2]);
            Assert.AreEqual(TodoPriority.Critical, ordered[3]);
        }
    }
}
