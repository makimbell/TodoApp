using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class User
{
    [StringLength(256)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [StringLength(256)]
    public string? Email { get; set; }

    [StringLength(256)]
    public string? NormalizedEmail { get; set; }

    [StringLength(256)]
    public string? DisplayName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<UserLogin> Logins { get; set; } = new();
}

