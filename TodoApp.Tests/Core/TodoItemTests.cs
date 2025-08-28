using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TodoApp.Core;

namespace TodoApp.Tests.Core
{
    /// <summary>
    /// TDD Unit tests for TodoItem domain model
    /// </summary>
    [TestClass]
    public class TodoItemTests
    {


        [TestMethod]
        public void TodoItem_Should_Set_Properties_Correctly()
        {
            // Arrange
            var createdDate = DateTime.Now;
            var completedDate = DateTime.Now.AddHours(1);

            // Act
            var todoItem = new TodoItem
            {
                Id = 1,
                Title = "Test Todo",
                Description = "Test Description",
                IsCompleted = true,
                CreatedDate = createdDate,
                CompletedDate = completedDate,
                Priority = TodoPriority.High
            };

            // Assert
            Assert.AreEqual(1, todoItem.Id);
            Assert.AreEqual("Test Todo", todoItem.Title);
            Assert.AreEqual("Test Description", todoItem.Description);
            Assert.IsTrue(todoItem.IsCompleted);
            Assert.AreEqual(createdDate, todoItem.CreatedDate);
            Assert.AreEqual(completedDate, todoItem.CompletedDate);
            Assert.AreEqual(TodoPriority.High, todoItem.Priority);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void TodoItem_Should_Fail_Validation_With_Empty_Title(string title)
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Title = title,
                Description = "Valid description"
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(todoItem);

            // Assert
            Assert.IsTrue(validationResults.Any(v => v.MemberNames.Contains("Title")));
        }

        [TestMethod]
        public void TodoItem_Should_Fail_Validation_With_Long_Title()
        {
            // Arrange
            var longTitle = new string('a', 201); // Exceeds 200 character limit
            var todoItem = new TodoItem
            {
                Title = longTitle,
                Description = "Valid description"
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(todoItem);

            // Assert
            Assert.IsTrue(validationResults.Any(v => v.MemberNames.Contains("Title")));
        }

        [TestMethod]
        public void TodoItem_Should_Pass_Validation_With_Valid_Data()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Title = "Valid Title",
                Description = "Valid description",
                Priority = TodoPriority.Medium
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(todoItem);

            // Assert
            Assert.AreEqual(0, validationResults.Count);
        }

        [TestMethod]
        [DataRow(TodoPriority.Low)]
        [DataRow(TodoPriority.Medium)]
        [DataRow(TodoPriority.High)]
        [DataRow(TodoPriority.Critical)]
        public void TodoItem_Should_Accept_All_Priority_Values(TodoPriority priority)
        {
            // Arrange & Act
            var todoItem = new TodoItem
            {
                Title = "Test",
                Priority = priority
            };

            // Assert
            Assert.AreEqual(priority, todoItem.Priority);
        }
    }

    /// <summary>
    /// Helper class for model validation testing
    /// </summary>
    public static class ValidationHelper
    {
        public static System.Collections.Generic.List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}
