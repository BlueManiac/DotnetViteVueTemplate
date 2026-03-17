namespace Persistence.Auth;

/// <summary>
/// Represents the currently authenticated user.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The unique identifier of the current user.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// The display name of the current user.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// The email address of the current user.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// The authentication provider used (e.g., "password", "google", "microsoft").
    /// </summary>
    string? Provider { get; }

    /// <summary>
    /// Indicates whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
