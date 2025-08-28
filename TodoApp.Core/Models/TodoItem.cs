using System;
using System.ComponentModel.DataAnnotations;

namespace TodoApp.Core
{
    /// <summary>
    /// Core domain model for Todo items
    /// </summary>
    public class TodoItem
    {
        public TodoItem()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public TodoPriority Priority { get; set; } = TodoPriority.None;
    }
}
