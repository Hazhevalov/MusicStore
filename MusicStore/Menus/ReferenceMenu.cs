using Exam.Services;

namespace Exam.Menus;

public class ReferenceMenu
{
    private readonly ReferenceService _service;
    private readonly AuthorizationService _authorization;

    // Initializes the reference-data menu.
    public ReferenceMenu(ReferenceService service, AuthorizationService authorization)
    {
        _service = service;
        _authorization = authorization;
    }

    // Displays the reference-data categories.
    public void Show()
    {
        _authorization.Require(UserRoles.Administrator, UserRoles.Manager);
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Reference Data ===");
            Console.WriteLine("1. Artists");
            Console.WriteLine("2. Genres");
            Console.WriteLine("3. Publishers");
            Console.WriteLine("0. Back");
            if (!int.TryParse(Console.ReadLine(), out int value) || value < 0 || value > 3)
                continue;
            if (value == 0) return;
            ShowType((ReferenceType)value);
        }
    }

    // Displays actions for a selected reference-data type.
    private void ShowType(ReferenceType type)
    {
        while (true)
        {
            Console.Clear();
            Print(type);
            Console.WriteLine("1. Add");
            Console.WriteLine("2. Rename");
            if (_authorization.IsInRole(UserRoles.Administrator))
                Console.WriteLine("3. Delete");
            Console.WriteLine("0. Back");
            string? choice = Console.ReadLine();
            try
            {
                switch (choice)
                {
                    case "1":
                        Console.Write("Name: ");
                        _service.Add(type, Console.ReadLine() ?? "");
                        break;
                    case "2":
                        _service.Rename(type, ReadInt("ID: "), ReadName());
                        break;
                    case "3" when _authorization.IsInRole(UserRoles.Administrator):
                        _service.Delete(type, ReadInt("ID: "));
                        break;
                    case "0": return;
                    default: continue;
                }
                Console.WriteLine("Operation completed.");
            }
            catch (Exception ex) when (ex is ArgumentException or UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ReadKey();
        }
    }

    // Prints all entries of a reference-data type.
    private void Print(ReferenceType type)
    {
        foreach (var item in _service.GetAll(type))
            Console.WriteLine($"{item.Id,4}  {item.Name}");
        Console.WriteLine();
    }

    // Reads a new reference-data name.
    private static string ReadName()
    {
        Console.Write("New name: ");
        return Console.ReadLine() ?? "";
    }

    // Reads an integer from the console.
    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value)) return value;
        }
    }
}
