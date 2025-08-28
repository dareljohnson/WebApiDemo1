using System.Data.Entity;

namespace TodoApp.Data
{
    public class InitializeDatabase : CreateDatabaseIfNotExists<TodoContext>
    {
        protected override void Seed(TodoContext context)
        {
            // Optionally seed initial data
            base.Seed(context);
        }
    }
}
