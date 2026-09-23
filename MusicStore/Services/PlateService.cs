using MusicStore.Data;
using MusicStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace MusicStore.Services;

public class PlateService
{
    private readonly AuthorizationService _authorization;

    // Initializes record operations and authorization checks.
    public PlateService(AuthorizationService authorization)
    {
        _authorization = authorization;
    }

    // Returns all active records with reference data.
    public List<Plates> GetAll()
    {
        using var db = new ApplicationContext();

        return db.Plates.Where(p => !p.IsArchived)
        .Include(p => p.Artist)
        .Include(p => p.Publisher)
        .Include(p => p.Genre)
        .AsNoTracking()
        .ToList();
    }
    // Returns an active record by ID.
    public Plates GetById(int id)
    {
        using var db = new ApplicationContext();

        return db.Plates.AsNoTracking().FirstOrDefault(p => p.Id == id && !p.IsArchived)
            ?? throw new ArgumentException("Record not found."); ;
    }

    // Adds a validated record and its opening stock movement.
    public void Add(Plates plate)
    {
        _authorization.Require(UserRoles.Administrator, UserRoles.Manager);
        using var db = new ApplicationContext();

        ValidatePlate(plate, allowQuantity: true);

        if (!db.Artists.Any(a => a.Id == plate.ArtistId))
            throw new ArgumentException("Artist not found.");

        if (!db.Publishers.Any(p => p.Id == plate.PublisherId))
            throw new ArgumentException("Publisher not found.");

        if (!db.Genres.Any(g => g.Id == plate.GenreId))
            throw new ArgumentException("Genre not found.");

        db.Plates.Add(plate);
        if (plate.Quantity > 0)
        {
            plate.StockMovements.Add(new StockMovement
            {
                QuantityChange = plate.Quantity,
                MovementType = StockMovementType.Receipt,
                CreatedAt = DateOnly.FromDateTime(DateTime.Today),
                Reason = "Opening stock"
            });
        }
        db.SaveChanges();
    }

    // Archives an existing record.
    public void Delete(Plates plate)
    {
        _authorization.Require(UserRoles.Administrator);
        using var db = new ApplicationContext();

        Plates existing = db.Plates.FirstOrDefault(a => a.Id == plate.Id)
            ?? throw new ArgumentException("Record not found.");
        existing.IsArchived = true;
        db.SaveChanges();
    }
    // Updates an existing record after validation.
    public void Change(Plates plate)
    {
        _authorization.Require(UserRoles.Administrator, UserRoles.Manager);
        using var db = new ApplicationContext();

        ValidatePlate(plate, allowQuantity: false);

        if (!db.Artists.Any(a => a.Id == plate.ArtistId))
            throw new ArgumentException("Artist not found.");

        if (!db.Publishers.Any(p => p.Id == plate.PublisherId))
            throw new ArgumentException("Publisher not found.");

        if (!db.Genres.Any(g => g.Id == plate.GenreId))
            throw new ArgumentException("Genre not found.");

        Plates existing = db.Plates.FirstOrDefault(p => p.Id == plate.Id && !p.IsArchived)
            ?? throw new ArgumentException("Record not found.");
        existing.Title = plate.Title.Trim();
        existing.ArtistId = plate.ArtistId;
        existing.PublisherId = plate.PublisherId;
        existing.GenreId = plate.GenreId;
        existing.TrackCount = plate.TrackCount;
        existing.ReleaseYear = plate.ReleaseYear;
        existing.CostPrise = plate.CostPrise;
        existing.SalePrise = plate.SalePrise;
        db.SaveChanges();
    }

    // Validates record fields before saving.
    private static void ValidatePlate(Plates plate, bool allowQuantity)
    {
        if (string.IsNullOrWhiteSpace(plate.Title))
            throw new ArgumentException("Title cannot be empty.");
        if (plate.Title.Trim().Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.");
        if (plate.TrackCount <= 0)
            throw new ArgumentException("Track count must be greater than zero.");
        if (plate.ReleaseYear < 1877 || plate.ReleaseYear > 2100)
            throw new ArgumentException("Invalid release year.");
        if (plate.CostPrise < 0 || plate.SalePrise < 0)
            throw new ArgumentException("Price cannot be negative.");
        if (allowQuantity && plate.Quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");
        plate.Title = plate.Title.Trim();
    }
    // Prints all genres available for selection.
    public void PrintGenres()
    {
        using var db = new ApplicationContext();

        var query = db.Genres.ToList();
        Console.WriteLine(new string('-', 35));
        Console.WriteLine($"| {"ID",3} | {"Genre",-25} |");
        Console.WriteLine(new string('-', 35));

        foreach (var g in query)
        {
            string name = g.Name.Length > 25
                ? g.Name[..24] + "…"
                : g.Name;

            Console.WriteLine($"| {g.Id,3} | {name,-25} |");
        }

        Console.WriteLine(new string('-', 35));
    }
    // Prints all publishers available for selection.
    public void PrintPublishers()
    {
        using var db = new ApplicationContext();

        var query = db.Publishers.ToList();
        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"| {"ID",3} | {"Publisher",-30} |");
        Console.WriteLine(new string('-', 40));

        foreach (var g in query)
        {
            string name = g.Name.Length > 30
                ? g.Name[..29] + "…"
                : g.Name;

            Console.WriteLine($"| {g.Id,3} | {name,-30} |");
        }

        Console.WriteLine(new string('-', 40));
    }
    // Prints all artists available for selection.
    public void PrintArtists()
    {
        using var db = new ApplicationContext();

        var query = db.Artists.ToList();
        Console.WriteLine(new string('-', 45));
        Console.WriteLine($"| {"ID",3} | {"Artist",-35} |");
        Console.WriteLine(new string('-', 45));

        foreach (var g in query)
        {
            string name = g.Name.Length > 35
                ? g.Name[..34] + "…"
                : g.Name;

            Console.WriteLine($"| {g.Id,3} | {name,-35} |");
        }

        Console.WriteLine(new string('-', 45));
    }
    // Finds and prints records whose titles contain the search text.
    public void GetByTitle(string title)
    {
        using var db = new ApplicationContext();

        if (title == "")
            throw new ArgumentException("Search text cannot be empty.");

        var plates = db.Plates
            .AsNoTracking()
            .Where(a => !a.IsArchived && a.Title.Contains(title.Trim()))
            .ToList();

        if (plates.Count == 0)
        {
            Console.WriteLine("No matches found!");
            return;
        }

        Console.WriteLine("Matches:");
        PrintPlateSearchResult(plates);
    }
    // Finds and prints records in a selected genre.
    public void GetByGenre(int genreId)
    {
        using var db = new ApplicationContext();

        if (!db.Genres.Any(g => g.Id == genreId))
            throw new ArgumentException("Genre not found.");

        var genre = db.Genres.FirstOrDefault(a => a.Id == genreId);

        var plates = db.Plates
            .AsNoTracking()
            .Where(a => !a.IsArchived && a.GenreId == genreId)
            .ToList();

        if (plates.Count == 0)
        {
            Console.WriteLine("No matches found!");
            return;
        }

        Console.WriteLine("Matches:");
        PrintPlateSearchResult(plates);
    }
    
    // Finds and prints records by a selected artist.
    public void GetByArtist(int artistId)
    {
        using var db = new ApplicationContext();

        if (!db.Artists.Any(g => g.Id == artistId))
            throw new ArgumentException("Artist not found.");

        var artist = db.Artists.FirstOrDefault(a => a.Id == artistId);

        var plates = db.Plates
            .AsNoTracking()
            .Where(a => !a.IsArchived && a.ArtistId == artistId)
            .ToList();

        if (plates.Count == 0)
        {
            Console.WriteLine("No matches found!");
            return;
        }

        Console.WriteLine("Matches:");
        PrintPlateSearchResult(plates);
    }

    // Prints record search results in a table.
    private static void PrintPlateSearchResult(List<Plates> plates)
    {
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"| {"ID",3} | {"Title",-40} |");
        Console.WriteLine(new string('-', 50));

        foreach (var plate in plates)
        {
            string title = plate.Title.Length > 40
                ? plate.Title[..39] + "…"
                : plate.Title;

            Console.WriteLine($"| {plate.Id,3} | {title,-40} |");
        }

        Console.WriteLine(new string('-', 50));
    }

    // Returns the newest record releases.
    public List<Plates> GetNewReleases(int count = 10)
    {
        using var db = new ApplicationContext();

        return db.Plates.Where(p => !p.IsArchived)
            .AsNoTracking()
            .Include(p => p.Artist)
            .Include(p => p.Publisher)
            .Include(p => p.Genre)
            .OrderByDescending(p => p.ReleaseYear)
            .ThenByDescending(p => p.Id)
            .Take(count)
            .ToList();
    }
    // Converts a reporting-period option into a date range.
    private (DateOnly Start, DateOnly End) GetDates(int period)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        DateOnly weekStart = today.AddDays(
            -(((int)today.DayOfWeek + 6) % 7));

        return period switch
        {
            // Today.
            1 => (today, today.AddDays(1)),

            // Current week: Monday through Sunday.
            2 => (weekStart, weekStart.AddDays(7)),

            // Current month.
            3 => (
                new DateOnly(today.Year, today.Month, 1),
                new DateOnly(today.Year, today.Month, 1)
                    .AddMonths(1)
            ),

            // Current year.
            4 => (
                new DateOnly(today.Year, 1, 1),
                new DateOnly(today.Year + 1, 1, 1)
            ),

            _ => throw new ArgumentException(
                "Invalid period")
        };
    }

    // Returns the best-selling records for a period.
    public List<(string Name, int Quantity)> GetPopularPlates(
        int period,
        int count = 10)
    {
        using var db = new ApplicationContext();

        var (start, end) = GetDates(period);

        var result = db.SaleItems
            .Where(s =>
                s.Sale.SaleDate >= start &&
                s.Sale.SaleDate < end)
            .GroupBy(s => new
            {
                s.PlateId,
                s.Plate.Title
            })
            .Select(g => new
            {
                Name = g.Key.Title,
                Id = g.Key.PlateId,
                Quantity = g.Sum(s => s.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .ThenBy(x => x.Id)
            .Take(count)
            .ToList();

        return result
            .Select(x => (x.Name, x.Quantity))
            .ToList();
    }

    // Returns the best-selling artists for a period.
    public List<(string Name, int Quantity)> GetPopularArtists(
        int period,
        int count = 10)
    {
        using var db = new ApplicationContext();

        var (start, end) = GetDates(period);

        var result = db.SaleItems
            .Where(s =>
                s.Sale.SaleDate >= start &&
                s.Sale.SaleDate < end)
            .GroupBy(s => new
            {
                s.Plate.ArtistId,
                s.Plate.Artist.Name
            })
            .Select(g => new
            {
                Name = g.Key.Name,
                Id = g.Key.ArtistId,
                Quantity = g.Sum(s => s.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .ThenBy(x => x.Id)
            .Take(count)
            .ToList();

        return result
            .Select(x => (x.Name, x.Quantity))
            .ToList();
    }

    // Returns the best-selling genres for a period.
    public List<(string Name, int Quantity)> GetPopularGenres(
        int period,
        int count = 10)
    {
        using var db = new ApplicationContext();

        var (start, end) = GetDates(period);

        var result = db.SaleItems
            .Where(s =>
                s.Sale.SaleDate >= start &&
                s.Sale.SaleDate < end)
            .GroupBy(s => new
            {
                s.Plate.GenreId,
                s.Plate.Genre.Name
            })
            .Select(g => new
            {
                Name = g.Key.Name,
                Id = g.Key.GenreId,
                Quantity = g.Sum(s => s.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .ThenBy(x => x.Id)
            .Take(count)
            .ToList();

        return result
            .Select(x => (x.Name, x.Quantity))
            .ToList();
    }


}
