using MusicStore.Models;

namespace MusicStore.Menus;

public class MainMenu
{
    private readonly PlateMenu _plateMenu;
    private readonly ShopMenu _shopMenu;
    private readonly RecomendationMenu _recomendationMenu;
    private readonly CustomerMenu _customerMenu;
    private readonly ReferenceMenu _referenceMenu;
    private readonly Services.AuthorizationService _authorization;

    // Initializes the main menu and its child menus.
    public MainMenu(PlateMenu plateMenu,
        ShopMenu shopMenu, 
        RecomendationMenu recomendationMenu,
        CustomerMenu customerMenu,
        ReferenceMenu referenceMenu,
        Services.AuthorizationService authorization
        )
    {
        _plateMenu = plateMenu;
        _shopMenu = shopMenu;
        _recomendationMenu = recomendationMenu;
        _customerMenu = customerMenu;
        _referenceMenu = referenceMenu;
        _authorization = authorization;
    }

    // Displays the main menu; returns true to close the application or false to sign out.
    public bool Show(Users user)
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine($"User: {user.Login}");
            Console.WriteLine("=== Main Menu ===");

            Console.WriteLine("1. Records menu");
            Console.WriteLine("2. Record store");
            Console.WriteLine("3. View recommendations");
            Console.WriteLine("4. Customers menu");
            Console.WriteLine("5. Sign out");
            if (_authorization.IsInRole(
                Services.UserRoles.Administrator, Services.UserRoles.Manager))
                Console.WriteLine("6. Reference data");
            Console.WriteLine("0. Exit application");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    _plateMenu.Show();
                    break;

                case "2":
                    _shopMenu.Show();
                    break;

                case "3":
                    _recomendationMenu.Show();
                    break;

                case "4":
                    _customerMenu.Show();
                    break;

                case "5":
                    return false;

                case "6" when _authorization.IsInRole(
                    Services.UserRoles.Administrator, Services.UserRoles.Manager):
                    _referenceMenu.Show();
                    break;

                case "0":
                    return true;
            }
        }
    }
}
