using System;
using TodoApp.Core;

namespace TodoApp.Api.Models
{
    public class TodoResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TodoPriority Priority { get; set; }
        public TodoStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
