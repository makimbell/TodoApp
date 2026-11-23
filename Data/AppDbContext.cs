using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserLogin> UserLogins => Set<UserLogin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "todo");
            entity.HasIndex(u => u.NormalizedEmail)
                  .IsUnique()
                  .HasFilter("[NormalizedEmail] IS NOT NULL");
        });

        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.ToTable("UserLogins", "todo");
            entity.HasIndex(ul => new { ul.Provider, ul.ProviderKey }).IsUnique();
            entity.HasOne(ul => ul.User)
                  .WithMany(u => u.Logins)
                  .HasForeignKey(ul => ul.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TodoList>(entity =>
        {
            entity.ToTable("TodoLists", "todo");
            entity.HasIndex(l => l.Slug).IsUnique();
            entity.HasIndex(l => new { l.OwnerId, l.CreatedAt });
        });

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("TodoItems", "todo");
            entity.HasOne(i => i.TodoList)
                  .WithMany(l => l.Items)
                  .HasForeignKey(i => i.TodoListId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Composite index covering the list looking and sort order
            entity.HasIndex(i => new { i.TodoListId, i.IsCompleted, i.CreatedAt });
        });

        // Scopes all tables in this project to the 'todo' schema inside SandboxDb
        // This lets me reuse the Sandbox-Db for multiple projects without them interfering with each other
        modelBuilder.HasDefaultSchema("todo");
    }
}