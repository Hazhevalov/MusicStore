using MusicStore.Data;
using MusicStore.Models;
using Microsoft.EntityFrameworkCore;
using MusicStore.Helpers;

namespace MusicStore.Services;

public class AuthService
{
    // Authenticates a user with a username and password.
    public Users? Login(string login, string password)
    {
        using var db = new ApplicationContext();

        var user = db.Users.FirstOrDefault(u => u.Login == login.Trim());

        if (user == null || !PasswordSecurity.Verify(password, user.PasswordHash))
            return null;

        return new Users
        {
            Id = user.Id,
            Login = user.Login,
            PasswordHash = string.Empty,
            Role = user.Role
        };
    }
}
