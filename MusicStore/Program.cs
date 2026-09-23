using MusicStore.Data;
using MusicStore.Menus;
using MusicStore.Services;

DatabaseInitializer dbInitializer = new();

if (!dbInitializer.Initialize())
{
    Console.WriteLine("The application cannot be started.");
    return;
}

AuthService authService = new();
AuthorizationService authorizationService = new();
InventoryService inventoryService = new();
PlateService plateService = new(authorizationService);
StockService stockService = new(inventoryService, authorizationService);
SaleService saleService = new(inventoryService, authorizationService);
CustomerService customerService = new(inventoryService, authorizationService);
ReferenceService referenceService = new(authorizationService);

LoginMenu loginMenu = new(authService);
SearchMenu searchMenu = new(plateService);
PlateMenu plateMenu = new(plateService, searchMenu, authorizationService);
ShopMenu shopMenu = new(stockService, saleService, authorizationService);
RecomendationMenu recomendationMenu = new(plateService);
CustomerMenu customerMenu = new(customerService, saleService, authorizationService);
ReferenceMenu referenceMenu = new(referenceService, authorizationService);

MainMenu mainMenu = new(
    plateMenu, shopMenu, recomendationMenu, customerMenu,
    referenceMenu, authorizationService);

while (true)
{
    var user = loginMenu.Show();

    if (user == null)
        break;

    authorizationService.SignIn(user);
    bool exitProgram = mainMenu.Show(user);
    authorizationService.SignOut();

    if (exitProgram)
        break;
}
