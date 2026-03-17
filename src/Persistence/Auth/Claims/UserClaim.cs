using Microsoft.EntityFrameworkCore;
using Persistence.Auth.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Persistence.Auth.Claims;

[PrimaryKey(nameof(UserId), nameof(Provider), nameof(ClaimType))]
public class UserClaim
{
    public required Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [MaxLength(50)]
    public required string Provider { get; set; }

    [MaxLength(100)]
    public required string ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
