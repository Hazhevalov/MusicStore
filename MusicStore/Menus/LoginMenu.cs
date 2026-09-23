using MusicStore.Services;
using MusicStore.Models;

namespace MusicStore.Menus;

public class LoginMenu
{
    private readonly AuthService _authService;

    // Initializes the login menu with the authentication service.
    public LoginMenu(AuthService authService)
    {
        _authService = authService;
    }

    // Prompts for credentials and returns the authenticated user.
    public Users? Show()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Sign In ===");

            Console.Write("Username: ");
            string login = Console.ReadLine() ?? "";

            Console.Write("Password: ");
            string password = ReadPassword();

            Users? user = _authService.Login(login, password);

            if (user != null)
            {
                return user;
            }

            Console.WriteLine("Incorrect username or password.");
            Console.WriteLine("Press Enter to try again or Esc to exit.");

            if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                return null;
        }
    }

    // Reads a masked password from the console.
    private static string ReadPassword()
    {
        var password = new System.Text.StringBuilder();
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return password.ToString();
            }
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Length--;
                Console.Write("\b \b");
                continue;
            }
            if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write('*');
            }
        }
    }
}
