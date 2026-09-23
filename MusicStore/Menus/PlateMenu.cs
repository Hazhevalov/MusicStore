using MusicStore.Models;
using MusicStore.Services;
using Microsoft.IdentityModel.Tokens;
using System.Numerics;

namespace MusicStore.Menus;

public class PlateMenu
{
    private readonly PlateService _plateService;
    private readonly SearchMenu _searchMenu;
    private readonly AuthorizationService _authorization;

    // Initializes the record menu and its dependencies.
    public PlateMenu(
        PlateService plateService,
        SearchMenu searchMenu,
        AuthorizationService authorization)
    {
        _plateService = plateService;
        _searchMenu = searchMenu;
        _authorization = authorization;
    }

    // Displays record management actions.
    public void Show()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("=== Records ===");
            Console.WriteLine("1. List records");
            if (_authorization.IsInRole(UserRoles.Administrator, UserRoles.Manager))
            {
                Console.WriteLine("2. Add record");
                Console.WriteLine("4. Edit record");
            }
            if (_authorization.IsInRole(UserRoles.Administrator))
                Console.WriteLine("3. Archive record");
            Console.WriteLine("5. Search records");
            Console.WriteLine("0. Back");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAll();
                    Console.ReadKey();
                    break;

                case "2" when _authorization.IsInRole(UserRoles.Administrator, UserRoles.Manager):
                    AddPlate();
                    break;

                case "3" when _authorization.IsInRole(UserRoles.Administrator):
                    DeletePlate();
                    break;

                case "4" when _authorization.IsInRole(UserRoles.Administrator, UserRoles.Manager):
                    ChangePlate();
                    break;

                case "5":
                    _searchMenu.Show();
                    break;

                case "0":
                    return;
            }
        }
    }

    // Displays all active records.
    private void ShowAll()
    {
        var plates = _plateService.GetAll();

        if (plates.Count == 0)
        {
            Console.WriteLine("The record list is empty.");
            return;
        }
        Console.WriteLine(new string('-', 120));
        Console.WriteLine(
            $"| {"ID",3} | {"Title",-18} | {"Artist",-15} | " +
            $"{"Publisher",-14} | {"Genre",-9} | {"Tracks",5} | " +
            $"{"Year",4} | {"Cost",8} | {"Price",8} | {"Qty",5} |");
        Console.WriteLine(new string('-', 120));

        foreach (var plate in plates)
        {
            string title = plate.Title.Length > 18
                ? plate.Title[..17] + "…"
                : plate.Title;
            string artist = plate.Artist.Name.Length > 15
                ? plate.Artist.Name[..14] + "…"
                : plate.Artist.Name;
            string publisher = plate.Publisher.Name.Length > 14
                ? plate.Publisher.Name[..13] + "…"
                : plate.Publisher.Name;
            string genre = plate.Genre.Name.Length > 9
                ? plate.Genre.Name[..8] + "…"
                : plate.Genre.Name;

            Console.WriteLine(
                $"| {plate.Id,3} | {title,-18} | {artist,-15} | " +
                $"{publisher,-14} | {genre,-9} | {plate.TrackCount,5} | " +
                $"{plate.ReleaseYear,4} | {plate.CostPrise,8:N2} | " +
                $"{plate.SalePrise,8:N2} | {plate.Quantity,5} |");
        }
        Console.WriteLine(new string('-', 120));
    }

    // Collects data and adds a new record.
    private void AddPlate()
    {
        Console.Clear();

        Console.WriteLine("=== Add Record ===");

        Console.Write("Enter a title: ");
        string title = Console.ReadLine() ?? "";
        Console.WriteLine("\n===========\n");

        Console.WriteLine("Select an artist ID from the list.");
        _plateService.PrintArtists();
        Console.WriteLine();
        int artistId = ReadInt("Artist ID: ");
        Console.WriteLine("\n===========\n");

        Console.WriteLine("Select a publisher ID from the list.");
        _plateService.PrintPublishers();
        Console.WriteLine();
        int publisherId = ReadInt("Publisher ID: ");
        Console.WriteLine("\n===========\n");

        Console.WriteLine("Select a genre ID from the list.");
        _plateService.PrintGenres();
        Console.WriteLine();
        int genreId = ReadInt("Genre ID: ");
        Console.WriteLine("\n===========\n");

        int trackCount = ReadInt("Track count: ");
        Console.WriteLine("\n===========\n");

        int releaseYear = ReadInt("Release year: ");
        Console.WriteLine("\n===========\n");

        decimal costPrice = ReadDecimal("Cost price: ");
        Console.WriteLine("\n===========\n");

        decimal sellPrice = ReadDecimal("Sale price: ");
        Console.WriteLine("\n===========\n");

        int quantity = ReadInt("Quantity in stock: ");
        Console.WriteLine("\n===========\n");

        Plates plate = new()
        {
            Title = title,
            ArtistId = artistId,
            PublisherId = publisherId,
            GenreId = genreId,
            TrackCount = trackCount,
            ReleaseYear = releaseYear,
            CostPrise = costPrice,
            SalePrise = sellPrice,
            Quantity = quantity
        };

        try
        {
            _plateService.Add(plate);

            Console.WriteLine("Record added successfully!");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.ReadKey();
    }
    // Archives a record selected by ID.
    private void DeletePlate()
    {
        Console.Clear();
        Console.WriteLine("=== Archive Record ===");
        ShowAll();
        int id = ReadInt("Enter the record ID to archive: ");

        try
        {
            _plateService.Delete(_plateService.GetById(id));

            Console.WriteLine("Record archived successfully!");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.ReadKey();
    }
    // Updates an existing record selected by ID.
    private void ChangePlate()
    {
        Console.Clear();
        Console.WriteLine("=== Edit Record ===");
        ShowAll();
        int id = ReadInt("Enter the record ID to edit: ");

        Plates newPlate = new();

        try
        {
            newPlate = _plateService.GetById(id);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.ReadKey();
            return;
        }

        Console.Write("Enter a title: ");
        string title = Console.ReadLine() ?? "";
        Console.WriteLine("\n===========\n");

        Console.WriteLine("Select an artist ID from the list.");
        _plateService.PrintArtists();
        Console.WriteLine();
        int artistId = ReadInt("Artist ID: ");
        Console.WriteLine("\n===========\n");

        Console.WriteLine("Select a publisher ID from the list.");
        _plateService.PrintPublishers();
        Console.WriteLine();
        int publisherId = ReadInt("Publisher ID: ");
        Console.WriteLine("\n===========\n");

        Console.WriteLine("Select a genre ID from the list.");
        _plateService.PrintGenres();
        Console.WriteLine();
        int genreId = ReadInt("Genre ID: ");
        Console.WriteLine("\n===========\n");

        int trackCount = ReadInt("Track count: ");
        Console.WriteLine("\n===========\n");

        int releaseYear = ReadInt("Release year: ");
        Console.WriteLine("\n===========\n");

        decimal costPrice = ReadDecimal("Cost price: ");
        Console.WriteLine("\n===========\n");

        decimal sellPrice = ReadDecimal("Sale price: ");
        Console.WriteLine("\n===========\n");

        newPlate.Title = title;
        newPlate.ArtistId = artistId;
        newPlate.PublisherId = publisherId;
        newPlate.GenreId = genreId;
        newPlate.TrackCount = trackCount;
        newPlate.ReleaseYear = releaseYear;
        newPlate.CostPrise = costPrice;
        newPlate.SalePrise = sellPrice;

        try
        {
            _plateService.Change(newPlate);

            Console.WriteLine("Record updated successfully!");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
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
