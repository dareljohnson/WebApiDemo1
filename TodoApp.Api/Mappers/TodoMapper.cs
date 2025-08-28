using System;
using TodoApp.Api.Models;
using TodoApp.Core;

namespace TodoApp.Api.Mappers
{
    /// <summary>
    /// Mapper for converting between domain models and API DTOs
    /// </summary>
    public static class TodoMapper
    {
        /// <summary>
        /// Maps domain TodoItem to API response DTO
        /// </summary>
        public static TodoResponse ToResponse(this TodoItem item)
        {
            if (item == null) return null;
            
            return new TodoResponse
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Priority = item.Priority,
                Status = item.IsCompleted ? TodoStatus.Completed : TodoStatus.Pending,
                DueDate = null, // Not available in current model
                CreatedAt = item.CreatedDate,
                UpdatedAt = item.CreatedDate, // Not tracked separately in current model
                IsCompleted = item.IsCompleted,
                CompletedAt = item.CompletedDate
            };
        }
        
        /// <summary>
        /// Maps create request DTO to domain TodoItem
        /// </summary>
        public static TodoItem ToEntity(this CreateTodoRequest request)
        {
            if (request == null) return null;
            
            return new TodoItem
            {
                Title = request.Title?.Trim(),
                Description = request.Description?.Trim(),
                Priority = request.Priority,
                CreatedDate = DateTime.Now,
                IsCompleted = false
            };
        }
        
        /// <summary>
        /// Updates domain TodoItem from update request DTO
        /// </summary>
        public static void UpdateFrom(this TodoItem item, UpdateTodoRequest request)
        {
            if (item == null || request == null) return;
            
            item.Title = request.Title?.Trim();
            item.Description = request.Description?.Trim();
            item.Priority = request.Priority;
            
            // Handle completion status change
            if (!item.IsCompleted && request.Status == TodoStatus.Completed)
            {
                item.CompletedDate = DateTime.Now;
                item.IsCompleted = true;
            }
            else if (item.IsCompleted && request.Status != TodoStatus.Completed)
            {
                item.CompletedDate = null;
                item.IsCompleted = false;
            }
        }
    }
}
