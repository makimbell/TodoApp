using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class UserLogin
{
    public int Id { get; set; }

    [Required, StringLength(256)]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(64)]
    public string Provider { get; set; } = string.Empty;

    [Required, StringLength(256)]
    public string ProviderKey { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

