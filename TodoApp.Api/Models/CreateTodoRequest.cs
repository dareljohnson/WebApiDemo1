using System;
using TodoApp.Core;

namespace TodoApp.Api.Models
{
    public class CreateTodoRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;
        public DateTime? DueDate { get; set; }
    }
}
