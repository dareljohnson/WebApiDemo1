using System.Data.Entity.Migrations;
using TodoApp.Data;

namespace TodoApp.Data.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<TodoContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(TodoContext context)
        {
            // Seed initial data if needed
        }
    }
}
