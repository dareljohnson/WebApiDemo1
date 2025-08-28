using System.Data.Entity;
using TodoApp.Core;

namespace TodoApp.Tests.Data
{
    // In-memory DbContext for EF6 integration-style tests
    public class TestTodoContext : DbContext
    {
        public DbSet<TodoItem> Todos { get; set; }

        public TestTodoContext() : base("name=TestTodoContext")
        {
            // Use Effort or a localdb connection string for true in-memory
            // For pure in-memory, you can use Effort (if available) or LocalDb for integration
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoItem>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
