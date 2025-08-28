using System;
using System.Web.Http;
using TodoApp.Data;
using TodoApp.Services;

namespace TodoApp.Api.Controllers
{
    /// <summary>
    /// Simple test controller to verify API is working
    /// </summary>
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(new { message = "API is working!", timestamp = System.DateTime.Now });
        }

        [HttpGet]
        [Route("database")]
        public IHttpActionResult TestDatabase()
        {
            try
            {
                using (var context = new TodoContext())
                {
                    // Test if we can create the context
                    var connectionString = context.Database.Connection.ConnectionString;
                    return Ok(new { 
                        message = "Database context created successfully!", 
                        connectionString = connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "...",
                        timestamp = DateTime.Now 
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Database test failed: {ex.Message}", ex));
            }
        }

        [HttpGet]
        [Route("service")]
        public IHttpActionResult TestService()
        {
            try
            {
                using (var context = new TodoContext())
                {
                    var service = new TodoService(context);
                    return Ok(new { 
                        message = "TodoService created successfully!", 
                        serviceType = service.GetType().Name,
                        timestamp = DateTime.Now 
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Service test failed: {ex.Message}", ex));
            }
        }
    }
}
