using System;
using TodoApp.Core;

namespace TodoApp.Api.Models
{
    public class UpdateTodoRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TodoPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public TodoStatus Status { get; set; }
    }
}
