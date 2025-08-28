using System.Data.Entity;
using TodoApp.Core;

namespace TodoApp.Data
{
    /// <summary>
    /// Entity Framework database context for Todo application
    /// </summary>
    public class TodoContext : DbContext
    {
        static TodoContext()
        {
            Database.SetInitializer(new InitializeDatabase());
        }
        
        public TodoContext() : base("DefaultConnection") 
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }
        
        public DbSet<TodoItem> TodoItems { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure TodoItem entity
            modelBuilder.Entity<TodoItem>()
                .HasKey(t => t.Id);
                
            modelBuilder.Entity<TodoItem>()
                .Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);
                
            modelBuilder.Entity<TodoItem>()
                .Property(t => t.Description)
                .HasMaxLength(1000);
                
            modelBuilder.Entity<TodoItem>()
                .Property(t => t.CreatedDate)
                .IsRequired();
        }
    }
}
