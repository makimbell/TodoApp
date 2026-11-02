using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<TodoList> TodoList => Set<TodoList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoList>(entity =>
        {
            entity.ToTable("TodoLists", "todo");
            entity.HasIndex(l => l.Slug).IsUnique();
        });

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("TodoItems", "todo");
            entity.HasOne(i => i.TodoList)
                  .WithMany(l => l.Items)
                  .HasForeignKey(i => i.TodoListId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Scopes all tables in this project to the 'todo' schema inside SandboxDb
        // This lets me reuse the Sandbox-Db for multiple projects without them interfering with each other
        modelBuilder.HasDefaultSchema("todo");
    }
}