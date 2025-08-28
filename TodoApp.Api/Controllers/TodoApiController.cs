using System;
using System.Linq;
using System.Web.Http;
using TodoApp.Api.Mappers;
using TodoApp.Api.Models;
using TodoApp.Core;
using TodoApp.Data;
using TodoApp.Services;
using log4net;

namespace TodoApp.Api.Controllers
{
    /// <summary>
    /// Web API controller for Todo operations
    /// </summary>
    [RoutePrefix("api/todos")]
    public class TodoApiController : ApiController
    {
    private readonly ITodoService _todoService;
    private readonly ILog _logger;

        // For dependency injection (testing)
        public TodoApiController(ITodoService todoService, ILog logger)
        {
            _todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
            _logger = logger ?? LogManager.GetLogger(typeof(TodoApiController));
        }

        /// <summary>
        /// Get all todos
        /// </summary>
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            _logger.Info("Getting all todos");
            try
            {
                var todos = _todoService.GetAll();
                var response = todos.Select(t => t.ToResponse()).ToList();
                _logger.Info($"Returned {response.Count} todos");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetAll", ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get todo by ID
        /// </summary>
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid ID");

                var todo = _todoService.GetById(id);
                if (todo == null)
                    return NotFound();

                return Ok(todo.ToResponse());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get todos by priority
        /// </summary>
        [HttpGet, Route("priority/{priority:int}")]
        public IHttpActionResult GetByPriority(int priority)
        {
            try
            {
                if (!Enum.IsDefined(typeof(TodoPriority), priority))
                    return BadRequest("Invalid priority value");

                var todos = _todoService.GetByPriority((TodoPriority)priority);
                var response = todos.Select(t => t.ToResponse()).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get completed todos
        /// </summary>
        [HttpGet, Route("completed")]
        public IHttpActionResult GetCompleted()
        {
            try
            {
                var todos = _todoService.GetCompleted();
                var response = todos.Select(t => t.ToResponse()).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get pending todos
        /// </summary>
        [HttpGet, Route("pending")]
        public IHttpActionResult GetPending()
        {
            try
            {
                var todos = _todoService.GetPending();
                var response = todos.Select(t => t.ToResponse()).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create new todo
        /// </summary>
        [HttpPost, Route("")]
        public IHttpActionResult Create([FromBody] CreateTodoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (request == null)
                    return BadRequest("Request body is required");

                var todoItem = request.ToEntity();
                var createdTodo = _todoService.Add(todoItem);
                var response = createdTodo.ToResponse();

                // Use Created to ensure a Created result is always returned
                return Created($"api/todos/{response.Id}", response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update existing todo
        /// </summary>
        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] UpdateTodoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (request == null)
                    return BadRequest("Request body is required");

                if (id <= 0)
                    return BadRequest("Invalid ID");

                var existingTodo = _todoService.GetById(id);
                if (existingTodo == null)
                    return NotFound();

                existingTodo.UpdateFrom(request);
                var updatedTodo = _todoService.Update(existingTodo);

                return Ok(updatedTodo.ToResponse());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Delete todo
        /// </summary>
        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid ID");

                var success = _todoService.Delete(id);
                if (!success)
                    return NotFound();

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get todo statistics
        /// </summary>
        [HttpGet, Route("stats")]
        public IHttpActionResult GetStats()
        {
            try
            {
                var stats = new
                {
                    TotalTodos = _todoService.GetAll().Count(),
                    CompletedCount = _todoService.GetCompletedCount(),
                    PendingCount = _todoService.GetPendingCount(),
                    CompletionRate = GetCompletionRate()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private double GetCompletionRate()
        {
            var total = _todoService.GetAll().Count();
            if (total == 0) return 0;

            var completed = _todoService.GetCompletedCount();
            return Math.Round((double)completed / total * 100, 2);
        }
    }
}
