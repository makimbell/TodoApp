using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public enum TodoPriority
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3
}

public class TodoItem
{
    public int Id { get; set; }
    public int TodoListId { get; set; }
    public TodoList? TodoList { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TodoPriority Priority { get; set; } = TodoPriority.None;

    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
