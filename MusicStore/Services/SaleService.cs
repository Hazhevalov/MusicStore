using MusicStore.Data;
using MusicStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Data;

namespace MusicStore.Services;

public class SaleService
{
    private readonly InventoryService _inventoryService;
    private readonly AuthorizationService _authorization;

    // Initializes sales operations and related services.
    public SaleService(
        InventoryService inventoryService,
        AuthorizationService authorization)
    {
        _inventoryService = inventoryService;
        _authorization = authorization;
    }

    // Returns all active records available for sale.
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

    // Sells an available quantity of a record to a customer.
    public void Sale(int plateId, int customerId, int amount)
    {
        _authorization.Require(
            UserRoles.Administrator, UserRoles.Manager, UserRoles.Seller);

        if (amount <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        using var db = new ApplicationContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        _inventoryService.ExpireReservations(db, today);

        Plates plate = db.Plates
            .FirstOrDefault(p => p.Id == plateId && !p.IsArchived)
            ?? throw new ArgumentException("Record not found.");

        int available = _inventoryService.GetAvailableQuantity(db, plate, today);
        if (available < amount)
            throw new ArgumentException(
                $"Insufficient available stock. Available: {available} units.");

        Customers customer = db.Customers
            .FirstOrDefault(c => c.Id == customerId)
            ?? throw new ArgumentException("Customer not found.");

        CreateSale(db, plate, customer, amount, today);
        _inventoryService.SaveChanges(db);
        transaction.Commit();
    }

    // Sells some or all of the quantity held by a reservation.
    public void SaleReservation(int reservationId, int amount)
    {
        _authorization.Require(
            UserRoles.Administrator, UserRoles.Manager, UserRoles.Seller);

        if (amount <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        using var db = new ApplicationContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        _inventoryService.ExpireReservations(db, today);

        Reservations reservation = db.Reservations
            .Include(r => r.Plate)
            .Include(r => r.Customer)
            .FirstOrDefault(r => r.Id == reservationId)
            ?? throw new ArgumentException("Reservation not found.");

        if (reservation.Status != ReservationStatus.Active || reservation.ExpiresAt < today)
            throw new ArgumentException("The reservation is not active.");

        int remaining = reservation.Quantity - reservation.FulfilledQuantity;
        if (amount > remaining)
            throw new ArgumentException($"The reservation has {remaining} units remaining.");
        if (reservation.Plate.Quantity < amount)
            throw new InvalidOperationException("Physical stock is lower than the reserved quantity.");

        CreateSale(db, reservation.Plate, reservation.Customer, amount, today);
        reservation.FulfilledQuantity += amount;
        if (reservation.FulfilledQuantity == reservation.Quantity)
            reservation.Status = ReservationStatus.Completed;

        _inventoryService.SaveChanges(db);
        transaction.Commit();
    }

    // Creates sale records, applies discounts, and updates stock totals.
    private void CreateSale(
        ApplicationContext db,
        Plates plate,
        Customers customer,
        int amount,
        DateOnly today)
    {
        decimal customerDiscount = GetCustomerDiscount(customer.TotalSpent);

        decimal promotionDiscount = GetPromotionDiscount(db, plate);

        decimal discountPercent = Math.Max(
            customerDiscount,
            promotionDiscount);

        decimal unitPrice = plate.SalePrise;

        decimal totalAmount = Math.Round(
            unitPrice * amount *
            (1 - discountPercent / 100m),
            2,
            MidpointRounding.AwayFromZero);

        plate.Quantity -= amount;

        customer.TotalSpent += totalAmount;

        Sales sale = new()
        {
            CustomerId = customer.Id,
            SaleDate = today,
            TotalAmount = totalAmount
        };

        SaleItems saleItem = new()
        {
            PlateId = plate.Id,
            Quantity = amount,
            UnitPrise = unitPrice,
            DiscountPercent = discountPercent
        };

        sale.SaleItems.Add(saleItem);
        db.Sales.Add(sale);

        StockMovement movement = new()
        {
            PlateId = plate.Id,
            QuantityChange = -amount,
            MovementType = StockMovementType.Sale,
            CreatedAt = today,
            Reason = "Sale"
        };
        db.StockMovements.Add(movement);

    }

    // Calculates a loyalty discount from the customer's total spending.
    private decimal GetCustomerDiscount(decimal totalSpent)
    {
        return totalSpent switch
        {
            >= 10000 => 10m,
            >= 5000 => 5m,
            >= 1000 => 3m,
            _ => 0m
        };
    }

    // Returns the highest active promotion discount for a record.
    private decimal GetPromotionDiscount(
        ApplicationContext db,
        Plates plate)
    {
        DateOnly today =
            DateOnly.FromDateTime(DateTime.Today);

        return db.Promotions
            .Where(p =>
                p.StartDate <= today &&
                p.EndDate >= today &&
                (
                    p.GenreId == plate.GenreId ||
                    p.PromotionPlates.Any(pp =>
                        pp.PlateId == plate.Id)
                ))
            .Select(p => (decimal?)p.DiscountPercent)
            .Max() ?? 0m;
    }

    // Prints customers available for a sale.
    public void PrintCustomers()
    {
        using var db = new ApplicationContext();

        var query = db.Customers.ToList();
        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"| {"ID",3} | {"Customer",-30} |");
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

    // Returns all sales with customer details.
    public List<Sales> GetSale()
    {
        using var db = new ApplicationContext();
        var sales = db.Sales
            .Include(p => p.Customer)
            .AsNoTracking()
            .ToList();

        return sales;
    }

}
