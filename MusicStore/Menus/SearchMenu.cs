using Exam.Models;
using Exam.Services;
using Microsoft.IdentityModel.Tokens;
using System.Numerics;

namespace Exam.Menus;

public class SearchMenu
{
    private readonly PlateService _plateService;

    // Initializes the record search menu.
    public SearchMenu(PlateService plateService)
    {
        _plateService = plateService;
    }

    // Displays the record search options.
    public void Show()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("=== Record Search ===");
            Console.WriteLine("1. Search by title");
            Console.WriteLine("2. Search by artist");
            Console.WriteLine("3. Search by genre");
            Console.WriteLine("0. Back");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    FindByTitle();
                    break;

                case "2":
                    FindByArtist();
                    break;

                case "3":
                    FindByGenre();
                    break;

                case "0":
                    return;
            }
        }
    }

    // Displays all records in a table.
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

    // Searches for records by title.
    private void FindByTitle()
    {
        Console.Clear();
        Console.WriteLine("=== Search by Title ===");
        Console.WriteLine("Enter a title: ");
        string? title = Console.ReadLine() ?? "";

        try
        {
            _plateService.GetByTitle(title);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.ReadKey();
    }
    // Searches for records by genre.
    private void FindByGenre()
    {
        Console.Clear();
        Console.WriteLine("=== Search by Genre ===");
        _plateService.PrintGenres();
        Console.WriteLine("Select a genre ID: ");
        int genreId = ReadInt("");

        try
        {
            _plateService.GetByGenre(genreId);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.ReadKey();
    }
    // Searches for records by artist.
    private void FindByArtist()
    {
        Console.Clear();
        Console.WriteLine("=== Search by Artist ===");
        _plateService.PrintArtists();
        Console.WriteLine("Select an artist ID: ");
        int artistId = ReadInt("");

        try
        {
            _plateService.GetByArtist(artistId);
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
