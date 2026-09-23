using MusicStore.Data;
using MusicStore.Models;
using Microsoft.EntityFrameworkCore;

namespace MusicStore.Services;

public enum ReferenceType
{
    Artist = 1,
    Genre = 2,
    Publisher = 3
}

public class ReferenceService
{
    private readonly AuthorizationService _authorization;

    // Initializes reference-data operations and authorization checks.
    public ReferenceService(AuthorizationService authorization)
    {
        _authorization = authorization;
    }

    // Returns all entries for a reference-data type.
    public List<(int Id, string Name)> GetAll(ReferenceType type)
    {
        using var db = new ApplicationContext();
        return type switch
        {
            ReferenceType.Artist => db.Artists.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name }).AsEnumerable().Select(x => (x.Id, x.Name)).ToList(),
            ReferenceType.Genre => db.Genres.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name }).AsEnumerable().Select(x => (x.Id, x.Name)).ToList(),
            ReferenceType.Publisher => db.Publishers.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name }).AsEnumerable().Select(x => (x.Id, x.Name)).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    // Adds a unique reference-data entry.
    public void Add(ReferenceType type, string name)
    {
        _authorization.Require(UserRoles.Administrator, UserRoles.Manager);
        name = ValidateName(type, name);
        using var db = new ApplicationContext();
        EnsureUnique(db, type, name);

        switch (type)
        {
            case ReferenceType.Artist: db.Artists.Add(new Artists { Name = name }); break;
            case ReferenceType.Genre: db.Genres.Add(new Genres { Name = name }); break;
            case ReferenceType.Publisher: db.Publishers.Add(new Publisher { Name = name }); break;
            default: throw new ArgumentOutOfRangeException(nameof(type));
        }
        db.SaveChanges();
    }

    // Renames an existing reference-data entry.
    public void Rename(ReferenceType type, int id, string name)
    {
        _authorization.Require(UserRoles.Administrator, UserRoles.Manager);
        name = ValidateName(type, name);
        using var db = new ApplicationContext();
        EnsureUnique(db, type, name, id);

        switch (type)
        {
            case ReferenceType.Artist:
                (db.Artists.Find(id) ?? throw new ArgumentException("Artist not found.")).Name = name;
                break;
            case ReferenceType.Genre:
                (db.Genres.Find(id) ?? throw new ArgumentException("Genre not found.")).Name = name;
                break;
            case ReferenceType.Publisher:
                (db.Publishers.Find(id) ?? throw new ArgumentException("Publisher not found.")).Name = name;
                break;
            default: throw new ArgumentOutOfRangeException(nameof(type));
        }
        db.SaveChanges();
    }

    // Deletes an unused reference-data entry.
    public void Delete(ReferenceType type, int id)
    {
        _authorization.Require(UserRoles.Administrator);
        using var db = new ApplicationContext();

        bool inUse = type switch
        {
            ReferenceType.Artist => db.Plates.Any(p => p.ArtistId == id),
            ReferenceType.Genre => db.Plates.Any(p => p.GenreId == id) || db.Promotions.Any(p => p.GenreId == id),
            ReferenceType.Publisher => db.Plates.Any(p => p.PublisherId == id),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        if (inUse)
            throw new ArgumentException("The entry is in use and cannot be deleted.");

        switch (type)
        {
            case ReferenceType.Artist:
                db.Artists.Remove(db.Artists.Find(id) ?? throw new ArgumentException("Artist not found."));
                break;
            case ReferenceType.Genre:
                db.Genres.Remove(db.Genres.Find(id) ?? throw new ArgumentException("Genre not found."));
                break;
            case ReferenceType.Publisher:
                db.Publishers.Remove(db.Publishers.Find(id) ?? throw new ArgumentException("Publisher not found."));
                break;
        }
        db.SaveChanges();
    }

    // Validates and normalizes a reference-data name.
    private static string ValidateName(ReferenceType type, string name)
    {
        name = name.Trim();
        int maxLength = type == ReferenceType.Genre ? 100 : 150;
        if (name.Length == 0)
            throw new ArgumentException("Name cannot be empty.");
        if (name.Length > maxLength)
            throw new ArgumentException($"Name cannot exceed {maxLength} characters.");
        return name;
    }

    // Ensures that a reference-data name is unique.
    private static void EnsureUnique(
        ApplicationContext db, ReferenceType type, string name, int? excludedId = null)
    {
        bool exists = type switch
        {
            ReferenceType.Artist => db.Artists.Any(x => x.Name == name && x.Id != excludedId),
            ReferenceType.Genre => db.Genres.Any(x => x.Name == name && x.Id != excludedId),
            ReferenceType.Publisher => db.Publishers.Any(x => x.Name == name && x.Id != excludedId),
            _ => false
        };
        if (exists)
            throw new ArgumentException("An entry with this name already exists.");
    }
}
