using System.ComponentModel.DataAnnotations;

namespace Persistence.Auth.Users;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(100)]
    public string? Provider { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
