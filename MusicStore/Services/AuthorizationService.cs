using MusicStore.Models;

namespace MusicStore.Services;

public static class UserRoles
{
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";
    public const string Seller = "Seller";
}

public class AuthorizationService
{
    public Users? CurrentUser { get; private set; }

    // Stores the currently authenticated user.
    public void SignIn(Users user) => CurrentUser = user;
    // Clears the current authenticated user.
    public void SignOut() => CurrentUser = null;

    // Checks whether the current user has one of the specified roles.
    public bool IsInRole(params string[] roles) =>
        CurrentUser != null && roles.Any(role =>
            string.Equals(CurrentUser.Role, role, StringComparison.OrdinalIgnoreCase));

    // Requires authentication and one of the specified roles.
    public void Require(params string[] roles)
    {
        if (CurrentUser == null)
            throw new UnauthorizedAccessException("Authentication is required.");
        if (!IsInRole(roles))
            throw new UnauthorizedAccessException("You do not have permission to perform this operation.");
    }
}
