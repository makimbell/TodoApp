using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scopes all tables in this project to the 'todo' schema inside SandboxDb
        // This lets me reuse the Sandbox-Db for multiple projects without them interfering with each other
        modelBuilder.HasDefaultSchema("todo");
    }
}