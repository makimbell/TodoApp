using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class TodoList
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Slug { get; set; } = string.Empty; // Sanitized and shortened Title for URL

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<TodoItem> Items { get; set; } = new();
}
