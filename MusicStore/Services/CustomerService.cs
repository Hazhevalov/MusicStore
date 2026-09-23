using MusicStore.Data;
using MusicStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MusicStore.Services;

public class CustomerService
{
    private readonly InventoryService _inventoryService;
    private readonly AuthorizationService _authorization;

    // Initializes customer operations and authorization checks.
    public CustomerService(
        InventoryService inventoryService,
        AuthorizationService authorization)
    {
        _inventoryService = inventoryService;
        _authorization = authorization;
    }

    // Returns all customers.
    public List<Customers> GetCustomers()
    {
        using var db = new ApplicationContext();

        return db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToList();
    }
    // Adds a customer after validating the name.
    public void Add(string name)
    {
        _authorization.Require(
            UserRoles.Administrator, UserRoles.Manager, UserRoles.Seller);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Invalid name.");
        if (name.Trim().Length > 150)
            throw new ArgumentException("Name cannot exceed 150 characters.");

        using var db = new ApplicationContext();

        Customers customer = new()
        {
            Name = name.Trim(),
            TotalSpent = 0,
            RegisteredAt = DateOnly.FromDateTime(DateTime.Today)
        };

        db.Customers.Add(customer);
        db.SaveChanges();
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
            .OrderBy(p => p.Id)
            .ToList();
    }

    // Returns an active record by ID.
    public Plates GetById(int id)
    {
        using var db = new ApplicationContext();

        return db.Plates
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == id && !p.IsArchived)
            ?? throw new ArgumentException("Record not found.");
    }

    // Returns all genres.
    public List<Genres> GetGenres()
    {
        using var db = new ApplicationContext();

        return db.Genres
            .AsNoTracking()
            .OrderBy(g => g.Id)
            .ToList();
    }

    // Returns all promotions with their genres.
    public List<Promotions> GetPromotions()
    {
        using var db = new ApplicationContext();

        return db.Promotions
            .Include(p => p.Genre)
            .AsNoTracking()
            .OrderByDescending(p => p.StartDate)
            .ToList();
    }

    // Adds a validated genre promotion.
    public void AddPromotion(
        string name,
        int genreId,
        decimal discountPercent,
        DateOnly startDate,
        DateOnly endDate)
    {
        _authorization.Require(UserRoles.Administrator, UserRoles.Manager);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Promotion name is required.");
        if (name.Trim().Length > 150)
            throw new ArgumentException("Promotion name cannot exceed 150 characters.");

        if (discountPercent <= 0 || discountPercent > 100)
            throw new ArgumentException(
                "Discount must be between 0 and 100%.");

        if (endDate < startDate)
            throw new ArgumentException(
                "The end date cannot be earlier than the start date.");

        using var db = new ApplicationContext();

        if (!db.Genres.Any(g => g.Id == genreId))
            throw new ArgumentException("Genre not found.");

        Promotions promotion = new()
        {
            Name = name.Trim(),
            GenreId = genreId,
            DiscountPercent = discountPercent,
            StartDate = startDate,
            EndDate = endDate
        };

        db.Promotions.Add(promotion);
        db.SaveChanges();
    }

    // Creates a reservation after checking available stock.
    public void Reserve(
        int customerId,
        int plateId,
        int quantity,
        DateOnly expiresAt)
    {
        _authorization.Require(
            UserRoles.Administrator, UserRoles.Manager, UserRoles.Seller);
        if (quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        DateOnly today =
            DateOnly.FromDateTime(DateTime.Today);

        if (expiresAt < today)
            throw new ArgumentException(
                "The reservation end date cannot be in the past.");

        using var db = new ApplicationContext();

        // Keep stock validation and reservation creation in one transaction.
        using var transaction = db.Database.BeginTransaction(
            IsolationLevel.Serializable);
        _inventoryService.ExpireReservations(db, today);

        if (!db.Customers.Any(c => c.Id == customerId))
            throw new ArgumentException("Customer not found.");

        Plates plate = db.Plates
            .FirstOrDefault(p => p.Id == plateId && !p.IsArchived)
            ?? throw new ArgumentException("Record not found.");

        int available = _inventoryService.GetAvailableQuantity(db, plate, today);

        if (available < quantity)
            throw new ArgumentException(
                $"Insufficient stock. Available: {available} units.");

        Reservations reservation = new()
        {
            CustomerId = customerId,
            PlatesId = plateId,
            Quantity = quantity,
            FulfilledQuantity = 0,
            ReservedAt = today,
            ExpiresAt = expiresAt,
            Status = ReservationStatus.Active
        };

        db.Reservations.Add(reservation);

        _inventoryService.SaveChanges(db);
        transaction.Commit();
    }

    // Returns all reservations and expires overdue entries.
    public List<Reservations> GetReservations()
    {
        using var db = new ApplicationContext();
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        _inventoryService.ExpireReservations(db, today);

        return db.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Plate)
            .AsNoTracking()
            .OrderByDescending(r => r.ReservedAt)
            .ThenByDescending(r => r.Id)
            .ToList();
    }

    // Cancels an active reservation by ID.
    public void CancelReservation(int reservationId)
    {
        _authorization.Require(
            UserRoles.Administrator, UserRoles.Manager, UserRoles.Seller);

        using var db = new ApplicationContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        _inventoryService.ExpireReservations(db, today);

        Reservations reservation = db.Reservations
            .FirstOrDefault(r => r.Id == reservationId)
            ?? throw new ArgumentException("Reservation not found.");

        if (reservation.Status != ReservationStatus.Active)
            throw new ArgumentException("Only an active reservation can be cancelled.");

        reservation.Status = ReservationStatus.Cancelled;
        _inventoryService.SaveChanges(db);
        transaction.Commit();
    }
}
