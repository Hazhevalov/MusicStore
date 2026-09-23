using Exam.Models;
using Exam.Services;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Numerics;

namespace Exam.Menus;

public class ShopMenu
{
    private readonly SaleService _saleService;
    private readonly StockService _stockService;
    private readonly AuthorizationService _authorization;

    // Initializes the store menu and related services.
    public ShopMenu(
        StockService stockService,
        SaleService saleService,
        AuthorizationService authorization)
    {
        _stockService = stockService;
        _saleService = saleService;
        _authorization = authorization;
    }

    // Displays store, inventory, and sales actions.
    public void Show()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("=== Store ===");
            Console.WriteLine("1. Record catalog");
            if (_authorization.IsInRole(UserRoles.Administrator, UserRoles.Manager))
            {
                Console.WriteLine("2. Receive records");
                Console.WriteLine("3. Write off records");
            }
            Console.WriteLine("4. Sell record");
            Console.WriteLine("5. View stock movements");
            Console.WriteLine("6. View sales");
            Console.WriteLine("7. Sell reserved record");
            Console.WriteLine("0. Back");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAll();
                    Console.ReadKey();
                    break;

                case "2" when _authorization.IsInRole(UserRoles.Administrator, UserRoles.Manager):
                    RecieptPlate();
                    break;

                case "3" when _authorization.IsInRole(UserRoles.Administrator, UserRoles.Manager):
                    WriteOffPlate();
                    break;

                case "4":
                    SalePlate();
                    break;

                case "5":
                    PrintStockMovement();
                    break;

                case "6":
                    PrintSales();
                    break;

                case "7":
                    SaleReservedPlate();
                    break;

                case "0":
                    return;
            }
        }
    }

    // Completes a sale from a reservation.
    private void SaleReservedPlate()
    {
        Console.Clear();
        Console.WriteLine("=== Sell Reserved Record ===");
        int reservationId = ReadInt("Reservation ID: ");
        int amount = ReadInt("Quantity: ");
        try
        {
            _saleService.SaleReservation(reservationId, amount);
            Console.WriteLine("Sale completed and reservation updated.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.ReadKey();
    }

    // Displays all records available in the store.
    private void ShowAll()
    {
        var plates = _stockService.GetAll();

        if (plates.Count == 0)
        {
            Console.WriteLine("The record list is empty.");
            return;
        }
        Console.WriteLine(new string('-', 116));
        Console.WriteLine(
            $"| {"ID",3} | {"Title",-20} | {"Artist",-16} | " +
            $"{"Publisher",-15} | {"Genre",-10} | {"Tracks",5} | " +
            $"{"Year",4} | {"Price",9} | {"Qty",6} |");
        Console.WriteLine(new string('-', 116));

        foreach (var plate in plates)
        {
            string title = plate.Title.Length > 20
                ? plate.Title[..19] + "…"
                : plate.Title;
            string artist = plate.Artist.Name.Length > 16
                ? plate.Artist.Name[..15] + "…"
                : plate.Artist.Name;
            string publisher = plate.Publisher.Name.Length > 15
                ? plate.Publisher.Name[..14] + "…"
                : plate.Publisher.Name;
            string genre = plate.Genre.Name.Length > 10
                ? plate.Genre.Name[..9] + "…"
                : plate.Genre.Name;

            Console.WriteLine(
                $"| {plate.Id,3} | {title,-20} | {artist,-16} | " +
                $"{publisher,-15} | {genre,-10} | {plate.TrackCount,5} | " +
                $"{plate.ReleaseYear,4} | {plate.SalePrise,9:N2} | {plate.Quantity,6} |");
        }
        Console.WriteLine(new string('-', 116));
    }

    // Receives additional units of a record into stock.
    private void RecieptPlate()
    {
        Console.Clear();
        Console.WriteLine("=== Receive Records ===");
        ShowAll();
        int plateId = ReadInt("Enter a record ID: ");

        Plates newPlate = new();

        try
        {
            newPlate = _stockService.GetById(plateId);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("\n===========\n");
        int reciept = ReadInt("Enter the quantity of records: ");

        Console.WriteLine("\n===========\n");
        Console.WriteLine("Enter a reason: ");
        string? reason = Console.ReadLine() ?? "";

        try
        {
            _stockService.Reciept(plateId, reciept, reason);

            Console.WriteLine("Stock receipt added successfully!");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.ReadKey();
    }
    // Writes off units of a record from stock.
    private void WriteOffPlate()
    {
        Console.Clear();
        Console.WriteLine("=== Write Off Records ===");
        ShowAll();
        int plateId = ReadInt("Enter a record ID: ");

        Plates newPlate = new();

        try
        {
            newPlate = _stockService.GetById(plateId);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("\n===========\n");
        int whiteOff = ReadInt("Enter the quantity of records: ");

        Console.WriteLine("\n===========\n");
        Console.WriteLine("Enter a reason: ");
        string? reason = Console.ReadLine() ?? "";

        try
        {
            _stockService.WriteOff(plateId, whiteOff, reason);

            Console.WriteLine("Records written off successfully.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.ReadKey();
    }

    // Sells a selected record to a customer.
    private void SalePlate()
    {
        Console.Clear();
        Console.WriteLine("=== Sell Record ===");
        ShowAll();
        int plateId = ReadInt("Enter a record ID: ");

        Plates newPlate = new();

        try
        {
            newPlate = _saleService.GetById(plateId);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("\n===========\n");
        Console.WriteLine("Enter a customer ID:");
        _saleService.PrintCustomers();
        Console.WriteLine();
        int customerId = ReadInt("Customer ID: ");

        Console.WriteLine("\n===========\n");
        int amount = ReadInt("Enter the quantity of records: ");

        try
        {
            _saleService.Sale(plateId, customerId, amount);

            Console.WriteLine("Records sold successfully!");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.ReadKey();
    }

    // Displays the stock movement history.
    private void PrintStockMovement()
    {
        var stock = _stockService.GetStockMovement();

        if (stock == null || stock.Count == 0)
        {
            Console.WriteLine("The list is empty.");
            return;
        }

        Console.WriteLine(new string('-', 109));
        Console.WriteLine(
            $"| {"ID",3} | {"Record",-24} | {"Change",6} | " +
            $"{"Type",-12} | {"Date",-10} | {"Reason",-35} |");
        Console.WriteLine(new string('-', 109));

        foreach (var s in stock)
        {
            string quantityChangeStr = s.QuantityChange > 0 ? $"+{s.QuantityChange}" : s.QuantityChange.ToString();
            string plateTitle = s.Plate?.Title ?? "—";
            string reason = s.Reason ?? "—";

            if (plateTitle.Length > 24)
                plateTitle = plateTitle[..23] + "…";

            if (reason.Length > 35)
                reason = reason[..34] + "…";

            string type = s.MovementType switch
            {
                StockMovementType.Receipt => "Receipt",
                StockMovementType.Sale => "Sale",
                StockMovementType.WriteOff => "Write-off",
                _ => "Unknown"
            };

            Console.WriteLine(
                $"| {s.Id,3} | {plateTitle,-24} | {quantityChangeStr,6} | " +
                $"{type,-12} | {s.CreatedAt:dd.MM.yyyy} | {reason,-35} |");
        }

        Console.WriteLine(new string('-', 109));
        Console.ReadKey();
    }

    // Displays the sales history.
    private void PrintSales()
    {
        var sales = _saleService.GetSale();

        if (sales == null || sales.Count == 0)
        {
            Console.WriteLine("The sales list is empty.");
            return;
        }

        Console.WriteLine(new string('-', 72));
        Console.WriteLine(
            $"| {"ID",3} | {"Customer",-30} | {"Sale date",-12} | {"Amount",14} |");
        Console.WriteLine(new string('-', 72));

        foreach (var s in sales)
        {
            string customerName = s.Customer.Name.Length > 30
                ? s.Customer.Name[..29] + "…"
                : s.Customer.Name;

            Console.WriteLine(
                $"| {s.Id,3} | {customerName,-30} | " +
                $"{s.SaleDate.ToString("dd.MM.yyyy"),-12} | {s.TotalAmount,14:N2} |");
        }

        Console.WriteLine(new string('-', 72));
        Console.ReadKey();
    }

    // Reads an integer from the console.
    private int ReadInt(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (int.TryParse(Console.ReadLine(), out int value))
                return value;

            Console.WriteLine("Invalid number. Please try again.");
        }
    }
    // Reads a decimal value from the console.
    private decimal ReadDecimal(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (decimal.TryParse(Console.ReadLine(), out decimal value))
                return value;

            Console.WriteLine("Invalid price. Please try again.");
        }
    }
}
