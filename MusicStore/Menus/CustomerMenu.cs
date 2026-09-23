using MusicStore.Services;

using MusicStore.Models;

namespace MusicStore.Menus;

public class CustomerMenu
{
    private readonly CustomerService _customerService;
    private readonly SaleService _saleService;
    private readonly AuthorizationService _authorization;

    // Initializes the customer menu and related services.
    public CustomerMenu(
        CustomerService customerService,
        SaleService saleService,
        AuthorizationService authorization)
    {
        _customerService = customerService;
        _saleService = saleService;
        _authorization = authorization;
    }

    // Displays customer, promotion, and reservation actions.
    public void Show()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("=== Customers Menu ===");
            Console.WriteLine("1. Add customer");
            Console.WriteLine("2. List customers");
            if (_authorization.IsInRole(UserRoles.Administrator, UserRoles.Manager))
            {
                Console.WriteLine("3. Add promotion");
                Console.WriteLine("4. View promotions");
            }
            Console.WriteLine("5. Reserve a record");
            Console.WriteLine("6. View reservations");
            Console.WriteLine("7. Cancel reservation");
            Console.WriteLine("8. Sell reserved record");
            Console.WriteLine("0. Back");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddCustomer();
                    break;

                case "2":
                    PrintCustomers();
                    break;

                case "3" when _authorization.IsInRole(UserRoles.Administrator, UserRoles.Manager):
                    AddPromotion();
                    break;

                case "4" when _authorization.IsInRole(UserRoles.Administrator, UserRoles.Manager):
                    PrintPromotions();
                    break;

                case "5":
                    ReservePlate();
                    break;

                case "6":
                    PrintReservations();
                    break;

                case "7":
                    CancelReservation();
                    break;

                case "8":
                    SellReservation();
                    break;

                case "0":
                    return;
            }
        }
    }

    // Cancels an active reservation selected by ID.
    private void CancelReservation()
    {
        Console.Clear();
        PrintReservations(waitForKey: false);
        try
        {
            _customerService.CancelReservation(ReadInt("Reservation ID: "));
            Console.WriteLine("Reservation cancelled.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Pause();
    }

    // Completes a sale from an active reservation.
    private void SellReservation()
    {
        Console.Clear();
        PrintReservations(waitForKey: false);
        try
        {
            int id = ReadInt("Reservation ID: ");
            int quantity = ReadInt("Quantity to sell: ");
            _saleService.SaleReservation(id, quantity);
            Console.WriteLine("Sale completed and reservation updated.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Pause();
    }

    // Adds a new customer.
    private void AddCustomer()
    {
        Console.Clear();

        Console.WriteLine("=== Add Customer ===");

        Console.Write("Enter a name: ");
        string name = Console.ReadLine() ?? "";

        try
        {
            _customerService.Add(name);

            Console.WriteLine("Customer added successfully.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Pause();
    }

    // Displays all customers.
    private void PrintCustomers()
    {
        Console.Clear();

        Console.WriteLine("=== Customer List ===");

        var customers = _customerService.GetCustomers();

        if (customers.Count == 0)
        {
            Console.WriteLine("The customer list is empty.");
            Pause();
            return;
        }

        Console.WriteLine(new string('-', 74));
        Console.WriteLine(
            $"| {"ID",3} | {"Name",-28} | {"Total spent",14} | {"Registered",-16} |");
        Console.WriteLine(new string('-', 74));

        foreach (var c in customers)
        {
            string name = c.Name.Length > 28
                ? c.Name[..27] + "…"
                : c.Name;

            Console.WriteLine(
                $"| {c.Id,3} | {name,-28} | {c.TotalSpent,14:N2} | " +
                $"{c.RegisteredAt.ToString("dd.MM.yyyy"),-16} |");
        }

        Console.WriteLine(new string('-', 74));

        Pause();
    }

    // Creates a promotion for a genre.
    private void AddPromotion()
    {   
        Console.Clear();

        Console.WriteLine("=== Add Promotion ===");

        Console.Write("Promotion name: ");
        string name = Console.ReadLine() ?? "";

        Console.WriteLine("\nAvailable genres:");

        var genres = _customerService.GetGenres();

        if (genres.Count == 0)
        {
            Console.WriteLine("No genres are available.");
            Pause();
            return;
        }

        Console.WriteLine(new string('-', 35));
        Console.WriteLine($"| {"ID",3} | {"Genre",-25} |");
        Console.WriteLine(new string('-', 35));

        foreach (var genre in genres)
        {
            string genreName = genre.Name.Length > 25
                ? genre.Name[..24] + "…"
                : genre.Name;

            Console.WriteLine($"| {genre.Id,3} | {genreName,-25} |");
        }

        Console.WriteLine(new string('-', 35));

        int genreId = ReadInt("\nGenre ID: ");

        decimal discount = ReadDecimal(
            "Discount percentage: ");

        DateOnly startDate = ReadDate(
            "Start date (dd.MM.yyyy): ");

        DateOnly endDate = ReadDate(
            "End date (dd.MM.yyyy): ");

        try
        {
            _customerService.AddPromotion(
                name,
                genreId,
                discount,
                startDate,
                endDate);

            Console.WriteLine("Promotion added successfully.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Pause();
    }

    // Displays all promotions and their status.
    private void PrintPromotions()
    {
        Console.Clear();

        Console.WriteLine("=== Promotion List ===");

        var promotions = _customerService.GetPromotions();

        if (promotions.Count == 0)
        {
            Console.WriteLine("The promotion list is empty.");
            Pause();
            return;
        }

        DateOnly today =
            DateOnly.FromDateTime(DateTime.Today);

        Console.WriteLine(new string('-', 101));
        Console.WriteLine(
            $"| {"ID",3} | {"Name",-22} | {"Genre",-12} | {"Discount",9} | " +
            $"{"Start",-10} | {"End",-10} | {"Status",-13} |");
        Console.WriteLine(new string('-', 101));

        foreach (var p in promotions)
        {
            string name = p.Name.Length > 22
                ? p.Name[..21] + "…"
                : p.Name;
            string genre = p.Genre.Name.Length > 12
                ? p.Genre.Name[..11] + "…"
                : p.Genre.Name;

            string status = today < p.StartDate
                ? "Scheduled": today > p.EndDate
                    ? "Finished": "Active";

            Console.WriteLine(
                $"| {p.Id,3} | {name,-22} | {genre,-12} | " +
                $"{p.DiscountPercent,8:N2}% | {p.StartDate:dd.MM.yyyy} | " +
                $"{p.EndDate:dd.MM.yyyy} | {status,-13} |");
        }

        Console.WriteLine(new string('-', 101));

        Pause();
    }

    // Creates a record reservation for a customer.
    private void ReservePlate()
    {
        Console.Clear();

        Console.WriteLine("=== Reserve Record ===");

        var customers = _customerService.GetCustomers();

        if (customers.Count == 0)
        {
            Console.WriteLine("Add a customer first.");
            Pause();
            return;
        }

        Console.WriteLine("\nCustomers:");

        Console.WriteLine(new string('-', 38));
        Console.WriteLine($"| {"ID",3} | {"Customer",-28} |");
        Console.WriteLine(new string('-', 38));

        foreach (var customer in customers)
        {
            string name = customer.Name.Length > 28
                ? customer.Name[..27] + "…"
                : customer.Name;

            Console.WriteLine($"| {customer.Id,3} | {name,-28} |");
        }

        Console.WriteLine(new string('-', 38));

        int customerId = ReadInt("\nCustomer ID: ");

        var plates = _customerService.GetAll();

        if (plates.Count == 0)
        {
            Console.WriteLine("No records are available.");
            Pause();
            return;
        }

        Console.WriteLine("\nRecords:");

        Console.WriteLine(new string('-', 69));
        Console.WriteLine(
            $"| {"ID",3} | {"Record",-24} | {"Artist",-22} | {"In stock",7} |");
        Console.WriteLine(new string('-', 69));

        foreach (var plate in plates)
        {
            string title = plate.Title.Length > 24
                ? plate.Title[..23] + "…"
                : plate.Title;
            string artist = plate.Artist.Name.Length > 22
                ? plate.Artist.Name[..21] + "…"
                : plate.Artist.Name;

            Console.WriteLine(
                $"| {plate.Id,3} | {title,-24} | " +
                $"{artist,-22} | {plate.Quantity,7} |");
        }

        Console.WriteLine(new string('-', 69));

        int plateId = ReadInt("\nRecord ID: ");

        int quantity = ReadInt("Quantity: ");

        DateOnly expiresAt = ReadDate(
            "Reserve until (dd.MM.yyyy): ");

        try
        {
            _customerService.Reserve(
                customerId,
                plateId,
                quantity,
                expiresAt);

            Console.WriteLine(
                "Records reserved successfully.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Pause();
    }

    // Displays reservations and their current status.
    private void PrintReservations(bool waitForKey = true)
    {
        Console.Clear();

        Console.WriteLine("=== Reservations ===");

        var reservations = _customerService.GetReservations();

        if (reservations.Count == 0)
        {
            Console.WriteLine("There are no reservations.");
            if (waitForKey) Pause();
            return;
        }

        DateOnly today =
            DateOnly.FromDateTime(DateTime.Today);

        Console.WriteLine(new string('-', 104));
        Console.WriteLine(
            $"| {"ID",3} | {"Customer",-22} | {"Record",-20} | " +
            $"{"Qty",6} | {"Date",-10} | {"Until",-10} | {"Status",-11} |");
        Console.WriteLine(new string('-', 104));

        foreach (var r in reservations)
        {
            string customer = r.Customer.Name.Length > 22
                ? r.Customer.Name[..21] + "…"
                : r.Customer.Name;
            string plate = r.Plate.Title.Length > 20
                ? r.Plate.Title[..19] + "…"
                : r.Plate.Title;

            string status = r.Status switch
            {
                ReservationStatus.Active => "Active",
                ReservationStatus.Completed => "Completed",
                ReservationStatus.Cancelled => "Cancelled",
                ReservationStatus.Expired => "Expired",
                _ => "Unknown"
            };

            Console.WriteLine(
                $"| {r.Id,3} | {customer,-22} | {plate,-20} | " +
                $"{r.Quantity - r.FulfilledQuantity,6} | {r.ReservedAt:dd.MM.yyyy} | " +
                $"{r.ExpiresAt:dd.MM.yyyy} | {status,-11} |");
        }

        Console.WriteLine(new string('-', 104));

        if (waitForKey) Pause();
    }

    // Reads an integer from the console.
    private int ReadInt(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (int.TryParse(
                Console.ReadLine(), out int value))
            {
                return value;
            }

            Console.WriteLine(
                "Invalid number. Please try again.");
        }
    }

    // Reads a decimal value from the console.
    private decimal ReadDecimal(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (decimal.TryParse(
                Console.ReadLine(), out decimal value))
            {
                return value;
            }

            Console.WriteLine(
                "Invalid number. Please try again.");
        }
    }

    // Reads a date in dd.MM.yyyy format.
    private DateOnly ReadDate(string message)
    {
        while (true)
        {
            Console.Write(message);

            string input = Console.ReadLine() ?? "";

            if (DateOnly.TryParseExact(
                input,
                "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateOnly date))
            {
                return date;
            }

            Console.WriteLine(
                "Invalid date. Example: 25.09.2026");
        }
    }

    // Pauses until the user presses a key.
    private void Pause()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}
